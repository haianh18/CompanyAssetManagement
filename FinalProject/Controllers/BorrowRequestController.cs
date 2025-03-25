using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class BorrowRequestController : Controller
    {
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IWarehouseAssetService _warehouseAssetService;
        private readonly IUserService _userService;
        private readonly IAssetService _assetService;

        public BorrowRequestController(
            IBorrowTicketService borrowTicketService,
            IWarehouseAssetService warehouseAssetService,
            IUserService userService,
            IAssetService assetService)
        {
            _borrowTicketService = borrowTicketService;
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
            // Get all warehouse assets that are available for borrowing
            var warehouseAssets = await _warehouseAssetService.GetAllAsync();

            // Create SelectList for dropdown in the view
            ViewBag.WarehouseAssets = new SelectList(warehouseAssets, "Id", "Asset.Name");

            // Get the current user (in a real app, this would be the logged-in user)
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            ViewBag.CurrentUserId = currentUser?.Id;

            return View();
        }

        // POST: BorrowRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowTicket borrowTicket)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the warehouse asset to check availability
                    var warehouseAsset = await _warehouseAssetService.GetByIdAsync(borrowTicket.WarehouseAssetId.Value);

                    if (warehouseAsset == null)
                    {
                        ModelState.AddModelError("", "Tài sản không tồn tại.");
                        await PrepareCreateViewBag();
                        return View(borrowTicket);
                    }

                    // Check if requested quantity is available
                    if (borrowTicket.Quantity > warehouseAsset.Quantity)
                    {
                        ModelState.AddModelError("Quantity", $"Số lượng yêu cầu vượt quá số lượng hiện có. Hiện tại chỉ còn {warehouseAsset.Quantity} sản phẩm.");
                        await PrepareCreateViewBag();
                        return View(borrowTicket);
                    }

                    // Setup borrowing details
                    borrowTicket.DateCreated = DateTime.Now;
                    borrowTicket.ApproveStatus = TicketStatus.Pending; // Set initial status to Pending

                    // In a real app, the current user would be the borrower
                    if (borrowTicket.BorrowById == null)
                    {
                        var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
                        borrowTicket.BorrowById = currentUser.Id;
                    }

                    // The warehouse manager would typically be the owner
                    if (borrowTicket.OwnerId == null)
                    {
                        // For demo purposes, we'll use the same user
                        borrowTicket.OwnerId = borrowTicket.BorrowById;
                    }

                    // Add the borrow ticket
                    await _borrowTicketService.AddAsync(borrowTicket);

                    TempData["SuccessMessage"] = "Yêu cầu mượn tài sản đã được tạo thành công và đang chờ phê duyệt.";
                    return RedirectToAction(nameof(MyBorrowRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                    await PrepareCreateViewBag();
                }
            }

            await PrepareCreateViewBag();
            return View(borrowTicket);
        }

        private async Task PrepareCreateViewBag()
        {
            var warehouseAssets = await _warehouseAssetService.GetAllAsync();
            ViewBag.WarehouseAssets = new SelectList(warehouseAssets, "Id", "Asset.Name");

            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            ViewBag.CurrentUserId = currentUser?.Id;
        }

        // GET: BorrowRequest/MyBorrowRequests
        public async Task<IActionResult> MyBorrowRequests()
        {
            // For demo purposes, we'll just get all requests
            // In a real app, you would filter by the current user
            var requests = await _borrowTicketService.GetAllAsync();
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

        // GET: BorrowRequest/ApproveBorrowRequest/5
        public async Task<IActionResult> ApproveBorrowRequest(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: BorrowRequest/ApproveBorrowRequest/5
        [HttpPost, ActionName("ApproveBorrowRequest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBorrowRequestConfirm(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            try
            {
                // Check if requested quantity is available
                var warehouseAsset = await _warehouseAssetService.GetByIdAsync(request.WarehouseAssetId.Value);
                if (warehouseAsset.Quantity < request.Quantity)
                {
                    TempData["ErrorMessage"] = $"Không thể phê duyệt yêu cầu. Số lượng yêu cầu ({request.Quantity}) vượt quá số lượng hiện có ({warehouseAsset.Quantity}).";
                    return RedirectToAction(nameof(BorrowRequests));
                }

                // Update the warehouse asset quantity
                await _warehouseAssetService.UpdateQuantityAsync(warehouseAsset.Id, -request.Quantity.Value);

                // Update status to Approved
                request.ApproveStatus = TicketStatus.Approved;
                request.DateModified = DateTime.Now;
                await _borrowTicketService.UpdateAsync(request);

                TempData["SuccessMessage"] = "Yêu cầu mượn tài sản đã được phê duyệt thành công.";
                return RedirectToAction(nameof(BorrowRequests));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi phê duyệt yêu cầu: {ex.Message}";
                return RedirectToAction(nameof(BorrowRequests));
            }
        }

        // GET: BorrowRequest/RejectBorrowRequest/5
        public async Task<IActionResult> RejectBorrowRequest(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: BorrowRequest/RejectBorrowRequest/5
        [HttpPost, ActionName("RejectBorrowRequest")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBorrowRequestConfirm(int id, string rejectionReason)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            try
            {
                // Update status to Rejected instead of soft deleting
                request.ApproveStatus = TicketStatus.Rejected;
                request.DateModified = DateTime.Now;

                // Add the rejection reason to the note
                if (!string.IsNullOrEmpty(rejectionReason))
                {
                    request.Note = string.IsNullOrEmpty(request.Note)
                        ? $"Lý do từ chối: {rejectionReason}"
                        : $"{request.Note}\nLý do từ chối: {rejectionReason}";
                }

                await _borrowTicketService.UpdateAsync(request);

                TempData["SuccessMessage"] = "Yêu cầu mượn tài sản đã bị từ chối.";
                return RedirectToAction(nameof(BorrowRequests));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi từ chối yêu cầu: {ex.Message}";
                return RedirectToAction(nameof(BorrowRequests));
            }
        }
    }
}