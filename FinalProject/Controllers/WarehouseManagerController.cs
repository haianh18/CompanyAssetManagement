using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.Handover;
using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    //[Authorize(Roles = "WarehouseManager")]
    public class WarehouseManagerController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IAssetCategoryService _assetCategoryService;
        private readonly IDepartmentService _departmentService;
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IReturnTicketService _returnTicketService;
        private readonly IHandoverTicketService _handoverTicketService;
        private readonly IHandoverReturnService _handoverReturnService;
        private readonly IDisposalTicketService _disposalTicketService;
        private readonly IUserService _userService;
        private readonly IWarehouseService _warehouseService;
        private readonly IWarehouseAssetService _warehouseAssetService;

        public WarehouseManagerController(
            IAssetService assetService,
            IAssetCategoryService assetCategoryService,
            IDepartmentService departmentService,
            IBorrowTicketService borrowTicketService,
            IReturnTicketService returnTicketService,
            IHandoverTicketService handoverTicketService,
            IHandoverReturnService handoverReturnService,
            IDisposalTicketService disposalTicketService,
            IUserService userService,
            IWarehouseService warehouseService,
            IWarehouseAssetService warehouseAssetService)
        {
            _assetService = assetService;
            _assetCategoryService = assetCategoryService;
            _departmentService = departmentService;
            _borrowTicketService = borrowTicketService;
            _returnTicketService = returnTicketService;
            _handoverTicketService = handoverTicketService;
            _handoverReturnService = handoverReturnService;
            _disposalTicketService = disposalTicketService;
            _userService = userService;
            _warehouseService = warehouseService;
            _warehouseAssetService = warehouseAssetService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new WarehouseManagerDashboardViewModel
            {
                TotalAssets = (await _assetService.GetAllInCludeDeletedAsync()).Count(),
                TotalCategories = (await _assetCategoryService.GetAllInCludeDeletedAsync()).Count(),
                TotalDepartments = (await _departmentService.GetAllAsync()).Count(),
                TotalWarehouses = (await _warehouseService.GetAllAsync()).Count(),
                TotalDisposedAssets = (await _warehouseAssetService.GetAssetsWithDisposedQuantityAsync()).Count(),
                TotalPendingBorrowRequests = (await _borrowTicketService.GetBorrowTicketsWithoutReturnAsync()).Count(),
                ActiveAssets = (await _warehouseAssetService.GetAssetsWithGoodQuantityAsync()).Count(),
                BrokenAssets = (await _warehouseAssetService.GetAssetsWithBrokenQuantityAsync()).Count(),
                FixingAssets = (await _warehouseAssetService.GetAssetsWithFixingQuantityAsync()).Count()
            };

            return View(viewModel);
        }


        #region Borrow Requests Management
        public async Task<IActionResult> BorrowRequests()
        {
            var requests = await _borrowTicketService.GetBorrowTicketsWithoutReturnAsync();

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
            var borrowTicket = await _borrowTicketService.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Get current user to set as owner
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Update ticket
            borrowTicket.ApproveStatus = TicketStatus.Approved;
            borrowTicket.OwnerId = currentUser.Id;
            borrowTicket.DateModified = DateTime.Now;

            await _borrowTicketService.UpdateAsync(borrowTicket);

            // Update warehouse asset borrowed quantity
            if (borrowTicket.WarehouseAssetId.HasValue)
            {
                await _warehouseAssetService.UpdateBorrowedQuantityAsync(
                    borrowTicket.WarehouseAssetId.Value,
                    borrowTicket.Quantity ?? 0);
            }

            TempData["SuccessMessage"] = "Yêu cầu mượn đã được phê duyệt thành công.";
            return RedirectToAction(nameof(BorrowRequests));
        }

        public async Task<IActionResult> RejectBorrowRequest(int id)
        {
            var borrowTicket = await _borrowTicketService.GetByIdAsync(id);
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
                var borrowTicket = await _borrowTicketService.GetByIdAsync(model.BorrowTicketId);
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

                await _borrowTicketService.UpdateAsync(borrowTicket);

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
                var success = await _borrowTicketService.ApproveExtensionAsync(id);
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
            var borrowTicket = await _borrowTicketService.GetByIdAsync(id);
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
                    var success = await _borrowTicketService.RejectExtensionAsync(
                        model.BorrowTicketId,
                        model.RejectionReason);

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
        #endregion

        #region Return Requests Management
        public async Task<IActionResult> ReturnRequests()
        {
            var returns = await _returnTicketService.GetAllAsync();

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
        #endregion

        #region Handover and Return Management
        public async Task<IActionResult> HandoverTickets()
        {
            var tickets = await _handoverTicketService.GetAllAsync();

            // Group by active status
            ViewBag.ActiveHandovers = tickets.Where(h => h.IsActive).ToList();
            ViewBag.InactiveHandovers = tickets.Where(h => !h.IsActive).ToList();

            // Get pending returns
            var pendingReturns = await _handoverReturnService.GetPendingHandoverReturnsAsync();
            ViewBag.PendingReturns = pendingReturns;

            return View(tickets);
        }

        public async Task<IActionResult> ProcessHandoverReturn(int id)
        {
            var handoverReturn = await _handoverReturnService.GetHandoverReturnWithDetailsAsync(id);
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
                    await _handoverReturnService.ApproveHandoverReturnAsync(
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
            var handoverTicket = await _handoverTicketService.GetByIdAsync(id);
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
            var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _handoverReturnService.CreateHandoverReturnAsync(
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
            var handoverTicket = await _handoverTicketService.GetByIdAsync(model.HandoverTicketId);
            if (handoverTicket != null)
            {
                model.AssetName = handoverTicket.WarehouseAsset?.Asset?.Name;
                model.ReturnName = handoverTicket.HandoverTo?.FullName;
                model.HandoverDate = handoverTicket.DateCreated;
                model.Quantity = handoverTicket.Quantity;
            }

            return View(model);
        }

        public async Task<IActionResult> ProcessEmployeeTermination(int id)
        {
            var employee = await _userService.GetUserByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var model = new ProcessEmployeeTerminationViewModel
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                DepartmentName = employee.Department?.Name,
                ConfirmText = string.Empty // User will need to type confirmation
            };

            // Get active assets
            var activeHandovers = await _handoverTicketService.GetActiveHandoversByEmployeeAsync(id);
            var activeBorrows = await _borrowTicketService.GetActiveBorrowTicketsByUserAsync(id);

            model.ActiveHandoverAssets = activeHandovers.ToList();
            model.ActiveBorrowedAssets = activeBorrows.ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEmployeeTermination(ProcessEmployeeTerminationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Confirm termination with text verification
                if (model.ConfirmText != $"TERMINATE-{model.EmployeeId}")
                {
                    ModelState.AddModelError("ConfirmText", "Mã xác nhận không chính xác. Vui lòng nhập đúng theo hướng dẫn.");

                    // Reload lists
                    var activeHandovers = await _handoverTicketService.GetActiveHandoversByEmployeeAsync(model.EmployeeId);
                    var activeBorrows = await _borrowTicketService.GetActiveBorrowTicketsByUserAsync(model.EmployeeId);

                    model.ActiveHandoverAssets = activeHandovers.ToList();
                    model.ActiveBorrowedAssets = activeBorrows.ToList();

                    return View(model);
                }

                try
                {
                    // Process handover returns
                    await _handoverTicketService.ProcessEmployeeTerminationAsync(model.EmployeeId);

                    // Mark borrowed assets as needing return
                    var activeBorrows = await _borrowTicketService.GetActiveBorrowTicketsByUserAsync(model.EmployeeId);
                    var currentUser = await _userService.GetUserByUserNameAsync(User.Identity.Name);

                    foreach (var borrow in activeBorrows)
                    {
                        await _returnTicketService.CreateReturnRequestAsync(
                            borrow.Id,
                            currentUser.Id,
                            "Nhân viên nghỉ việc - yêu cầu trả tài sản"
                        );
                    }

                    // Update employee status (if needed - this would depend on your system)
                    // await _userService.DeactivateUserAsync(model.EmployeeId);

                    TempData["SuccessMessage"] = "Quy trình xử lý nghỉ việc đã được hoàn tất. Tất cả tài sản đã được yêu cầu trả lại.";
                    return RedirectToAction("Index", "User"); // Assuming you have a user management controller
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            // Reload lists
            var reloadHandovers = await _handoverTicketService.GetActiveHandoversByEmployeeAsync(model.EmployeeId);
            var reloadBorrows = await _borrowTicketService.GetActiveBorrowTicketsByUserAsync(model.EmployeeId);

            model.ActiveHandoverAssets = reloadHandovers.ToList();
            model.ActiveBorrowedAssets = reloadBorrows.ToList();

            return View(model);
        }
        #endregion

        #region Disposal Management
        public async Task<IActionResult> CreateDisposalTicket()
        {
            ViewBag.WarehouseAssets = await _warehouseAssetService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDisposalTicket(DisposalTicket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.DateCreated = DateTime.Now;
                await _disposalTicketService.AddAsync(ticket);
                return RedirectToAction(nameof(DisposalTickets));
            }
            ViewBag.WarehouseAssets = await _warehouseAssetService.GetAllAsync();
            return View(ticket);
        }

        public async Task<IActionResult> DisposalTickets()
        {
            var tickets = await _disposalTicketService.GetAllAsync();
            return View(tickets);
        }
        #endregion

        #region Reports
        public async Task<IActionResult> Reports()
        {
            return View();
        }

        public async Task<IActionResult> BorrowReport()
        {
            var borrowTickets = await _borrowTicketService.GetAllAsync();
            return View(borrowTickets);
        }

        public async Task<IActionResult> ReturnReport()
        {
            var returnTickets = await _returnTicketService.GetAllAsync();
            return View(returnTickets);
        }

        public async Task<IActionResult> AssetSummaryReport()
        {
            var assets = await _assetService.GetAllAsync();
            var categories = await _assetCategoryService.GetAllAsync();
            var warehouses = await _warehouseService.GetAllAsync();
            var goodAssets = await _warehouseAssetService.GetAssetsWithGoodQuantityAsync();
            var brokenAssets = await _warehouseAssetService.GetAssetsWithBrokenQuantityAsync();
            var fixingAssets = await _warehouseAssetService.GetAssetsWithFixingQuantityAsync();
            var disposedAssets = await _warehouseAssetService.GetAssetsWithDisposedQuantityAsync();
            var viewModel = new AssetSummaryReportViewModel
            {
                Assets = assets.ToList(),
                Categories = categories.ToList(),
                Warehouses = warehouses.ToList(),
                TotalValue = await _assetService.GetTotalAssetsValueAsync(),
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
            var disposalTickets = await _disposalTicketService.GetAllAsync();
            return View(disposalTickets);
        }

        public async Task<IActionResult> WarehouseInventoryReport()
        {
            var warehouseAssets = await _warehouseAssetService.GetAllAsync();
            return View(warehouseAssets);
        }

        public async Task<IActionResult> BorrowedAssetsReport()
        {
            var borrowTickets = await _borrowTicketService.GetBorrowTicketsWithoutReturnAsync();
            return View(borrowTickets);
        }

        public async Task<IActionResult> HandoverReport()
        {
            var handoverTickets = await _handoverTicketService.GetAllAsync();
            return View(handoverTickets);
        }
        #endregion

        #region Account Management
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

            var user = await _userService.GetUserByUserNameAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userService.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
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
        #endregion
    }
}