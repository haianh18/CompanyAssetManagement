﻿// 1. Update BorrowRequestController
using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.ReturnRequest;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class BorrowRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BorrowRequestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: BorrowRequest/BorrowRequests
        public async Task<IActionResult> BorrowRequests()
        {
            try
            {
                // Lấy tất cả các yêu cầu mượn, bao gồm cả thông tin liên quan
                var requests = await _unitOfWork.BorrowTickets.GetBorrowTicketsWithoutReturn();

                // Ghi log để debug
                Console.WriteLine($"Found {requests.Count()} borrow requests");

                // Đảm bảo tải đầy đủ các đối tượng liên quan
                foreach (var request in requests)
                {
                    // Đảm bảo đối tượng liên quan được tải
                    if (request.BorrowById.HasValue && request.BorrowBy == null)
                    {
                        request.BorrowBy = await _unitOfWork.Users.GetUserByIdAsync(request.BorrowById.Value);
                    }

                    if (request.WarehouseAssetId.HasValue && request.WarehouseAsset == null)
                    {
                        request.WarehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(request.WarehouseAssetId.Value);

                        // Đảm bảo Asset được tải
                        if (request.WarehouseAsset != null && request.WarehouseAsset.AssetId.HasValue && request.WarehouseAsset.Asset == null)
                        {
                            request.WarehouseAsset.Asset = await _unitOfWork.Assets.GetByIdAsync(request.WarehouseAsset.AssetId.Value);
                        }
                    }
                }

                return View(requests);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải danh sách yêu cầu mượn: {ex.Message}";
                return View(new List<BorrowTicket>());
            }
        }

        // GET: BorrowRequest/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);

            // Check if user is eligible for borrowing (no overdue items)
            if (currentUser != null && !await _unitOfWork.BorrowTickets.IsUserEligibleForBorrowing(currentUser.Id))
            {
                TempData["ErrorMessage"] = "Bạn có tài sản đã quá hạn trả. Vui lòng trả lại tài sản trước khi mượn mới.";
                return RedirectToAction("MyBorrowRequests");
            }

            // Get all warehouse assets that are available for borrowing
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithAvailableQuantity();

            // Create SelectList for dropdown in the view
            ViewBag.WarehouseAssets = new SelectList(warehouseAssets, "Id", "Asset.Name");
            ViewBag.CurrentUserId = currentUser?.Id;

            return View();
        }

        // POST: BorrowRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowRequestViewModel model)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate request date range (7-30 days)
                    var minReturnDate = DateTime.Now.AddDays(7);
                    var maxReturnDate = DateTime.Now.AddDays(30);

                    if (model.ReturnDate < minReturnDate)
                    {
                        ModelState.AddModelError("ReturnDate", "Thời gian mượn tối thiểu là 7 ngày.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    if (model.ReturnDate > maxReturnDate)
                    {
                        ModelState.AddModelError("ReturnDate", "Thời gian mượn tối đa là 30 ngày.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    // Get the warehouse asset to check availability
                    var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(model.WarehouseAssetId);
                    if (warehouseAsset == null)
                    {
                        ModelState.AddModelError("", "Tài sản không tồn tại.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    // Check if requested quantity is available
                    int availableQuantity = (warehouseAsset.GoodQuantity ?? 0) - (warehouseAsset.BorrowedGoodQuantity ?? 0) - (warehouseAsset.HandedOverGoodQuantity ?? 0);
                    if (model.Quantity > availableQuantity)
                    {
                        ModelState.AddModelError("Quantity", $"Số lượng yêu cầu vượt quá số lượng hiện có. Hiện tại chỉ còn {availableQuantity} sản phẩm khả dụng.");
                        await PrepareCreateViewBag(currentUser.Id);
                        return View(model);
                    }

                    // Create borrow ticket
                    var borrowTicket = new BorrowTicket
                    {
                        WarehouseAssetId = model.WarehouseAssetId,
                        BorrowById = currentUser.Id,
                        OwnerId = null, // Will be set by warehouse manager during approval
                        Quantity = model.Quantity,
                        ReturnDate = model.ReturnDate,
                        Note = model.Note,
                        ApproveStatus = TicketStatus.Pending,
                        BorrowedAssetStatus = AssetStatus.GOOD, // Only good assets can be borrowed
                        DateCreated = DateTime.Now
                    };

                    await _unitOfWork.BorrowTickets.AddAsync(borrowTicket);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Yêu cầu mượn tài sản đã được tạo thành công và đang chờ phê duyệt.";
                    return RedirectToAction(nameof(MyBorrowRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                    await PrepareCreateViewBag(currentUser.Id);
                }
            }

            await PrepareCreateViewBag(currentUser.Id);
            return View(model);
        }

        private async Task PrepareCreateViewBag(int currentUserId)
        {
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithAvailableQuantity();
            ViewBag.WarehouseAssets = new SelectList(warehouseAssets, "Id", "Asset.Name");
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.MinReturnDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            ViewBag.MaxReturnDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        }

        // GET: BorrowRequest/MyBorrowRequests
        public async Task<IActionResult> MyBorrowRequests()
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _unitOfWork.BorrowTickets.GetBorrowTicketsByUser(currentUser.Id);
            return View(requests);
        }

        // GET: BorrowRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: BorrowRequest/RequestExtension/5
        public async Task<IActionResult> RequestExtension(int id)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowTicket = await _unitOfWork.BorrowTickets.GetBorrowTicketWithExtensions(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Check if user is the borrower
            if (borrowTicket.BorrowById != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền yêu cầu gia hạn cho phiếu mượn này.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if ticket can be extended (not already extended)
            if (borrowTicket.IsExtended)
            {
                TempData["ErrorMessage"] = "Phiếu mượn này đã được gia hạn trước đó. Mỗi phiếu mượn chỉ được gia hạn một lần.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if ticket is approved
            if (borrowTicket.ApproveStatus != TicketStatus.Approved)
            {
                TempData["ErrorMessage"] = "Chỉ có thể gia hạn phiếu mượn đã được phê duyệt.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if ticket has pending extension request
            if (borrowTicket.ExtensionApproveStatus == TicketStatus.Pending)
            {
                TempData["InfoMessage"] = "Yêu cầu gia hạn của bạn đang chờ xét duyệt.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if return date is at least 7 days from now
            if (borrowTicket.ReturnDate < DateTime.Now.AddDays(7))
            {
                TempData["ErrorMessage"] = "Chỉ có thể yêu cầu gia hạn trước thời hạn trả ít nhất 7 ngày.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            var model = new ExtensionRequestViewModel
            {
                BorrowTicketId = borrowTicket.Id,
                AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                OriginalBorrowDate = borrowTicket.DateCreated,
                CurrentReturnDate = borrowTicket.ReturnDate,
                Quantity = borrowTicket.Quantity,
                // Default extension to 30 days from current return date
                NewReturnDate = borrowTicket.ReturnDate?.AddDays(30)
            };

            return View(model);
        }

        // POST: BorrowRequest/RequestExtension
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestExtension(ExtensionRequestViewModel model)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(model.BorrowTicketId);
                    if (borrowTicket == null)
                    {
                        return NotFound();
                    }

                    // Check all the validation rules again in case of form manipulation
                    if (borrowTicket.BorrowById != currentUser.Id ||
                        borrowTicket.IsExtended ||
                        borrowTicket.ApproveStatus != TicketStatus.Approved ||
                        borrowTicket.ExtensionApproveStatus == TicketStatus.Pending ||
                        borrowTicket.ReturnDate < DateTime.Now.AddDays(7))
                    {
                        TempData["ErrorMessage"] = "Không thể gia hạn phiếu mượn này.";
                        return RedirectToAction(nameof(MyBorrowRequests));
                    }

                    // Validate extension date
                    if (model.NewReturnDate <= borrowTicket.ReturnDate)
                    {
                        ModelState.AddModelError("NewReturnDate", "Ngày trả mới phải sau ngày trả hiện tại.");
                        return View(model);
                    }

                    // Validate max extension (30 days from current return date)
                    var maxExtensionDate = borrowTicket.ReturnDate.Value.AddDays(30);
                    if (model.NewReturnDate > maxExtensionDate)
                    {
                        ModelState.AddModelError("NewReturnDate", "Thời gian gia hạn tối đa là 30 ngày từ ngày trả hiện tại.");
                        return View(model);
                    }

                    // Request extension
                    await _unitOfWork.BorrowTickets.RequestExtensionAsync(model.BorrowTicketId, model.NewReturnDate.Value);
                    await _unitOfWork.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Yêu cầu gia hạn đã được gửi và đang chờ xét duyệt.";
                    return RedirectToAction(nameof(MyBorrowRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: BorrowRequest/ReturnAsset/5
        public async Task<IActionResult> ReturnAsset(int id)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Check if user is the borrower
            if (borrowTicket.BorrowById != currentUser.Id)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền trả tài sản này.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if already returned
            if (borrowTicket.IsReturned)
            {
                TempData["ErrorMessage"] = "Tài sản này đã được trả trước đó.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            // Check if approved
            if (borrowTicket.ApproveStatus != TicketStatus.Approved)
            {
                TempData["ErrorMessage"] = "Chỉ có thể trả tài sản đã được phê duyệt mượn.";
                return RedirectToAction(nameof(MyBorrowRequests));
            }

            var model = new ReturnAssetViewModel
            {
                BorrowTicketId = borrowTicket.Id,
                AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                BorrowDate = borrowTicket.DateCreated,
                ScheduledReturnDate = borrowTicket.ReturnDate,
                Quantity = borrowTicket.Quantity,
                IsEarlyReturn = DateTime.Now < borrowTicket.ReturnDate
            };

            return View(model);
        }

        // POST: BorrowRequest/ReturnAsset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnAsset(ReturnAssetViewModel model)
        {
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Process early return
                    var returnTicket = await ProcessEarlyReturnAsync(
                        model.BorrowTicketId,
                        currentUser.Id,
                        model.Notes
                    );

                    TempData["SuccessMessage"] = "Yêu cầu trả tài sản đã được gửi. Vui lòng mang tài sản đến kho để hoàn tất quy trình.";
                    return RedirectToAction(nameof(MyBorrowRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }


        public async Task<ReturnTicket> ProcessEarlyReturnAsync(int borrowTicketId, int returnById, string notes)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null)
                throw new Exception("Borrow ticket not found");

            if (borrowTicket.IsReturned)
                throw new Exception("This borrow ticket has already been returned");

            // Create return ticket for early return (initiated by borrower)
            var returnTicket = new ReturnTicket
            {
                BorrowTicketId = borrowTicketId,
                ReturnById = returnById,
                OwnerId = borrowTicket.OwnerId,
                Quantity = borrowTicket.Quantity,
                Note = notes,
                ApproveStatus = TicketStatus.Pending,
                ReturnRequestDate = DateTime.Now,
                IsEarlyReturn = DateTime.Now < borrowTicket.ReturnDate,
                DateCreated = DateTime.Now
            };

            await _unitOfWork.ReturnTickets.AddAsync(returnTicket);
            await _unitOfWork.SaveChangesAsync();
            return returnTicket;
        }



    }
}