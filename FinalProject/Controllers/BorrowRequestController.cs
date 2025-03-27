// 1. Update BorrowRequestController
using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.ReturnRequest;
using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class BorrowRequestController : Controller
    {
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IReturnTicketService _returnTicketService;
        private readonly IWarehouseAssetService _warehouseAssetService;
        private readonly IUserService _userService;
        private readonly IAssetService _assetService;

        public BorrowRequestController(
            IBorrowTicketService borrowTicketService,
            IReturnTicketService returnTicketService,
            IWarehouseAssetService warehouseAssetService,
            IUserService userService,
            IAssetService assetService)
        {
            _borrowTicketService = borrowTicketService;
            _returnTicketService = returnTicketService;
            _warehouseAssetService = warehouseAssetService;
            _userService = userService;
            _assetService = assetService;
        }

        // GET: BorrowRequest/BorrowRequests
        public async Task<IActionResult> BorrowRequests()
        {
            var requests = await _borrowTicketService.GetBorrowTicketsWithoutReturnAsync();
            return View(requests);
        }

        // GET: BorrowRequest/Create
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);

            // Check if user is eligible for borrowing (no overdue items)
            if (currentUser != null && !await _borrowTicketService.IsUserEligibleForBorrowingAsync(currentUser.Id))
            {
                TempData["ErrorMessage"] = "Bạn có tài sản đã quá hạn trả. Vui lòng trả lại tài sản trước khi mượn mới.";
                return RedirectToAction("MyBorrowRequests");
            }

            // Get all warehouse assets that are available for borrowing
            var warehouseAssets = await _warehouseAssetService.GetBorrowableAssetsAsync();

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
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
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
                    var warehouseAsset = await _warehouseAssetService.GetByIdAsync(model.WarehouseAssetId);
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

                    await _borrowTicketService.AddAsync(borrowTicket);

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
            var warehouseAssets = await _warehouseAssetService.GetBorrowableAssetsAsync();
            ViewBag.WarehouseAssets = new SelectList(warehouseAssets, "Id", "Asset.Name");
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.MinReturnDate = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
            ViewBag.MaxReturnDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        }

        // GET: BorrowRequest/MyBorrowRequests
        public async Task<IActionResult> MyBorrowRequests()
        {
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _borrowTicketService.GetBorrowTicketsByUserAsync(currentUser.Id);
            return View(requests);
        }

        // GET: BorrowRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: BorrowRequest/RequestExtension/5
        public async Task<IActionResult> RequestExtension(int id)
        {
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowTicket = await _borrowTicketService.GetBorrowTicketWithExtensionsAsync(id);
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
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var borrowTicket = await _borrowTicketService.GetByIdAsync(model.BorrowTicketId);
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
                    await _borrowTicketService.RequestExtensionAsync(model.BorrowTicketId, model.NewReturnDate.Value);

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
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowTicket = await _borrowTicketService.GetByIdAsync(id);
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
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Process early return
                    var returnTicket = await _returnTicketService.ProcessEarlyReturnAsync(
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
    }
}