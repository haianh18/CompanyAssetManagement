using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
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
                TotalDisposedAssets = (await _assetService.GetAssetsByStatusAsync(AssetStatus.DISPOSED)).Count(),
                TotalPendingBorrowRequests = (await _borrowTicketService.GetBorrowTicketsWithoutReturnAsync()).Count(),
                ActiveAssets = (await _assetService.GetAssetsByStatusAsync(AssetStatus.GOOD)).Count(),
                BrokenAssets = (await _assetService.GetAssetsByStatusAsync(AssetStatus.BROKEN)).Count(),
                FixingAssets = (await _assetService.GetAssetsByStatusAsync(AssetStatus.FIXING)).Count()
            };

            return View(viewModel);
        }

        #region Asset Categories Management
        public async Task<IActionResult> AssetCategories()
        {
            var categories = await _assetCategoryService.GetAllAsync();
            return View(categories);
        }

        public IActionResult CreateAssetCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssetCategory(AssetCategory category)
        {
            if (ModelState.IsValid)
            {
                category.DateCreated = DateTime.Now;
                await _assetCategoryService.AddAsync(category);
                return RedirectToAction(nameof(AssetCategories));
            }
            return View(category);
        }

        public async Task<IActionResult> EditAssetCategory(int id)
        {
            var category = await _assetCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAssetCategory(int id, AssetCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                category.DateModified = DateTime.Now;
                await _assetCategoryService.UpdateAsync(category);
                return RedirectToAction(nameof(AssetCategories));
            }
            return View(category);
        }

        public async Task<IActionResult> DeleteAssetCategory(int id)
        {
            var category = await _assetCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("DeleteAssetCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssetCategoryConfirmed(int id)
        {
            await _assetCategoryService.SoftDeleteCategoryAsync(id);
            return RedirectToAction(nameof(AssetCategories));
        }
        #endregion

        #region Borrow Requests Management
        public async Task<IActionResult> BorrowRequests()
        {
            var requests = await _borrowTicketService.GetBorrowTicketsWithoutReturnAsync();
            return View(requests);
        }

        public async Task<IActionResult> ApproveBorrowRequest(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            // Logic to approve borrow request
            // This might involve changing status, updating warehouse assets, etc.
            return RedirectToAction(nameof(BorrowRequests));
        }

        public async Task<IActionResult> RejectBorrowRequest(int id)
        {
            var request = await _borrowTicketService.GetByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }
            // Logic to reject borrow request
            // This might involve changing status, etc.
            return RedirectToAction(nameof(BorrowRequests));
        }
        #endregion

        #region Return Requests Management
        public async Task<IActionResult> ReturnRequests()
        {
            var returns = await _returnTicketService.GetAllAsync();
            return View(returns);
        }

        public async Task<IActionResult> ApproveReturnRequest(int id)
        {
            var returnRequest = await _returnTicketService.GetByIdAsync(id);
            if (returnRequest == null)
            {
                return NotFound();
            }
            // Logic to approve return request
            // This might involve changing status, updating warehouse assets, etc.
            return RedirectToAction(nameof(ReturnRequests));
        }
        #endregion

        #region Handover Management
        public async Task<IActionResult> CreateHandoverTicket()
        {
            ViewBag.Users = await _userService.GetAllUsersAsync();
            ViewBag.Departments = await _departmentService.GetAllAsync();
            ViewBag.WarehouseAssets = await _warehouseAssetService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHandoverTicket(HandoverTicket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.DateCreated = DateTime.Now;
                await _handoverTicketService.AddAsync(ticket);
                return RedirectToAction(nameof(HandoverTickets));
            }
            ViewBag.Users = await _userService.GetAllUsersAsync();
            ViewBag.Departments = await _departmentService.GetAllAsync();
            ViewBag.WarehouseAssets = await _warehouseAssetService.GetAllAsync();
            return View(ticket);
        }

        public async Task<IActionResult> HandoverTickets()
        {
            var tickets = await _handoverTicketService.GetAllAsync();
            return View(tickets);
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

            var viewModel = new AssetSummaryReportViewModel
            {
                Assets = assets.ToList(),
                Categories = categories.ToList(),
                Warehouses = warehouses.ToList(),
                TotalValue = await _assetService.GetTotalAssetsValueAsync(),
                AssetCountByStatus = new Dictionary<AssetStatus, int>
                {
                    { AssetStatus.GOOD, await _assetService.CountAssetsByStatusAsync(AssetStatus.GOOD) },
                    { AssetStatus.BROKEN, await _assetService.CountAssetsByStatusAsync(AssetStatus.BROKEN) },
                    { AssetStatus.FIXING, await _assetService.CountAssetsByStatusAsync(AssetStatus.FIXING) },
                    { AssetStatus.DISPOSED, await _assetService.CountAssetsByStatusAsync(AssetStatus.DISPOSED) }
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