using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    //[Authorize(Roles = "WarehouseManager")]
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IAssetCategoryService _assetCategoryService;
        private readonly IWarehouseAssetService _warehouseAssetService;
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IHandoverTicketService _handoverTicketService;
        private readonly CompanyAssetManagementContext _context;

        public AssetsController(
            IAssetService assetService,
            IAssetCategoryService assetCategoryService,
            IWarehouseAssetService warehouseAssetService,
            IBorrowTicketService borrowTicketService,
            IHandoverTicketService handoverTicketService,
            CompanyAssetManagementContext context)
        {
            _assetService = assetService;
            _assetCategoryService = assetCategoryService;
            _warehouseAssetService = warehouseAssetService;
            _borrowTicketService = borrowTicketService;
            _handoverTicketService = handoverTicketService;
            _context = context;
        }

        // GET: Assets/Index
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString,
            string status, int? page, bool showDeleted = false)
        {
            try
            {
                // Set up sorting parameters
                ViewBag.CurrentSort = sortOrder;
                ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
                ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
                ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";
                ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

                // Handle pagination reset for new searches
                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                // Store filter values in ViewBag
                ViewBag.CurrentFilter = searchString;
                ViewBag.StatusFilter = status;
                ViewBag.ShowDeleted = showDeleted;

                // Prepare assets query
                IEnumerable<Asset> assets;
                IQueryable<Asset> query;

                // Handle search with or without deleted items
                if (!string.IsNullOrEmpty(searchString))
                {
                    assets = await _assetService.SearchAssetAsync(searchString, showDeleted);
                    query = assets.AsQueryable();
                }
                else
                {
                    // Get assets based on deleted status filter
                    if (showDeleted)
                    {
                        assets = await _assetService.GetAllInCludeDeletedAsync();
                        // Filter to only show deleted assets
                        query = assets.Where(a => a.IsDeleted).AsQueryable();
                    }
                    else
                    {
                        assets = await _assetService.GetAllAsync();
                        query = assets.AsQueryable();
                    }
                }

                // Apply status filter if not in deleted mode
                if (!showDeleted && !string.IsNullOrEmpty(status) && Enum.TryParse<AssetStatus>(status, out var assetStatus))
                {
                    // Filter assets by status across warehouse assets
                    query = FilterAssetsByStatus(query, assetStatus);
                }

                // Apply sorting
                query = ApplySorting(query, sortOrder);

                // Set up pagination
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                var paginatedAssets = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                // Create view models with additional info
                var assetsWithInfo = await CreateAssetViewModels(paginatedAssets);

                return View(assetsWithInfo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<AssetViewModel>());
            }
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdIncludeDeletedAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(id.Value);
            var borrowTickets = await _borrowTicketService.GetBorrowTicketsByAssetAsync(id.Value);
            var handoverTickets = await _handoverTicketService.GetHandoverTicketsByAssetIdAsync(id.Value);

            var assetViewModel = new AssetDetailViewModel
            {
                Asset = asset,
                WarehouseAssets = warehouseAssets.ToList(),
                BorrowTickets = borrowTickets.ToList(),
                HandoverTickets = handoverTickets.ToList(),
                TotalGoodQuantity = warehouseAssets.Sum(wa => wa.GoodQuantity ?? 0),
                TotalBrokenQuantity = warehouseAssets.Sum(wa => wa.BrokenQuantity ?? 0),
                TotalFixingQuantity = warehouseAssets.Sum(wa => wa.FixingQuantity ?? 0),
                TotalDisposedQuantity = warehouseAssets.Sum(wa => wa.DisposedQuantity ?? 0),
                TotalBorrowedQuantity = warehouseAssets.Sum(wa => wa.BorrowedGoodQuantity ?? 0),
                TotalHandedOverQuantity = warehouseAssets.Sum(wa => wa.HandedOverGoodQuantity ?? 0)
            };

            return View(assetViewModel);
        }

        // GET: Assets/Create
        public async Task<IActionResult> Create()
        {
            await PrepareAssetCategoriesForView();
            return View();
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asset asset)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate asset names
                    var existingAssets = await _assetService.GetAllAsync();
                    if (existingAssets.Any(a => a.Name.Equals(asset.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại tài sản với tên này. Vui lòng chọn tên khác.");
                        await PrepareAssetCategoriesForView();
                        return View(asset);
                    }

                    // Set creation date
                    asset.DateCreated = DateTime.Now;
                    await _assetService.AddAsync(asset);

                    TempData["SuccessMessage"] = $"Tài sản '{asset.Name}' đã được tạo thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Tạo tài sản thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tài sản.");
                }
            }

            await PrepareAssetCategoriesForView();
            return View(asset);
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdIncludeDeletedAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            await PrepareAssetCategoriesForView();
            return View(asset);
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Asset asset)
        {
            if (id != asset.Id)
            {
                TempData["ErrorMessage"] = "ID tài sản không khớp.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing asset for comparison
                    var existingAsset = await _assetService.GetByIdIncludeDeletedAsync(id);
                    if (existingAsset == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy tài sản cần cập nhật.";
                        return NotFound();
                    }

                    // Check for duplicate names except self
                    var allAssets = await _assetService.GetAllAsync();
                    if (allAssets.Any(a => a.Id != id && a.Name.Equals(asset.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại tài sản khác với tên này. Vui lòng chọn tên khác.");
                        await PrepareAssetCategoriesForView();
                        return View(asset);
                    }

                    // Preserve original creation date and deletion status
                    asset.DateCreated = existingAsset.DateCreated;
                    asset.DateModified = DateTime.Now;
                    asset.IsDeleted = existingAsset.IsDeleted;
                    asset.DeletedDate = existingAsset.DeletedDate;

                    // Detach entity to avoid tracking conflicts
                    _context.Entry(existingAsset).State = EntityState.Detached;

                    await _assetService.UpdateAsync(asset);
                    TempData["SuccessMessage"] = $"Tài sản '{asset.Name}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Cập nhật tài sản thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật tài sản.");
                }
            }

            await PrepareAssetCategoriesForView();
            return View(asset);
        }

        // GET: Assets/ManageQuantity/5
        public async Task<IActionResult> ManageQuantity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(id.Value);
            var warehouses = await _context.Warehouses.ToListAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");

            var viewModel = new AssetQuantityViewModel
            {
                Asset = asset,
                WarehouseAssets = warehouseAssets.ToList()
            };

            return View(viewModel);
        }

        // POST: Assets/UpdateAssetQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAssetQuantity(AssetQuantityUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle updating or creating warehouse asset
                    if (model.WarehouseAssetId.HasValue)
                    {
                        // Update existing warehouse asset
                        var warehouseAsset = await _warehouseAssetService.GetByIdAsync(model.WarehouseAssetId.Value);
                        if (warehouseAsset == null)
                        {
                            TempData["ErrorMessage"] = "Không tìm thấy thông tin tài sản trong kho.";
                            return RedirectToAction(nameof(ManageQuantity), new { id = model.AssetId });
                        }

                        // Update quantities
                        if (model.GoodQuantity.HasValue) warehouseAsset.GoodQuantity = model.GoodQuantity;
                        if (model.BrokenQuantity.HasValue) warehouseAsset.BrokenQuantity = model.BrokenQuantity;
                        if (model.FixingQuantity.HasValue) warehouseAsset.FixingQuantity = model.FixingQuantity;
                        if (model.DisposedQuantity.HasValue) warehouseAsset.DisposedQuantity = model.DisposedQuantity;
                        warehouseAsset.DateModified = DateTime.Now;

                        await _warehouseAssetService.UpdateAsync(warehouseAsset);
                        TempData["SuccessMessage"] = "Cập nhật số lượng tài sản thành công.";
                    }
                    else if (model.WarehouseId.HasValue && model.AssetId.HasValue)
                    {
                        // Create new warehouse asset entry
                        var warehouseAsset = new WarehouseAsset
                        {
                            WarehouseId = model.WarehouseId,
                            AssetId = model.AssetId,
                            GoodQuantity = model.GoodQuantity ?? 0,
                            BrokenQuantity = model.BrokenQuantity ?? 0,
                            FixingQuantity = model.FixingQuantity ?? 0,
                            DisposedQuantity = model.DisposedQuantity ?? 0,
                            DateCreated = DateTime.Now
                        };

                        await _warehouseAssetService.AddAsync(warehouseAsset);
                        TempData["SuccessMessage"] = "Thêm số lượng tài sản vào kho thành công.";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Thông tin kho hoặc tài sản không hợp lệ.");
                        return RedirectToAction(nameof(ManageQuantity), new { id = model.AssetId });
                    }

                    return RedirectToAction(nameof(Details), new { id = model.AssetId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Cập nhật số lượng thất bại: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(ManageQuantity), new { id = model.AssetId });
        }

        // POST: Assets/TransferAssetStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferAssetStatus(AssetStatusTransferViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _warehouseAssetService.UpdateAssetStatusQuantityAsync(
                        model.WarehouseAssetId,
                        model.FromStatus,
                        model.ToStatus,
                        model.Quantity);

                    if (result)
                    {
                        TempData["SuccessMessage"] = $"Chuyển {model.Quantity} tài sản từ trạng thái {GetStatusName(model.FromStatus)} sang {GetStatusName(model.ToStatus)} thành công.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không thể chuyển trạng thái tài sản. Số lượng không đủ hoặc không hợp lệ.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Chuyển trạng thái thất bại: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";
            }

            return RedirectToAction(nameof(Details), new { id = model.AssetId });
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get asset including deleted ones to check if it's already deleted
            var asset = await _assetService.GetByIdIncludeDeletedAsync(id.Value);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                return RedirectToAction(nameof(Index));
            }

            // Check if asset can be deleted (not in use)
            bool canDelete = await CanDeleteAsset(id.Value);
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài sản này vì đang được mượn hoặc bàn giao.";
                return RedirectToAction(nameof(Index));
            }

            // Check if asset is already deleted
            if (asset.IsDeleted)
            {
                TempData["WarningMessage"] = "Tài sản này đã bị xóa trước đó.";
            }

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get asset including deleted ones
            var asset = await _assetService.GetByIdIncludeDeletedAsync(id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                return RedirectToAction(nameof(Index));
            }

            // Check if asset can be deleted
            bool canDelete = await CanDeleteAsset(id);
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài sản này vì đang được mượn hoặc bàn giao.";
                return RedirectToAction(nameof(Index));
            }

            // Check if asset is already deleted
            if (asset.IsDeleted)
            {
                TempData["WarningMessage"] = "Tài sản này đã bị xóa trước đó.";
                return View(asset);
            }

            try
            {
                await _assetService.SoftDeleteAsync(id);
                TempData["SuccessMessage"] = $"Tài sản '{asset.Name}' đã được xóa thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Xóa tài sản thất bại: {ex.Message}";
                return View(asset);
            }
        }

        // GET: Assets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _assetService.GetByIdIncludeDeletedAsync(id.Value);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                return RedirectToAction(nameof(Index), new { showDeleted = true });
            }

            if (!asset.IsDeleted)
            {
                TempData["WarningMessage"] = "Tài sản này chưa bị xóa, không cần khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            return View(asset);
        }

        // POST: Assets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var asset = await _assetService.GetByIdIncludeDeletedAsync(id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                return RedirectToAction(nameof(Index), new { showDeleted = true });
            }

            if (!asset.IsDeleted)
            {
                TempData["WarningMessage"] = "Tài sản này chưa bị xóa, không cần khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _assetService.RestoreAsync(id);
                TempData["SuccessMessage"] = $"Tài sản '{asset.Name}' đã được khôi phục thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Khôi phục tài sản thất bại: {ex.Message}";
                return RedirectToAction(nameof(Index), new { showDeleted = true });
            }
        }

        #region Helper Methods

        // Helper method to check if asset exists
        private async Task<bool> AssetExists(int id)
        {
            var asset = await _assetService.GetByIdAsync(id);
            return asset != null;
        }

        // Helper method to check if asset can be deleted
        private async Task<bool> CanDeleteAsset(int assetId)
        {
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(assetId);

            // Check if any asset is currently borrowed or handed over
            var isBorrowed = warehouseAssets.Any(wa => (wa.BorrowedGoodQuantity ?? 0) > 0);
            var isHandedOver = warehouseAssets.Any(wa => (wa.HandedOverGoodQuantity ?? 0) > 0);

            return !isBorrowed && !isHandedOver;
        }

        // Helper method to get borrowed quantity
        private async Task<int> GetBorrowedQuantityAsync(int assetId)
        {
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(assetId);
            return warehouseAssets.Sum(wa => wa.BorrowedGoodQuantity ?? 0);
        }

        // Helper method to get handed over quantity
        private async Task<int> GetHandedOverQuantityAsync(int assetId)
        {
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(assetId);
            return warehouseAssets.Sum(wa => wa.HandedOverGoodQuantity ?? 0);
        }

        // Helper method to filter assets by status
        private IQueryable<Asset> FilterAssetsByStatus(IQueryable<Asset> query, AssetStatus status)
        {
            return status switch
            {
                AssetStatus.GOOD => query.Where(a => a.WarehouseAssets.Any(wa => wa.GoodQuantity > 0)),
                AssetStatus.BROKEN => query.Where(a => a.WarehouseAssets.Any(wa => wa.BrokenQuantity > 0)),
                AssetStatus.FIXING => query.Where(a => a.WarehouseAssets.Any(wa => wa.FixingQuantity > 0)),
                AssetStatus.DISPOSED => query.Where(a => a.WarehouseAssets.Any(wa => wa.DisposedQuantity > 0)),
                _ => query
            };
        }

        // Helper method to apply sorting
        private IQueryable<Asset> ApplySorting(IQueryable<Asset> query, string sortOrder)
        {
            return sortOrder switch
            {
                "id_desc" => query.OrderByDescending(a => a.Id),
                "name_asc" => query.OrderBy(a => a.Name),
                "name_desc" => query.OrderByDescending(a => a.Name),
                "price_asc" => query.OrderBy(a => a.Price),
                "price_desc" => query.OrderByDescending(a => a.Price),
                "date_asc" => query.OrderBy(a => a.DateCreated),
                "date_desc" => query.OrderByDescending(a => a.DateCreated),
                _ => query.OrderBy(a => a.Id)
            };
        }

        // Helper method to get status name for display
        private string GetStatusName(AssetStatus status)
        {
            return status switch
            {
                AssetStatus.GOOD => "Tốt",
                AssetStatus.BROKEN => "Hỏng",
                AssetStatus.FIXING => "Đang sửa",
                AssetStatus.DISPOSED => "Đã thanh lý",
                _ => status.ToString()
            };
        }

        // Helper method to prepare asset categories for view
        private async Task PrepareAssetCategoriesForView()
        {
            var categories = await _assetCategoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }

        // Helper method to create asset view models with additional info
        private async Task<List<AssetViewModel>> CreateAssetViewModels(IEnumerable<Asset> assets)
        {
            var result = new List<AssetViewModel>();

            foreach (var asset in assets)
            {
                var assetViewModel = new AssetViewModel
                {
                    Asset = asset,
                    TotalQuantity = await GetTotalQuantityAsync(asset.Id),
                    BorrowedQuantity = await GetBorrowedQuantityAsync(asset.Id),
                    HandedOverQuantity = await GetHandedOverQuantityAsync(asset.Id)
                };

                result.Add(assetViewModel);
            }

            return result;
        }

        // Helper method to get total quantity
        private async Task<int> GetTotalQuantityAsync(int assetId)
        {
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(assetId);
            return warehouseAssets.Sum(wa =>
                (wa.GoodQuantity ?? 0) +
                (wa.BrokenQuantity ?? 0) +
                (wa.FixingQuantity ?? 0) +
                (wa.DisposedQuantity ?? 0));
        }

        #endregion
    }
}