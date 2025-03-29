using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels.ReturnRequest;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Controllers
{
    //[Authorize(Roles = "WarehouseManager")]
    public class ReturnRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReturnRequestController(IUnitOfWork _unitOfWork)
        {
            this._unitOfWork = _unitOfWork;
        }

        // GET: ReturnRequest/Index
        public async Task<IActionResult> Index()
        {
            var returnRequests = await _unitOfWork.ReturnTickets.GetAllAsync();
            return View(returnRequests);
        }

        // GET: ReturnRequest/PendingRequests
        public async Task<IActionResult> PendingRequests()
        {
            var pendingRequests = await _unitOfWork.ReturnTickets.GetPendingReturnRequests();
            return View(pendingRequests);
        }

        // GET: ReturnRequest/Create/5 (Create return request for a borrow ticket)
        public async Task<IActionResult> Create(int id)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
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
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await CreateReturnRequestAsync(
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
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(model.BorrowTicketId);
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
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(id);
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
                    await ApproveReturnAsync(
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
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(id);
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
                    await _unitOfWork.ReturnTickets.RejectReturnAsync(
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
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            return View(returnTicket);
        }

        public async Task<ReturnTicket> CreateReturnRequestAsync(int borrowTicketId, int ownerId, string note)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null)
                throw new Exception("Borrow ticket not found");

            if (borrowTicket.IsReturned)
                throw new Exception("This borrow ticket has already been returned");

            // Create return request (initiated by warehouse manager)
            var returnTicket = new ReturnTicket
            {
                BorrowTicketId = borrowTicketId,
                ReturnById = borrowTicket.BorrowById,
                OwnerId = ownerId,
                Quantity = borrowTicket.Quantity,
                Note = note,
                ApproveStatus = TicketStatus.Pending,
                ReturnRequestDate = DateTime.Now,
                DateCreated = DateTime.Now
            };

            await _unitOfWork.ReturnTickets.AddAsync(returnTicket);
            await _unitOfWork.SaveChangesAsync();
            return returnTicket;
        }

        public async Task<ReturnTicket> ApproveReturnAsync(int returnTicketId, AssetStatus assetCondition, string notes)
        {
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithBorrowDetails(returnTicketId);
            if (returnTicket == null)
                throw new Exception("Return ticket not found");

            var borrowTicket = returnTicket.BorrowTicket;
            if (borrowTicket == null)
                throw new Exception("Related borrow ticket not found");

            // Add notes if provided
            if (!string.IsNullOrEmpty(notes))
            {
                returnTicket.Note = string.IsNullOrEmpty(returnTicket.Note)
                    ? notes
                    : $"{returnTicket.Note}\n{notes}";
            }

            // Update return ticket
            returnTicket.ApproveStatus = TicketStatus.Approved;
            returnTicket.ActualReturnDate = DateTime.Now;
            returnTicket.AssetConditionOnReturn = assetCondition;
            returnTicket.DateModified = DateTime.Now;

            // Mark borrow ticket as returned
            await _unitOfWork.BorrowTickets.MarkAsReturnedAsync(borrowTicket.Id);

            // Update warehouse asset quantities
            var warehouseAsset = borrowTicket.WarehouseAsset;
            if (warehouseAsset != null)
            {
                // Reduce borrowed quantity
                await _unitOfWork.WarehouseAssets.UpdateBorrowedQuantity(
                    warehouseAsset.Id,
                    -(borrowTicket.Quantity ?? 0));

                // Update asset quantities based on condition
                if (assetCondition == AssetStatus.GOOD)
                {
                    await _unitOfWork.WarehouseAssets.UpdateAssetStatusQuantity(
                        warehouseAsset.Id,
                        AssetStatus.GOOD,
                        AssetStatus.GOOD,
                        borrowTicket.Quantity ?? 0);
                }
                else
                {
                    await _unitOfWork.WarehouseAssets.UpdateAssetStatusQuantity(
                        warehouseAsset.Id,
                        AssetStatus.GOOD,
                        assetCondition,
                        borrowTicket.Quantity ?? 0);
                }
            }

            _unitOfWork.ReturnTickets.Update(returnTicket);
            await _unitOfWork.SaveChangesAsync();
            return returnTicket;
        }
    }
}
