using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Models.ViewModels.BorrowRequest;
using FinalProject.Models.ViewModels.Handover;
using FinalProject.Models.ViewModels.ReturnRequest;
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
    [Authorize(Roles = "WarehouseManager")]
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

            // Get pending borrow requests
            var pendingBorrowRequests = (await _unitOfWork.BorrowTickets.GetBorrowTicketsWithoutReturn())
                .Count(b => b.ApproveStatus == TicketStatus.Pending);

            // Get manager-initiated return requests count
            var managerReturnRequests = await _unitOfWork.ManagerReturnRequests.GetAllActiveRequests();
            var managerReturnCount = managerReturnRequests.Count();

            var viewModel = new WarehouseManagerDashboardViewModel
            {
                TotalAssets = totalAssets.Value,
                TotalCategories = (await _unitOfWork.AssetCategories.GetAllAsync()).Count(),
                TotalDepartments = (await _unitOfWork.Departments.GetAllAsync()).Count(),
                TotalWarehouses = (await _unitOfWork.Warehouses.GetAllAsync()).Count(),
                TotalDisposedAssets = totalDisposedAssets.Value,
                TotalPendingBorrowRequests = pendingBorrowRequests,
                ActiveAssets = totalGoodAssets.Value,
                BrokenAssets = totalBrokenAssets.Value,
                FixingAssets = totalFixingAssets.Value,
                ManagerReturnRequestsCount = managerReturnCount
            };

            return View(viewModel);
        }

        #region Borrow Request Management

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

        // GET: WarehouseManager/DetailsBorrowRequest/5
        public async Task<IActionResult> DetailsBorrowRequest(int id)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            return View(borrowTicket);
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

        // GET: WarehouseManager/ReturnedBorrowTickets
        public async Task<IActionResult> ReturnedBorrowTickets(string searchString)
        {
            // Get all borrow tickets that have been returned
            var returnedTickets = await _unitOfWork.BorrowTickets.GetAllAsync();
            returnedTickets = returnedTickets
                .Where(b => b.IsReturned)
                .OrderByDescending(b => b.DateModified)
                .ToList();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                returnedTickets = returnedTickets
                    .Where(b =>
                        (b.BorrowBy?.FullName?.ToLower().Contains(searchString) == true) ||
                        (b.WarehouseAsset?.Asset?.Name?.ToLower().Contains(searchString) == true) ||
                        (b.Note?.ToLower().Contains(searchString) == true))
                    .ToList();
            }

            // Pass the current filter to maintain it when paging
            ViewBag.CurrentFilter = searchString;

            return View(returnedTickets);
        }

        // GET: WarehouseManager/ReturnedWithDelayTickets
        public async Task<IActionResult> ReturnedWithDelayTickets()
        {
            // Get all returned tickets that were returned after their scheduled return date
            var returnedTickets = await _unitOfWork.BorrowTickets.GetAllAsync();
            var delayedTickets = returnedTickets
                .Where(b => b.IsReturned &&
                           b.ReturnDate.HasValue &&
                           b.ReturnTickets.Any(rt => rt.ApproveStatus == TicketStatus.Approved &&
                                                     rt.ActualReturnDate.HasValue &&
                                                     rt.ActualReturnDate.Value > b.ReturnDate.Value))
                .OrderByDescending(b => b.DateModified)
                .ToList();

            return View(delayedTickets);
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

        #endregion

        #region Return Request Management

        //GET: WarehouseManager/ReturnRequests
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

        // GET: WarehouseManager/ProcessReturnRequest/5
        public async Task<IActionResult> ProcessReturnRequest(int id)
        {
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            // Only process pending return requests
            if (returnTicket.ApproveStatus != TicketStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể xử lý yêu cầu trả đang chờ duyệt.";
                return RedirectToAction(nameof(ReturnRequests));
            }

            var model = new ProcessReturnViewModel
            {
                ReturnTicketId = returnTicket.Id,
                BorrowTicketId = returnTicket.BorrowTicketId ?? 0,
                AssetName = returnTicket.BorrowTicket?.WarehouseAsset?.Asset?.Name,
                ReturnedBy = returnTicket.ReturnBy?.FullName,
                ReturnDate = returnTicket.ReturnRequestDate,
                Quantity = returnTicket.Quantity,
                Notes = returnTicket.Note,
                AssetCondition = AssetStatus.GOOD // Default to good condition
            };

            return View(model);
        }

        // POST: WarehouseManager/ProcessReturnRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessReturnRequest(ProcessReturnViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var returnTicket = await _unitOfWork.ReturnTickets.GetByIdAsync(model.ReturnTicketId);
                    if (returnTicket == null)
                    {
                        return NotFound();
                    }

                    // Only process pending return requests
                    if (returnTicket.ApproveStatus != TicketStatus.Pending)
                    {
                        TempData["ErrorMessage"] = "Chỉ có thể xử lý yêu cầu trả đang chờ duyệt.";
                        return RedirectToAction(nameof(ReturnRequests));
                    }

                    // Get current user to set as owner
                    var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Update return ticket
                    returnTicket.ApproveStatus = TicketStatus.Approved;
                    returnTicket.OwnerId = currentUser.Id;
                    returnTicket.DateModified = DateTime.Now;
                    returnTicket.ActualReturnDate = DateTime.Now;
                    returnTicket.AssetConditionOnReturn = model.AssetCondition;

                    // Add additional notes if provided
                    if (!string.IsNullOrEmpty(model.AdditionalNotes))
                    {
                        returnTicket.Note = string.IsNullOrEmpty(returnTicket.Note)
                            ? model.AdditionalNotes
                            : $"{returnTicket.Note}\n{model.AdditionalNotes}";
                    }

                    _unitOfWork.ReturnTickets.Update(returnTicket);

                    // Mark the borrow ticket as returned
                    if (returnTicket.BorrowTicketId.HasValue)
                    {
                        var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(returnTicket.BorrowTicketId.Value);
                        if (borrowTicket != null)
                        {
                            borrowTicket.IsReturned = true;
                            borrowTicket.DateModified = DateTime.Now;
                            _unitOfWork.BorrowTickets.Update(borrowTicket);

                            // Update warehouse asset quantities
                            if (borrowTicket.WarehouseAssetId.HasValue)
                            {
                                // Decrease borrowed quantity
                                await _unitOfWork.WarehouseAssets.UpdateBorrowedQuantity(
                                    borrowTicket.WarehouseAssetId.Value,
                                    -(returnTicket.Quantity ?? 0));

                                // If asset is returned in non-GOOD condition, update status
                                if (model.AssetCondition != AssetStatus.GOOD)
                                {
                                    await _unitOfWork.WarehouseAssets.UpdateAssetStatusQuantity(
                                        borrowTicket.WarehouseAssetId.Value,
                                        AssetStatus.GOOD,
                                        model.AssetCondition,
                                        returnTicket.Quantity ?? 0);
                                }
                            }
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Yêu cầu trả đã được duyệt thành công. Tài sản đã được cập nhật vào kho với trạng thái {model.AssetCondition}.";
                    return RedirectToAction(nameof(ReturnRequests));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: WarehouseManager/RejectReturnRequest/5
        public async Task<IActionResult> RejectReturnRequest(int id)
        {
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            // Only reject pending return requests
            if (returnTicket.ApproveStatus != TicketStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể từ chối yêu cầu trả đang chờ duyệt.";
                return RedirectToAction(nameof(ReturnRequests));
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

        // POST: WarehouseManager/RejectReturnRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReturnRequest(RejectReturnViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var returnTicket = await _unitOfWork.ReturnTickets.RejectReturnAsync(model.ReturnTicketId, model.RejectionReason);
                    if (returnTicket != null)
                    {
                        await _unitOfWork.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Yêu cầu trả đã bị từ chối.";
                        return RedirectToAction(nameof(ReturnRequests));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy yêu cầu trả hoặc yêu cầu không ở trạng thái chờ duyệt.";
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: WarehouseManager/ReturnRequestDetail/5
        public async Task<IActionResult> ReturnRequestDetail(int id)
        {
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(id);
            if (returnTicket == null)
            {
                return NotFound();
            }

            return View(returnTicket);
        }

        // GET: WarehouseManager/CreateManagerReturnRequest/5
        public async Task<IActionResult> CreateManagerReturnRequest(int id)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            if (borrowTicket.IsReturned == false && borrowTicket.ApproveStatus == TicketStatus.Approved)
            {
                // Check if there's already a pending return ticket for this borrow ticket
                var pendingReturnTickets = await _unitOfWork.ReturnTickets.GetReturnTicketsByBorrowTicket(id);
                if (pendingReturnTickets.Any(rt => rt.ApproveStatus == TicketStatus.Pending))
                {
                    TempData["ErrorMessage"] = "Đã có yêu cầu trả đang chờ duyệt cho phiếu mượn này. Không thể tạo yêu cầu trả sớm mới.";
                    return RedirectToAction(nameof(OverdueAssets));
                }

                var model = new ManagerReturnRequestViewModel
                {
                    BorrowTicketId = borrowTicket.Id,
                    AssetName = borrowTicket.WarehouseAsset?.Asset?.Name,
                    BorrowerName = borrowTicket.BorrowBy?.FullName,
                    Quantity = borrowTicket.Quantity,
                    BorrowDate = borrowTicket.DateCreated,
                    ReturnDate = borrowTicket.ReturnDate,
                    // Set default due date to 3 days from now
                    DueDate = DateTime.Now.AddDays(3)
                };

                return View(model);
            }
            else
            {
                TempData["ErrorMessage"] = "Chỉ có thể yêu cầu trả sớm đối với tài sản đã được duyệt và chưa trả.";
                return RedirectToAction(nameof(OverdueAssets));
            }
        }

        // POST: WarehouseManager/CreateManagerReturnRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManagerReturnRequest(ManagerReturnRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(model.BorrowTicketId);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            // Check if there's already a pending return ticket for this borrow ticket
            var pendingReturnTickets = await _unitOfWork.ReturnTickets.GetReturnTicketsByBorrowTicket(model.BorrowTicketId);
            if (pendingReturnTickets.Any(rt => rt.ApproveStatus == TicketStatus.Pending))
            {
                TempData["ErrorMessage"] = "Đã có yêu cầu trả đang chờ duyệt cho phiếu mượn này. Không thể tạo yêu cầu trả sớm mới.";
                return RedirectToAction(nameof(OverdueAssets));
            }

            try
            {
                // Get current user to set as requester
                var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Create the manager return request
                var managerReturnRequest = new ManagerReturnRequest
                {
                    BorrowTicketId = model.BorrowTicketId,
                    RequestedById = currentUser.Id,
                    Reason = model.Reason,
                    DueDate = model.DueDate,
                    Status = TicketStatus.Pending,
                    DateCreated = DateTime.Now
                };

                await _unitOfWork.ManagerReturnRequests.AddAsync(managerReturnRequest);

                // Update the borrow ticket note to include the early return request
                if (!string.IsNullOrEmpty(borrowTicket.Note))
                {
                    borrowTicket.Note += $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Yêu cầu trả sớm: {model.Reason}";
                }
                else
                {
                    borrowTicket.Note = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] Yêu cầu trả sớm: {model.Reason}";
                }

                borrowTicket.DateModified = DateTime.Now;
                _unitOfWork.BorrowTickets.Update(borrowTicket);

                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Yêu cầu trả sớm đã được tạo và gửi đến người mượn.";
                return RedirectToAction(nameof(BorrowRequests));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo yêu cầu trả sớm.");
                return View(model);
            }
        }
        #endregion

        #region Pending Expired Ticket Management

        // GET: WarehouseManager/PendingExpiredTickets
        public async Task<IActionResult> PendingExpiredTickets()
        {
            var currentDate = DateTime.Now;

            // Get pending borrow tickets with return date in the past
            var pendingExpiredTickets = await _unitOfWork.BorrowTickets.GetAllAsync();
            pendingExpiredTickets = pendingExpiredTickets
                .Where(b => b.ApproveStatus == TicketStatus.Pending &&
                            b.ReturnDate.HasValue &&
                            b.ReturnDate.Value < currentDate)
                .ToList();

            return View(pendingExpiredTickets);
        }

        // POST: WarehouseManager/RejectExpiredPendingTicket/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectExpiredPendingTicket(int id)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            if (borrowTicket.ApproveStatus != TicketStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể từ chối phiếu mượn đang chờ duyệt.";
                return RedirectToAction(nameof(PendingExpiredTickets));
            }

            try
            {
                // Update ticket status to rejected
                borrowTicket.ApproveStatus = TicketStatus.Rejected;

                // Add automatic rejection note
                string rejectionReason = "Tự động từ chối: Ngày trả dự kiến đã qua.";
                if (string.IsNullOrEmpty(borrowTicket.Note))
                {
                    borrowTicket.Note = rejectionReason;
                }
                else
                {
                    borrowTicket.Note += $"\n{rejectionReason}";
                }

                borrowTicket.DateModified = DateTime.Now;

                // Get current user to set as owner
                var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
                if (currentUser != null)
                {
                    borrowTicket.OwnerId = currentUser.Id;
                }

                _unitOfWork.BorrowTickets.Update(borrowTicket);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Phiếu mượn đã bị từ chối do quá hạn.";
                return RedirectToAction(nameof(PendingExpiredTickets));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi cập nhật phiếu mượn: {ex.Message}";
                return RedirectToAction(nameof(PendingExpiredTickets));
            }
        }

        // POST: WarehouseManager/ApproveExpiredPendingTicket/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveExpiredPendingTicket(int id, DateTime newReturnDate)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            if (borrowTicket.ApproveStatus != TicketStatus.Pending)
            {
                TempData["ErrorMessage"] = "Chỉ có thể duyệt phiếu mượn đang chờ duyệt.";
                return RedirectToAction(nameof(PendingExpiredTickets));
            }

            if (newReturnDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Ngày trả mới phải lớn hơn ngày hiện tại.";
                return RedirectToAction(nameof(PendingExpiredTickets));
            }

            try
            {
                // Update ticket status to approved with new return date
                borrowTicket.ApproveStatus = TicketStatus.Approved;
                borrowTicket.ReturnDate = newReturnDate;

                // Add note about return date adjustment
                string approvalNote = $"Đã duyệt với điều chỉnh ngày trả từ {borrowTicket.ReturnDate?.ToString("dd/MM/yyyy")} thành {newReturnDate.ToString("dd/MM/yyyy")}";
                if (string.IsNullOrEmpty(borrowTicket.Note))
                {
                    borrowTicket.Note = approvalNote;
                }
                else
                {
                    borrowTicket.Note += $"\n{approvalNote}";
                }

                borrowTicket.DateModified = DateTime.Now;

                // Get current user to set as owner
                var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
                if (currentUser != null)
                {
                    borrowTicket.OwnerId = currentUser.Id;
                }

                // Update warehouse asset borrowed quantity
                if (borrowTicket.WarehouseAssetId.HasValue)
                {
                    await _unitOfWork.WarehouseAssets.UpdateBorrowedQuantity(
                       borrowTicket.WarehouseAssetId.Value,
                       borrowTicket.Quantity ?? 0);
                }

                _unitOfWork.BorrowTickets.Update(borrowTicket);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Phiếu mượn đã được duyệt với ngày trả mới.";
                return RedirectToAction(nameof(PendingExpiredTickets));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi cập nhật phiếu mượn: {ex.Message}";
                return RedirectToAction(nameof(PendingExpiredTickets));
            }
        }

        #endregion

        #region Overdue Asset Management

        //GET 
        public async Task<int> GetOverdueCount()
        {
            var currentDate = DateTime.Now;
            // Get all overdue borrow tickets
            var overdueTickets = await _unitOfWork.BorrowTickets.GetOverdueBorrowTickets();
            return overdueTickets.Count();
        }

        // GET: WarehouseManager/OverdueAssets
        public async Task<IActionResult> OverdueAssets()
        {
            var currentDate = DateTime.Now;

            // Get all overdue borrow tickets
            var overdueTickets = await _unitOfWork.BorrowTickets.GetOverdueBorrowTickets();

            return View(overdueTickets);
        }

        // POST: WarehouseManager/ForceReturn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceReturn(int id, string note, AssetStatus assetCondition)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
            if (borrowTicket == null)
            {
                return NotFound();
            }

            if (borrowTicket.IsReturned || borrowTicket.ApproveStatus != TicketStatus.Approved)
            {
                TempData["ErrorMessage"] = "Chỉ có thể thu hồi tài sản đã được duyệt và chưa trả.";
                return RedirectToAction(nameof(OverdueAssets));
            }

            try
            {
                // Get current user to set as the return receiver
                var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Create a return ticket for this forced return
                var returnTicket = new ReturnTicket
                {
                    BorrowTicketId = borrowTicket.Id,
                    ReturnById = borrowTicket.BorrowById, // The borrower is technically returning it, even if forced
                    OwnerId = currentUser.Id, // The manager is receiving it
                    Quantity = borrowTicket.Quantity,
                    ApproveStatus = TicketStatus.Approved, // Auto-approve the return
                    ReturnRequestDate = DateTime.Now,
                    ActualReturnDate = DateTime.Now,
                    Note = $"Thu hồi bắt buộc do quá hạn: {note}",
                    AssetConditionOnReturn = assetCondition, // Use the selected asset condition
                    DateCreated = DateTime.Now
                };

                await _unitOfWork.ReturnTickets.AddAsync(returnTicket);

                // Mark borrow ticket as returned
                borrowTicket.IsReturned = true;
                if (!string.IsNullOrEmpty(borrowTicket.Note))
                {
                    borrowTicket.Note += $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Thu hồi bắt buộc do quá hạn: {note}";
                }
                else
                {
                    borrowTicket.Note = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] Thu hồi bắt buộc do quá hạn: {note}";
                }
                borrowTicket.DateModified = DateTime.Now;

                _unitOfWork.BorrowTickets.Update(borrowTicket);

                // Update warehouse asset quantities
                if (borrowTicket.WarehouseAssetId.HasValue)
                {
                    // Revert the borrowed quantity back to available
                    await _unitOfWork.WarehouseAssets.UpdateBorrowedQuantity(
                        borrowTicket.WarehouseAssetId.Value,
                        -borrowTicket.Quantity ?? 0); // Negative to decrease the borrowed count

                    // If asset is not in good condition, update the status accordingly
                    if (assetCondition != AssetStatus.GOOD)
                    {
                        await _unitOfWork.WarehouseAssets.UpdateAssetStatusQuantity(
                            borrowTicket.WarehouseAssetId.Value,
                            AssetStatus.GOOD, // From good status
                            assetCondition,   // To selected status
                            borrowTicket.Quantity ?? 0);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tài sản đã được thu hồi thành công với trạng thái {assetCondition}.";
                return RedirectToAction(nameof(OverdueAssets));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(OverdueAssets));
            }
        }


        // POST: WarehouseManager/SendOverdueReminder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOverdueReminder(int id, string reminderMessage)
        {
            try
            {
                var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
                if (borrowTicket == null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(reminderMessage))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập nội dung nhắc nhở.";
                    return RedirectToAction("OverdueAssets");
                }

                // Add reminder to note
                borrowTicket.Note = string.IsNullOrEmpty(borrowTicket.Note)
                    ? $"Nhắc nhở {DateTime.Now.ToString("dd/MM/yyyy")}: {reminderMessage}"
                    : $"{borrowTicket.Note}\nNhắc nhở {DateTime.Now.ToString("dd/MM/yyyy")}: {reminderMessage}";

                borrowTicket.DateModified = DateTime.Now;

                _unitOfWork.BorrowTickets.Update(borrowTicket);
                await _unitOfWork.SaveChangesAsync();

                // In a real application, you might send an email or notification here

                TempData["SuccessMessage"] = "Đã gửi nhắc nhở cho người mượn.";
                return RedirectToAction("OverdueAssets");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("OverdueAssets");
            }
        }

        //
        // SendBatchOverdueReminders Action
        //
        // POST: WarehouseManager/SendBatchOverdueReminders
        [HttpPost]
        public async Task<IActionResult> SendBatchOverdueReminders(int[] ids, string reminderMessage)
        {
            if (ids == null || ids.Length == 0 || string.IsNullOrEmpty(reminderMessage))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                int successCount = 0;
                foreach (var id in ids)
                {
                    var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(id);
                    if (borrowTicket == null)
                        continue;

                    // Add reminder to note
                    borrowTicket.Note = string.IsNullOrEmpty(borrowTicket.Note)
                        ? $"Nhắc nhở {DateTime.Now.ToString("dd/MM/yyyy")}: {reminderMessage}"
                        : $"{borrowTicket.Note}\nNhắc nhở {DateTime.Now.ToString("dd/MM/yyyy")}: {reminderMessage}";

                    borrowTicket.DateModified = DateTime.Now;

                    _unitOfWork.BorrowTickets.Update(borrowTicket);
                    successCount++;
                }

                // Save all changes at once
                await _unitOfWork.SaveChangesAsync();

                // In a real application, you might send emails or notifications here

                return Json(new { success = true, message = $"Đã gửi nhắc nhở thành công cho {successCount} người mượn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        #endregion

        #region Handover Management

        public async Task<IActionResult> HandoverTickets()
        {
            var tickets = await _unitOfWork.HandoverTickets.GetAllAsync();

            // Group by active status
            ViewBag.ActiveHandovers = tickets.Where(h => h.IsActive).ToList();
            ViewBag.InactiveHandovers = tickets.Where(h => !h.IsActive).ToList();

            // Get pending returns
            var pendingReturns = await _unitOfWork.HandoverReturns.GetPendingHandoverReturns();
            ViewBag.PendingReturns = pendingReturns;

            // Get list of handover ticket IDs that have pending returns
            var handoverTicketsWithPendingReturns = pendingReturns.Select(pr => pr.HandoverTicketId).ToList();
            ViewBag.HandoverTicketsWithPendingReturns = handoverTicketsWithPendingReturns;

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> HandoverTicketDetails(int id)
        {
            var handoverTicket = await _unitOfWork.HandoverTickets.GetHandoverTicketWithDetailsAsync(id);
            if (handoverTicket == null)
            {
                return NotFound();
            }

            // Get any return requests for this handover ticket
            var handoverReturns = await _unitOfWork.HandoverReturns.GetHandoverReturnsByTicketId(id);
            ViewBag.HandoverReturns = handoverReturns;

            // Check if there's any pending return request
            var pendingReturn = handoverReturns.FirstOrDefault(hr => !hr.DateModified.HasValue);
            ViewBag.HasPendingReturnRequest = pendingReturn != null;
            if (pendingReturn != null)
            {
                ViewBag.PendingReturnId = pendingReturn.Id;
            }

            return View(handoverTicket);
        }

        [HttpGet]
        public async Task<IActionResult> CancelHandoverReturn(int id)
        {
            try
            {
                var handoverReturn = await _unitOfWork.HandoverReturns.GetByIdAsync(id);
                if (handoverReturn == null)
                {
                    return NotFound();
                }

                // Check if the return request has already been processed
                if (handoverReturn.DateModified.HasValue)
                {
                    TempData["ErrorMessage"] = "Không thể hủy yêu cầu trả đã được xử lý.";
                    return RedirectToAction(nameof(HandoverTicketDetails), new { id = handoverReturn.HandoverTicketId });
                }

                // Store the handover ticket ID before deleting the entity
                int handoverTicketId = handoverReturn.HandoverTicketId;

                // Hard delete the handover return request
                _unitOfWork.HandoverReturns.HardDelete(handoverReturn);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã hủy yêu cầu trả tài sản.";
                return RedirectToAction(nameof(HandoverTicketDetails), new { id = handoverTicketId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi hủy yêu cầu trả: {ex.Message}";
                return RedirectToAction(nameof(HandoverTickets));
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateHandoverTicket()
        {
            // Get available warehouse assets (with good quantity > 0)
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithGoodQuantity();
            ViewBag.WarehouseAssets = warehouseAssets;

            // Get users for dropdown
            var users = await _unitOfWork.Users.GetAllUsersAsync();
            ViewBag.Users = users;

            // Get departments for dropdown
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = departments;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHandoverTicket(HandoverTicket handoverTicket)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get current user to set as handover initiator
                    var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Set handover by and created date
                    handoverTicket.HandoverById = currentUser.Id;
                    handoverTicket.DateCreated = DateTime.Now;
                    handoverTicket.IsActive = true;

                    // Get the warehouse asset to validate quantity
                    var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(handoverTicket.WarehouseAssetId ?? 0);
                    if (warehouseAsset == null)
                    {
                        ModelState.AddModelError("WarehouseAssetId", "Tài sản không tồn tại");
                        await PrepareViewBagForCreateHandoverTicket();
                        return View(handoverTicket);
                    }

                    // Check if there's enough available quantity
                    int availableQuantity = (warehouseAsset.GoodQuantity ?? 0) - (warehouseAsset.BorrowedGoodQuantity ?? 0) - (warehouseAsset.HandedOverGoodQuantity ?? 0);
                    if ((handoverTicket.Quantity ?? 0) > availableQuantity)
                    {
                        ModelState.AddModelError("Quantity", $"Số lượng bàn giao vượt quá số lượng khả dụng ({availableQuantity})");
                        await PrepareViewBagForCreateHandoverTicket();
                        return View(handoverTicket);
                    }

                    // Add the handover ticket
                    await _unitOfWork.HandoverTickets.AddAsync(handoverTicket);

                    // Update the warehouse asset quantities
                    warehouseAsset.HandedOverGoodQuantity = (warehouseAsset.HandedOverGoodQuantity ?? 0) + (handoverTicket.Quantity ?? 0);
                    _unitOfWork.WarehouseAssets.Update(warehouseAsset);

                    // Save changes
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Phiếu bàn giao đã được tạo thành công.";
                    return RedirectToAction(nameof(HandoverTickets));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi tạo phiếu bàn giao: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", $"Chi tiết: {ex.InnerException.Message}");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            await PrepareViewBagForCreateHandoverTicket();
            return View(handoverTicket);
        }

        // Helper method to prepare ViewBag data for the Create view
        private async Task PrepareViewBagForCreateHandoverTicket()
        {
            // Get available warehouse assets (with good quantity > 0)
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithGoodQuantity();
            ViewBag.WarehouseAssets = warehouseAssets;

            // Get users for dropdown
            var users = await _unitOfWork.Users.GetAllUsersAsync();
            ViewBag.Users = users;

            // Get departments for dropdown
            var departments = await _unitOfWork.Departments.GetAllAsync();
            ViewBag.Departments = departments;
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

        [HttpGet]
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

            // Check if there's already a pending return request for this handover ticket
            var pendingReturns = await _unitOfWork.HandoverReturns.GetHandoverReturnsByTicketId(id);
            var hasPendingReturn = pendingReturns.Any(r => !r.DateModified.HasValue);

            if (hasPendingReturn)
            {
                TempData["ErrorMessage"] = "Đã có yêu cầu trả tài sản đang chờ xử lý cho phiếu bàn giao này.";
                return RedirectToAction(nameof(HandoverTicketDetails), new { id = id });
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
                    // Check if there's already a pending return request for this handover ticket
                    var pendingReturns = await _unitOfWork.HandoverReturns.GetHandoverReturnsByTicketId(model.HandoverTicketId);
                    var hasPendingReturn = pendingReturns.Any(r => !r.DateModified.HasValue);

                    if (hasPendingReturn)
                    {
                        TempData["ErrorMessage"] = "Đã có yêu cầu trả tài sản đang chờ xử lý cho phiếu bàn giao này.";
                        return RedirectToAction(nameof(HandoverTicketDetails), new { id = model.HandoverTicketId });
                    }

                    await CreateHandoverReturnAsync(
                        model.HandoverTicketId,
                        model.ReturnById ?? currentUser.Id,
                        model.Notes
                    );

                    TempData["SuccessMessage"] = "Yêu cầu trả lại tài sản bàn giao đã được tạo.";
                    return RedirectToAction(nameof(HandoverTicketDetails), new { id = model.HandoverTicketId });
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

        private async Task<HandoverReturn> CreateHandoverReturnAsync(int handoverTicketId, int returnById, string notes)
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

        #endregion

        #region Dispose Asset Management

        public async Task<IActionResult> CreateDisposalTicket()
        {
            // Populate warehouse assets for dropdown
            ViewBag.WarehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDisposalTicket(DisposalTicket ticket, int? selectedAssetId, int? assetQuantity, double? disposedPrice)
        {
            if (ModelState.IsValid)
            {
                // Get current user to set as disposal initiator
                var currentUser = await _unitOfWork.Users.GetUserByUserNameAsync(User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Set disposal by and created date
                ticket.DisposalById = currentUser.Id;
                ticket.DateCreated = DateTime.Now;

                // First create the disposal ticket
                await _unitOfWork.DisposalTickets.AddAsync(ticket);
                await _unitOfWork.SaveChangesAsync();

                // If an asset was selected, add it to the disposal ticket
                if (selectedAssetId.HasValue && assetQuantity.HasValue && assetQuantity.Value > 0)
                {
                    var disposalTicketAsset = new DisposalTicketAsset
                    {
                        DisposalTicketId = ticket.Id,
                        WarehouseAssetId = selectedAssetId.Value,
                        Quantity = assetQuantity.Value,
                        DisposedPrice = disposedPrice,
                        DateCreated = DateTime.Now
                    };

                    await _unitOfWork.DisposalTicketAssets.AddAsync(disposalTicketAsset);

                    // Update warehouse asset quantities
                    var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(selectedAssetId.Value);
                    if (warehouseAsset != null)
                    {
                        // Apply quantity reduction logic (prioritize broken first, then fixing, then good)
                        await UpdateWarehouseAssetQuantities(warehouseAsset, assetQuantity.Value);

                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = "Phiếu thanh lý đã được tạo thành công.";
                return RedirectToAction(nameof(DisposalTicketDetails), new { id = ticket.Id });
            }

            ViewBag.WarehouseAssets = await _unitOfWork.WarehouseAssets.GetAssetsWithNonZeroQuantity();
            return View(ticket);
        }

        // Helper method to update warehouse asset quantities
        private async Task UpdateWarehouseAssetQuantities(WarehouseAsset warehouseAsset, int disposalQuantity)
        {
            int remainingQuantity = disposalQuantity;

            // Reduce broken quantity first
            int brokenQuantity = warehouseAsset.BrokenQuantity ?? 0;
            if (brokenQuantity > 0)
            {
                int quantityToReduce = Math.Min(brokenQuantity, remainingQuantity);
                warehouseAsset.BrokenQuantity -= quantityToReduce;
                remainingQuantity -= quantityToReduce;
            }

            // Then reduce fixing quantity if needed
            if (remainingQuantity > 0)
            {
                int fixingQuantity = warehouseAsset.FixingQuantity ?? 0;
                if (fixingQuantity > 0)
                {
                    int quantityToReduce = Math.Min(fixingQuantity, remainingQuantity);
                    warehouseAsset.FixingQuantity -= quantityToReduce;
                    remainingQuantity -= quantityToReduce;
                }
            }

            // Finally reduce good quantity if still needed
            if (remainingQuantity > 0)
            {
                warehouseAsset.GoodQuantity -= remainingQuantity;
            }

            // Increase disposed quantity
            warehouseAsset.DisposedQuantity = (warehouseAsset.DisposedQuantity ?? 0) + disposalQuantity;

            // Update warehouse asset
            _unitOfWork.WarehouseAssets.Update(warehouseAsset);
        }

        public async Task<IActionResult> DisposalTickets()
        {
            // Get all disposal tickets with filtering options if needed
            var tickets = await _unitOfWork.DisposalTickets.GetAllWithDetailsAsync();

            // Filter by date range if provided in ViewBag/TempData
            if (Request.Query.ContainsKey("dateFrom") && DateTime.TryParse(Request.Query["dateFrom"], out DateTime dateFrom))
            {
                ViewBag.DateFrom = dateFrom.ToString("yyyy-MM-dd");
                tickets = tickets.Where(t => t.DateCreated >= dateFrom).ToList();
            }

            if (Request.Query.ContainsKey("dateTo") && DateTime.TryParse(Request.Query["dateTo"], out DateTime dateTo))
            {
                ViewBag.DateTo = dateTo.ToString("yyyy-MM-dd");
                tickets = tickets.Where(t => t.DateCreated <= dateTo).ToList();
            }

            // Filter by search text if provided
            if (Request.Query.ContainsKey("searchText"))
            {
                string searchText = Request.Query["searchText"];
                ViewBag.SearchText = searchText;
                tickets = tickets.Where(t =>
                    (t.Reason != null && t.Reason.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Note != null && t.Note.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            return View(tickets);
        }

        [HttpGet]
        public async Task<IActionResult> DisposalTicketDetails(int id)
        {
            var disposalTicket = await _unitOfWork.DisposalTickets.GetDisposalTicketWithDetails(id);
            if (disposalTicket == null)
            {
                return NotFound();
            }

            return View(disposalTicket);
        }

        [HttpGet]
        public async Task<IActionResult> EditDisposalTicket(int id)
        {
            var disposalTicket = await _unitOfWork.DisposalTickets.GetDisposalTicketWithDetails(id);
            if (disposalTicket == null)
            {
                return NotFound();
            }

            return View(disposalTicket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDisposalTicket(DisposalTicket ticket)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing ticket to update only specific fields
                    var existingTicket = await _unitOfWork.DisposalTickets.GetByIdAsync(ticket.Id);
                    if (existingTicket == null)
                    {
                        return NotFound();
                    }

                    // Update only the editable fields
                    existingTicket.Reason = ticket.Reason;
                    existingTicket.Note = ticket.Note;
                    existingTicket.DateModified = DateTime.Now;

                    _unitOfWork.DisposalTickets.Update(existingTicket);
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Phiếu thanh lý đã được cập nhật thành công.";
                    return RedirectToAction(nameof(DisposalTicketDetails), new { id = existingTicket.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật phiếu thanh lý: " + ex.Message);
                }
            }

            // Reload the disposal ticket with details if validation fails
            var disposalTicket = await _unitOfWork.DisposalTickets.GetDisposalTicketWithDetails(ticket.Id);
            return View(disposalTicket);
        }

        [HttpGet]
        public async Task<IActionResult> AddAssetToDisposal(int id)
        {
            // Set the disposal ticket ID for the form
            ViewBag.DisposalTicketId = id;

            // Get warehouse assets with available quantity for selection
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
            ViewBag.WarehouseAssets = warehouseAssets;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssetToDisposal(DisposalTicketAsset disposalTicketAsset)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // IMPORTANT: Make sure the ID is not set
                    disposalTicketAsset.Id = 0; // This will force EF to treat it as a new entity

                    // Set creation date
                    disposalTicketAsset.DateCreated = DateTime.Now;

                    // Check if the warehouse asset exists
                    var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(disposalTicketAsset.WarehouseAssetId ?? 0);
                    if (warehouseAsset == null)
                    {
                        ModelState.AddModelError("", "Tài sản không tồn tại");
                        ViewBag.DisposalTicketId = disposalTicketAsset.DisposalTicketId;
                        ViewBag.WarehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
                        return View(disposalTicketAsset);
                    }

                    // Validate quantity
                    int totalAvailable = (warehouseAsset.BrokenQuantity ?? 0) +
                                         (warehouseAsset.FixingQuantity ?? 0) +
                                         (warehouseAsset.GoodQuantity ?? 0);

                    if ((disposalTicketAsset.Quantity ?? 0) > totalAvailable)
                    {
                        ModelState.AddModelError("Quantity", $"Số lượng thanh lý không thể lớn hơn tổng số lượng hiện có ({totalAvailable})");
                        ViewBag.DisposalTicketId = disposalTicketAsset.DisposalTicketId;
                        ViewBag.WarehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
                        return View(disposalTicketAsset);
                    }

                    // Add the asset to the disposal ticket
                    await _unitOfWork.DisposalTicketAssets.AddAsync(disposalTicketAsset);

                    // Determine which status to reduce based on preferred order
                    int disposalQuantity = disposalTicketAsset.Quantity ?? 0;
                    int brokenQuantity = warehouseAsset.BrokenQuantity ?? 0;
                    int fixingQuantity = warehouseAsset.FixingQuantity ?? 0;
                    int goodQuantity = warehouseAsset.GoodQuantity ?? 0;

                    // Reduce broken quantity first
                    if (brokenQuantity > 0)
                    {
                        int quantityToReduce = Math.Min(brokenQuantity, disposalQuantity);
                        warehouseAsset.BrokenQuantity -= quantityToReduce;
                        disposalQuantity -= quantityToReduce;
                    }

                    // Then reduce fixing quantity if needed
                    if (disposalQuantity > 0 && fixingQuantity > 0)
                    {
                        int quantityToReduce = Math.Min(fixingQuantity, disposalQuantity);
                        warehouseAsset.FixingQuantity -= quantityToReduce;
                        disposalQuantity -= quantityToReduce;
                    }

                    // Finally reduce good quantity if still needed
                    if (disposalQuantity > 0 && goodQuantity > 0)
                    {
                        int quantityToReduce = Math.Min(goodQuantity, disposalQuantity);
                        warehouseAsset.GoodQuantity -= quantityToReduce;
                    }

                    // Increase disposed quantity
                    warehouseAsset.DisposedQuantity = (warehouseAsset.DisposedQuantity ?? 0) + (disposalTicketAsset.Quantity ?? 0);

                    // Update the warehouse asset
                    _unitOfWork.WarehouseAssets.Update(warehouseAsset);

                    // Save all changes at once
                    await _unitOfWork.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Tài sản đã được thêm vào phiếu thanh lý thành công.";
                    return RedirectToAction(nameof(DisposalTicketDetails), new { id = disposalTicketAsset.DisposalTicketId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi thêm tài sản: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", $"Chi tiết: {ex.InnerException.Message}");
                    }
                }
            }

            // Set the disposal ticket ID for the form
            ViewBag.DisposalTicketId = disposalTicketAsset.DisposalTicketId;

            // Get warehouse assets with available quantity for selection
            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
            ViewBag.WarehouseAssets = warehouseAssets;

            return View(disposalTicketAsset);
        }


        #endregion

        #region Reports
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

        #endregion

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