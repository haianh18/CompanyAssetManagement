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
                // Lưu trạng thái sắp xếp hiện tại
                ViewBag.CurrentSort = sortOrder;
                ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
                ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
                ViewBag.PriceSortParam = sortOrder == "price_asc" ? "price_desc" : "price_asc";
                ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

                // Xử lý reset pagination
                if (searchString != null)
                {
                    page = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                // Lưu các giá trị filter vào ViewBag
                ViewBag.CurrentFilter = searchString;
                ViewBag.StatusFilter = status;
                ViewBag.ShowDeleted = showDeleted;

                // Chuẩn bị query
                IEnumerable<Asset> assets;
                IQueryable<Asset> query;

                // Xử lý tìm kiếm với trạng thái xóa
                if (!string.IsNullOrEmpty(searchString))
                {
                    // We always include deleted items in search when showDeleted is true
                    assets = await _assetService.SearchAssetAsync(searchString, showDeleted);
                    query = assets.AsQueryable();
                }
                else
                {
                    // Get all assets including deleted ones if showDeleted is true
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

                // Áp dụng bộ lọc trạng thái (skip if showing deleted items)
                if (!showDeleted && !string.IsNullOrEmpty(status) && Enum.TryParse<AssetStatus>(status, out var assetStatus))
                {
                    // Do not use AssetStatus from Asset, but instead look at the related WarehouseAssets
                    if (assetStatus == AssetStatus.GOOD)
                    {
                        query = query.Where(a => a.WarehouseAssets.Any(wa => wa.GoodQuantity > 0));
                    }
                    else if (assetStatus == AssetStatus.BROKEN)
                    {
                        query = query.Where(a => a.WarehouseAssets.Any(wa => wa.BrokenQuantity > 0));
                    }
                    else if (assetStatus == AssetStatus.FIXING)
                    {
                        query = query.Where(a => a.WarehouseAssets.Any(wa => wa.FixingQuantity > 0));
                    }
                    else if (assetStatus == AssetStatus.DISPOSED)
                    {
                        query = query.Where(a => a.WarehouseAssets.Any(wa => wa.DisposedQuantity > 0));
                    }
                }

                // Áp dụng sắp xếp
                query = ApplySorting(query, sortOrder);

                // Phân trang
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                var paginatedAssets = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                // Add additional information for assets
                var assetsWithInfo = new List<AssetViewModel>();
                foreach (var asset in paginatedAssets)
                {
                    var assetInfo = new AssetViewModel
                    {
                        Asset = asset,
                        TotalQuantity = await _warehouseAssetService.GetTotalAssetQuantityAsync(asset.Id),
                        BorrowedQuantity = await GetBorrowedQuantityAsync(asset.Id),
                        HandedOverQuantity = await GetHandedOverQuantityAsync(asset.Id)
                    };
                    assetsWithInfo.Add(assetInfo);
                }

                return View(assetsWithInfo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<AssetViewModel>());
            }
        }

        // Helper methods for getting quantities
        private async Task<int> GetBorrowedQuantityAsync(int assetId)
        {
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(assetId);
            return warehouseAssets.Sum(wa => wa.BorrowedGoodQuantity ?? 0);
        }

        private async Task<int> GetHandedOverQuantityAsync(int assetId)
        {
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(assetId);
            return warehouseAssets.Sum(wa => wa.HandedOverGoodQuantity ?? 0);
        }

        // Phương thức phụ để áp dụng sắp xếp
        private IQueryable<Asset> ApplySorting(IQueryable<Asset> query, string sortOrder)
        {
            switch (sortOrder)
            {
                case "id_desc":
                    return query.OrderByDescending(a => a.Id);
                case "name_asc":
                    return query.OrderBy(a => a.Name);
                case "name_desc":
                    return query.OrderByDescending(a => a.Name);
                case "price_asc":
                    return query.OrderBy(a => a.Price);
                case "price_desc":
                    return query.OrderByDescending(a => a.Price);
                case "date_asc":
                    return query.OrderBy(a => a.DateCreated);
                case "date_desc":
                    return query.OrderByDescending(a => a.DateCreated);
                default:
                    return query.OrderBy(a => a.Id);
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
            var assetViewModel = new AssetDetailViewModel
            {
                Asset = asset,
                WarehouseAssets = warehouseAssets.ToList(),
                BorrowTickets = (await _borrowTicketService.GetBorrowTicketsByAssetAsync(id.Value)).ToList(),
                HandoverTickets = (await _handoverTicketService.GetHandoverTicketsByAssetIdAsync(id.Value)).ToList(),
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
            var categories = await _assetCategoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
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
                    // Check if an asset with the same name already exists
                    var existingAssets = await _assetService.GetAllAsync();
                    if (existingAssets.Any(a => a.Name.Equals(asset.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại tài sản với tên này. Vui lòng chọn tên khác.");
                        var assetCategories = await _assetCategoryService.GetAllAsync();
                        ViewBag.Categories = new SelectList(assetCategories, "Id", "Name");
                        return View(asset);
                    }

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

            var categories = await _assetCategoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
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

            var categories = await _assetCategoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
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
                    // Check if asset exists and is not deleted
                    var existingAsset = await _assetService.GetByIdIncludeDeletedAsync(id);
                    if (existingAsset == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy tài sản cần cập nhật.";
                        return NotFound();
                    }

                    // Check if another asset with the same name already exists
                    var allAssets = await _assetService.GetAllAsync();
                    if (allAssets.Any(a => a.Id != id && a.Name.Equals(asset.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại tài sản khác với tên này. Vui lòng chọn tên khác.");
                        var assetCategories = await _assetCategoryService.GetAllAsync();
                        ViewBag.Categories = new SelectList(assetCategories, "Id", "Name");
                        return View(asset);
                    }

                    // Preserve original creation date and status data
                    asset.DateCreated = existingAsset.DateCreated;
                    asset.DateModified = DateTime.Now;
                    asset.IsDeleted = existingAsset.IsDeleted;
                    asset.DeletedDate = existingAsset.DeletedDate;

                    // Detach the existing entity to avoid tracking conflicts
                    _context.Entry(existingAsset).State = EntityState.Detached;
                    await _assetService.UpdateAsync(asset);
                    TempData["SuccessMessage"] = $"Tài sản '{asset.Name}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Cập nhật tài sản thất bại: {ex.Message}";
                    if (!await AssetExists(asset.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật tài sản.");
                    }
                }
            }

            var categories = await _assetCategoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
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
                    var warehouseAsset = await _warehouseAssetService.GetByIdAsync(model.WarehouseAssetId);
                    if (warehouseAsset == null)
                    {
                        // If no existing warehouse asset, create one
                        if (model.WarehouseId.HasValue && model.AssetId.HasValue)
                        {
                            warehouseAsset = new WarehouseAsset
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
                    }
                    else
                    {
                        // Update existing warehouse asset
                        warehouseAsset.GoodQuantity = model.GoodQuantity ?? warehouseAsset.GoodQuantity;
                        warehouseAsset.BrokenQuantity = model.BrokenQuantity ?? warehouseAsset.BrokenQuantity;
                        warehouseAsset.FixingQuantity = model.FixingQuantity ?? warehouseAsset.FixingQuantity;
                        warehouseAsset.DisposedQuantity = model.DisposedQuantity ?? warehouseAsset.DisposedQuantity;
                        warehouseAsset.DateModified = DateTime.Now;

                        await _warehouseAssetService.UpdateAsync(warehouseAsset);
                        TempData["SuccessMessage"] = "Cập nhật số lượng tài sản thành công.";
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

        private string GetStatusName(AssetStatus status)
        {
            switch (status)
            {
                case AssetStatus.GOOD: return "Tốt";
                case AssetStatus.BROKEN: return "Hỏng";
                case AssetStatus.FIXING: return "Đang sửa";
                case AssetStatus.DISPOSED: return "Đã thanh lý";
                default: return status.ToString();
            }
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

            // Check if any of this asset is currently borrowed or handed over
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(id.Value);
            var isBorrowed = warehouseAssets.Any(wa => (wa.BorrowedGoodQuantity ?? 0) > 0);
            var isHandedOver = warehouseAssets.Any(wa => (wa.HandedOverGoodQuantity ?? 0) > 0);

            if (isBorrowed || isHandedOver)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài sản này vì đang được mượn hoặc bàn giao.";
                return RedirectToAction(nameof(Index));
            }

            // Check if asset is already deleted
            if (asset.IsDeleted)
            {
                // Set warning message and stay on the details page
                TempData["WarningMessage"] = "Tài sản này đã bị xóa trước đó.";
                return View(asset); // Stay on the delete view
            }

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get asset including deleted ones to check if it's already deleted
            var asset = await _assetService.GetByIdIncludeDeletedAsync(id);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                return RedirectToAction(nameof(Index));
            }

            // Check if any of this asset is currently borrowed or handed over
            var warehouseAssets = await _warehouseAssetService.GetWarehouseAssetsByAssetAsync(id);
            var isBorrowed = warehouseAssets.Any(wa => (wa.BorrowedGoodQuantity ?? 0) > 0);
            var isHandedOver = warehouseAssets.Any(wa => (wa.HandedOverGoodQuantity ?? 0) > 0);

            if (isBorrowed || isHandedOver)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài sản này vì đang được mượn hoặc bàn giao.";
                return RedirectToAction(nameof(Index));
            }

            // Check if asset is already deleted
            if (asset.IsDeleted)
            {
                // Set warning message and stay on the details page
                TempData["WarningMessage"] = "Tài sản này đã bị xóa trước đó.";
                return View(asset); // Stay on the delete view
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
                return View(asset); // Stay on the delete view
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
                return RedirectToAction(nameof(Index), new { includeDeleted = true });
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
                return RedirectToAction(nameof(Index), new { includeDeleted = true });
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
                return RedirectToAction(nameof(Index), new { includeDeleted = true });
            }
        }

        // Phương thức kiểm tra tài sản tồn tại
        private async Task<bool> AssetExists(int id)
        {
            var asset = await _assetService.GetByIdAsync(id);
            return asset != null;
        }
    }
}