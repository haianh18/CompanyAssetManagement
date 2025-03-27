using FinalProject.Enums;
using FinalProject.Models.ViewModels.ReturnRequest;
using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    //[Authorize(Roles = "WarehouseManager")]
    public class ReturnRequestController : Controller
    {
        private readonly IReturnTicketService _returnTicketService;
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IUserService _userService;

        public ReturnRequestController(
            IReturnTicketService returnTicketService,
            IBorrowTicketService borrowTicketService,
            IUserService userService)
        {
            _returnTicketService = returnTicketService;
            _borrowTicketService = borrowTicketService;
            _userService = userService;
        }

        // GET: ReturnRequest/Index
        public async Task<IActionResult> Index()
        {
            var returnRequests = await _returnTicketService.GetAllAsync();
            return View(returnRequests);
        }

        // GET: ReturnRequest/PendingRequests
        public async Task<IActionResult> PendingRequests()
        {
            var pendingRequests = await _returnTicketService.GetPendingReturnRequestsAsync();
            return View(pendingRequests);
        }

        // GET: ReturnRequest/Create/5 (Create return request for a borrow ticket)
        public async Task<IActionResult> Create(int id)
        {
            var borrowTicket = await _borrowTicketService.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Check if already returned
            if (borrowTicket.IsReturned)
            {
                TempData["ErrorMessage"] = "Tài sản này đã được trả trước đó.";
                return RedirectToAction("BorrowRequests", "WarehouseManager");
            }

            var model = new CreateReturnRequestViewModel
            {
                BorrowTicketId = borrowTicket.Id,
                AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                BorrowerName = borrowTicket.BorrowBy?.FullName,
                BorrowDate = borrowTicket.DateCreated,
                ReturnDate = borrowTicket.ReturnDate,
                Quantity = borrowTicket.Quantity
            };

            return View(model);
        }

        // POST: ReturnRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReturnRequestViewModel model)
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
                    await _returnTicketService.CreateReturnRequestAsync(
                        model.BorrowTicketId,
                        currentUser.Id,
                        model.Notes
                    );

                    TempData["SuccessMessage"] = "Yêu cầu trả tài sản đã được tạo và gửi đến người mượn.";
                    return RedirectToAction("BorrowRequests", "WarehouseManager");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            // If we get here, something went wrong, so reload the borrow ticket info
            var borrowTicket = await _borrowTicketService.GetByIdAsync(model.BorrowTicketId);
            if (borrowTicket != null)
            {
                model.AssetName = borrowTicket.WarehouseAsset?.Asset?.Name;
                model.BorrowerName = borrowTicket.BorrowBy?.FullName;
                model.BorrowDate = borrowTicket.DateCreated;
                model.ReturnDate = borrowTicket.ReturnDate;
                model.Quantity = borrowTicket.Quantity;
            }

            return View(model);
        }

        // GET: ReturnRequest/ProcessReturn/5
        public async Task<IActionResult> ProcessReturn(int id)
        {
            var returnTicket = await _returnTicketService.GetReturnTicketWithDetailsAsync(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            var model = new ProcessReturnViewModel
            {
                ReturnTicketId = returnTicket.Id,
                BorrowTicketId = returnTicket.BorrowTicketId.Value,
                AssetName = returnTicket.BorrowTicket?.WarehouseAsset?.Asset?.Name,
                ReturnedBy = returnTicket.ReturnBy?.FullName,
                ReturnDate = DateTime.Now,
                Quantity = returnTicket.Quantity,
                Notes = returnTicket.Note,
                AssetCondition = AssetStatus.GOOD // Default to good condition
            };

            return View(model);
        }

        // POST: ReturnRequest/ProcessReturn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturn(ProcessReturnViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _returnTicketService.ApproveReturnAsync(
                        model.ReturnTicketId,
                        model.AssetCondition,
                        model.AdditionalNotes
                    );

                    TempData["SuccessMessage"] = "Trả tài sản đã được xác nhận. Tài sản đã được cập nhật vào kho.";
                    return RedirectToAction(nameof(PendingRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: ReturnRequest/Reject/5
        public async Task<IActionResult> Reject(int id)
        {
            var returnTicket = await _returnTicketService.GetReturnTicketWithDetailsAsync(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            var model = new RejectReturnViewModel
            {
                ReturnTicketId = returnTicket.Id,
                AssetName = returnTicket.BorrowTicket?.WarehouseAsset?.Asset?.Name,
                ReturnedBy = returnTicket.ReturnBy?.FullName,
                Notes = returnTicket.Note
            };

            return View(model);
        }

        // POST: ReturnRequest/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(RejectReturnViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _returnTicketService.RejectReturnAsync(
                        model.ReturnTicketId,
                        model.RejectionReason
                    );

                    TempData["SuccessMessage"] = "Yêu cầu trả tài sản đã bị từ chối.";
                    return RedirectToAction(nameof(PendingRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: ReturnRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var returnTicket = await _returnTicketService.GetReturnTicketWithDetailsAsync(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            return View(returnTicket);
        }
    }
}
