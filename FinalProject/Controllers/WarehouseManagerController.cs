using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.Handover;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    //[Authorize(Roles = "WarehouseManager")]
    public class WarehouseManagerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseManagerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalDisposedAssets = (await _unitOfWork.WarehouseAssets.GetAssetsWithDisposedQuantity()).Sum(wa => wa.DisposedQuantity);
            var totalBrokenAssets = (await _unitOfWork.WarehouseAssets.GetAssetsWithBrokenQuantity()).Sum(wa => wa.BrokenQuantity);
            var totalFixingAssets = (await _unitOfWork.WarehouseAssets.GetAssetsWithFixingQuantity()).Sum(wa => wa.FixingQuantity);
            var totalGoodAssets = (await _unitOfWork.WarehouseAssets.GetAssetsWithGoodQuantity()).Sum(wa => wa.GoodQuantity);
            var totalAssets = totalFixingAssets + totalGoodAssets + totalBrokenAssets;

            var viewModel = new WarehouseManagerDashboardViewModel
            {
                TotalAssets = totalAssets.Value,
                TotalCategories = (await _unitOfWork.AssetCategories.GetAllAsync()).Count(),
                TotalDepartments = (await _unitOfWork.Departments.GetAllAsync()).Count(),
                TotalWarehouses = (await _unitOfWork.Warehouses.GetAllAsync()).Count(),
                TotalDisposedAssets = totalDisposedAssets.Value,
                TotalPendingBorrowRequests = (await _unitOfWork.BorrowTickets.GetBorrowTicketsWithoutReturn()).Count(),
                ActiveAssets = totalGoodAssets.Value,
                BrokenAssets = totalBrokenAssets.Value,
                FixingAssets = totalFixingAssets.Value
            };

            return View(viewModel);
        }

        public async Task<IActionResult> BorrowRequests()
        {
            var requests = await _unitOfWork.BorrowTickets.GetBorrowTicketsWithoutReturn();

            // Group by status
            ViewBag.PendingRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Pending).ToList();
            ViewBag.ApprovedRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Approved && !r.IsReturned).ToList();
            ViewBag.RejectedRequests = requests.Where(r => r.ApproveStatus == TicketStatus.Rejected).ToList();

            // Calculate overdue requests
            var currentDate = DateTime.Now;
            ViewBag.OverdueRequests = requests
                .Where(r => r.ApproveStatus == TicketStatus.Approved &&
                           !r.IsReturned &&
                           r.ReturnDate < currentDate)
                .ToList();

            // Calculate requests expiring soon (within 7 days)
            var expirationDate = currentDate.AddDays(7);
            ViewBag.ExpiringRequests = requests
                .Where(r => r.ApproveStatus == TicketStatus.Approved &&
                           !r.IsReturned &&
                           r.ReturnDate >= currentDate &&
                           r.ReturnDate <= expirationDate)
                .ToList();

            // Extension requests
            ViewBag.PendingExtensionRequests = requests
                .Where(r => r.ExtensionApproveStatus == TicketStatus.Pending)
                .ToList();

            return View(requests);
        }

        public async Task<IActionResult> ApproveBorrowRequest(int id)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Get current user to set as owner
            var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Update ticket
            borrowTicket.ApproveStatus = TicketStatus.Approved;
            borrowTicket.OwnerId = currentUser.Id;
            borrowTicket.DateModified = DateTime.Now;

            _unitOfWork.BorrowTickets.Update(borrowTicket);
            // Update warehouse asset borrowed quantity
            if (borrowTicket.WarehouseAssetId.HasValue)
            {
                await _unitOfWork.WarehouseAssets.UpdateBorrowedQuantity(
                   borrowTicket.WarehouseAssetId.Value,
                   borrowTicket.Quantity ?? 0);
            }
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yêu cầu mượn đã được phê duyệt thành công.";
            return RedirectToAction(nameof(BorrowRequests));
        }

        public async Task<IActionResult> RejectBorrowRequest(int id)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Display a form to enter rejection reason
            var model = new RejectBorrowRequestViewModel
            {
                BorrowTicketId = borrowTicket.Id,
                AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                BorrowerName = borrowTicket.BorrowBy?.FullName,
                Quantity = borrowTicket.Quantity,
                BorrowDate = borrowTicket.DateCreated,
                ReturnDate = borrowTicket.ReturnDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBorrowRequest(RejectBorrowRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(model.BorrowTicketId);
                if (borrowTicket == null)
                {
                    return NotFound();
                }

                // Update ticket
                borrowTicket.ApproveStatus = TicketStatus.Rejected;
                borrowTicket.Note = string.IsNullOrEmpty(borrowTicket.Note)
                    ? $"Từ chối: {model.RejectionReason}"
                    : $"{borrowTicket.Note}\nTừ chối: {model.RejectionReason}";
                borrowTicket.DateModified = DateTime.Now;

                _unitOfWork.BorrowTickets.Update(borrowTicket);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yêu cầu mượn đã bị từ chối.";
                return RedirectToAction(nameof(BorrowRequests));
            }

            return View(model);
        }

        // Method to approve extension request
        public async Task<IActionResult> ApproveExtension(int id)
        {
            try
            {
                var success = await _unitOfWork.BorrowTickets.ApproveExtensionAsync(id);
                await _unitOfWork.SaveChangesAsync();
                if (success)
                {
                    TempData["SuccessMessage"] = "Yêu cầu gia hạn đã được phê duyệt thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể phê duyệt yêu cầu gia hạn.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(BorrowRequests));
        }

        // Method to reject extension request
        public async Task<IActionResult> RejectExtension(int id)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            var model = new RejectExtensionViewModel
            {
                BorrowTicketId = borrowTicket.Id,
                AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                BorrowerName = borrowTicket.BorrowBy?.FullName,
                CurrentReturnDate = borrowTicket.ReturnDate,
                RequestDate = borrowTicket.ExtensionRequestDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectExtension(RejectExtensionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _unitOfWork.BorrowTickets.RejectExtensionAsync(
                        model.BorrowTicketId,
                        model.RejectionReason);
                    await _unitOfWork.SaveChangesAsync();
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Yêu cầu gia hạn đã bị từ chối.";
                        return RedirectToAction(nameof(BorrowRequests));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể từ chối yêu cầu gia hạn.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> ReturnRequests()
        {
            var returns = await _unitOfWork.ReturnTickets.GetAllAsync();

            // Group by status
            ViewBag.PendingReturns = returns.Where(r => r.ApproveStatus == TicketStatus.Pending).ToList();
            ViewBag.ApprovedReturns = returns.Where(r => r.ApproveStatus == TicketStatus.Approved).ToList();
            ViewBag.RejectedReturns = returns.Where(r => r.ApproveStatus == TicketStatus.Rejected).ToList();

            // Group by condition
            ViewBag.GoodConditionReturns = returns
                .Where(r => r.ApproveStatus == TicketStatus.Approved && r.AssetConditionOnReturn == AssetStatus.GOOD)
                .ToList();
            ViewBag.DamagedReturns = returns
                .Where(r => r.ApproveStatus == TicketStatus.Approved &&
                           (r.AssetConditionOnReturn == AssetStatus.BROKEN || r.AssetConditionOnReturn == AssetStatus.FIXING))
                .ToList();

            return View(returns);
        }

        // Other return request management methods are handled in ReturnRequestController

        public async Task<IActionResult> HandoverTickets()
        {
            var tickets = await _unitOfWork.HandoverTickets.GetAllAsync();

            // Group by active status
            ViewBag.ActiveHandovers = tickets.Where(h => h.IsActive).ToList();
            ViewBag.InactiveHandovers = tickets.Where(h => !h.IsActive).ToList();

            // Get pending returns
            var pendingReturns = await _unitOfWork.HandoverReturns.GetPendingHandoverReturns();
            ViewBag.PendingReturns = pendingReturns;

            return View(tickets);
        }

        public async Task<IActionResult> ProcessHandoverReturn(int id)
        {
            var handoverReturn = await _unitOfWork.HandoverReturns.GetHandoverReturnWithDetails(id);
            if (handoverReturn == null)
            {
                return NotFound();
            }

            var model = new ProcessHandoverReturnViewModel
            {
                HandoverReturnId = handoverReturn.Id,
                HandoverTicketId = handoverReturn.HandoverTicketId,
                AssetName = handoverReturn.HandoverTicket?.WarehouseAsset?.Asset?.Name,
                ReturnedBy = handoverReturn.ReturnBy?.FullName,
                ReturnDate = handoverReturn.ReturnDate,
                Quantity = handoverReturn.HandoverTicket?.Quantity,
                Notes = handoverReturn.Note,
                AssetCondition = AssetStatus.GOOD // Default to good condition
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessHandoverReturn(ProcessHandoverReturnViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await ApproveHandoverReturnAsync(
                        model.HandoverReturnId,
                        model.AssetCondition,
                        model.AdditionalNotes
                    );

                    TempData["SuccessMessage"] = "Trả lại tài sản bàn giao đã được xác nhận. Tài sản đã được cập nhật vào kho.";
                    return RedirectToAction(nameof(HandoverTickets));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> CreateHandoverReturn(int id)
        {
            var handoverTicket = await _unitOfWork.HandoverTickets.GetByIdAsync(id);
            if (handoverTicket == null)
            {
                return NotFound();
            }

            // Check if ticket is active
            if (!handoverTicket.IsActive)
            {
                TempData["ErrorMessage"] = "Phiếu bàn giao này đã không còn hoạt động.";
                return RedirectToAction(nameof(HandoverTickets));
            }

            var model = new CreateHandoverReturnViewModel
            {
                HandoverTicketId = handoverTicket.Id,
                AssetName = handoverTicket.WarehouseAsset?.Asset?.Name,
                ReturnName = handoverTicket.HandoverTo?.FullName,
                HandoverDate = handoverTicket.DateCreated,
                Quantity = handoverTicket.Quantity
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHandoverReturn(CreateHandoverReturnViewModel model)
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
                    await CreateHandoverReturnAsync(
                        model.HandoverTicketId,
                        model.ReturnById ?? currentUser.Id,
                        model.Notes
                    );

                    TempData["SuccessMessage"] = "Yêu cầu trả lại tài sản bàn giao đã được tạo.";
                    return RedirectToAction(nameof(HandoverTickets));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            // Reload handover ticket info if model validation fails
            var handoverTicket = await _unitOfWork.HandoverTickets.GetByIdAsync(model.HandoverTicketId);
            if (handoverTicket != null)
            {
                model.AssetName = handoverTicket.WarehouseAsset?.Asset?.Name;
                model.ReturnName = handoverTicket.HandoverTo?.FullName;
                model.HandoverDate = handoverTicket.DateCreated;
                model.Quantity = handoverTicket.Quantity;
            }

            return View(model);
        }

        public async Task<HandoverReturn> CreateHandoverReturnAsync(int handoverTicketId, int returnById, string notes)
        {
            // Get the handover ticket
            var handoverTicket = await _unitOfWork.HandoverTickets.GetByIdAsync(handoverTicketId);
            if (handoverTicket == null)
                throw new Exception("Handover ticket not found");

            if (!handoverTicket.IsActive)
                throw new Exception("This handover ticket is no longer active");

            // Create handover return record
            var handoverReturn = new HandoverReturn
            {
                HandoverTicketId = handoverTicketId,
                ReturnById = returnById,
                ReceivedById = handoverTicket.HandoverById, // Default to original handover person
                ReturnDate = DateTime.Now,
                Note = notes,
                DateCreated = DateTime.Now
            };

            await _unitOfWork.HandoverReturns.AddAsync(handoverReturn);
            await _unitOfWork.SaveChangesAsync();

            return handoverReturn;
        }

        public async Task<HandoverReturn> ApproveHandoverReturnAsync(int handoverReturnId, AssetStatus assetCondition, string notes)
        {
            var handoverReturn = await _unitOfWork.HandoverReturns.GetByIdAsync(handoverReturnId);
            if (handoverReturn == null)
                throw new Exception("Handover return record not found");

            var handoverTicket = await _unitOfWork.HandoverTickets.GetByIdAsync(handoverReturn.HandoverTicketId);
            if (handoverTicket == null)
                throw new Exception("Related handover ticket not found");

            // Update handover return if notes provided
            if (!string.IsNullOrEmpty(notes))
            {
                handoverReturn.Note = string.IsNullOrEmpty(handoverReturn.Note)
                    ? notes
                    : $"{handoverReturn.Note}\n{notes}";
            }

            // Set asset condition on return
            handoverReturn.AssetConditionOnReturn = assetCondition;
            handoverReturn.DateModified = DateTime.Now;

            // Update handover ticket status
            await _unitOfWork.HandoverTickets.UpdateHandoverTicketStatus(
                handoverTicket.Id,
                false,  // isActive = false
                DateTime.Now // actualEndDate
            );

            // Update warehouse asset quantities
            if (handoverTicket.WarehouseAssetId.HasValue)
            {
                await _unitOfWork.WarehouseAssets.UpdateWarehouseAssetQuantitiesForHandover(
                    handoverTicket.WarehouseAssetId.Value,
                    handoverTicket.Quantity ?? 0,
                    true, // isReturn
                    assetCondition
                );
            }

            _unitOfWork.HandoverReturns.Update(handoverReturn);
            await _unitOfWork.SaveChangesAsync();

            return handoverReturn;
        }

        public async Task<IActionResult> CreateDisposalTicket()
        {
            ViewBag.WarehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDisposalTicket(DisposalTicket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.DateCreated = DateTime.Now;
                await _unitOfWork.DisposalTickets.AddAsync(ticket);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(DisposalTickets));
            }
            ViewBag.WarehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
            return View(ticket);
        }

        public async Task<IActionResult> DisposalTickets()
        {
            var tickets = await _unitOfWork.DisposalTickets.GetAllAsync();
            return View(tickets);
        }

        public async Task<IActionResult> Reports()
        {
            return View();
        }

        public async Task<IActionResult> BorrowReport()
        {
            var borrowTickets = await _unitOfWork.BorrowTickets.GetAllAsync();
            return View(borrowTickets);
        }

        public async Task<IActionResult> ReturnReport()
        {
            var returnTickets = await _unitOfWork.ReturnTickets.GetAllAsync();
            return View(returnTickets);
        }

        public async Task<IActionResult> AssetSummaryReport()
        {
            var assets = await _unitOfWork.Assets.GetAllAsync();
            var categories = await _unitOfWork.AssetCategories.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var goodAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithGoodQuantity();
            var brokenAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithBrokenQuantity();
            var fixingAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithFixingQuantity();
            var disposedAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithDisposedQuantity();
            double totalValue = 0;
            foreach (var wa in goodAssets)
            {
                totalValue += wa.Asset.Price * (wa.GoodQuantity ?? 0);
            }
            var viewModel = new AssetSummaryReportViewModel
            {
                Assets = assets.ToList(),
                Categories = categories.ToList(),
                Warehouses = warehouses.ToList(),
                TotalValue = totalValue,
                AssetCountByStatus = new Dictionary<AssetStatus, int>
                {
                    { AssetStatus.GOOD, goodAssets.Count() },
                    { AssetStatus.BROKEN, brokenAssets.Count() },
                    { AssetStatus.FIXING, fixingAssets.Count() },
                    { AssetStatus.DISPOSED, disposedAssets.Count() }
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> DisposalReport()
        {
            var disposalTickets = await _unitOfWork.DisposalTickets.GetAllAsync();
            return View(disposalTickets);
        }

        public async Task<IActionResult> WarehouseInventoryReport()
        {
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
            return View(warehouseAssets);
        }

        public async Task<IActionResult> BorrowedAssetsReport()
        {
            var borrowTickets = await _unitOfWork.BorrowTickets.GetBorrowTicketsWithoutReturn();
            return View(borrowTickets);
        }

        public async Task<IActionResult> HandoverReport()
        {
            var handoverTickets = await _unitOfWork.HandoverTickets.GetAllAsync();
            return View(handoverTickets);
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _unitOfWork.Users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result)
            {
                ViewBag.Message = "Password changed successfully";
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Failed to change password");
                return View(model);
            }
        }

    }
}