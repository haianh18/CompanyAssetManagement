using FinalProject.Enums;
using FinalProject.Models;
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

        private readonly CompanyAssetManagementContext _context;

        public AssetsController(
            IAssetService assetService,
            IAssetCategoryService assetCategoryService,
            IWarehouseAssetService warehouseAssetService,
            CompanyAssetManagementContext context)
        {
            _assetService = assetService;
            _assetCategoryService = assetCategoryService;
            _warehouseAssetService = warehouseAssetService;
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
                    query = query.Where(a => a.AssetStatus == assetStatus);
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

                return View(paginatedAssets.ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<Asset>());
            }
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

            return View(asset);
        }

        // GET: Assets/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _assetCategoryService.GetAllAsync();
            ViewBag.Categories = categories;
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
                        ViewBag.Categories = assetCategories;
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
            ViewBag.Categories = categories;
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
            ViewBag.Categories = categories;
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
                        ViewBag.Categories = assetCategories;
                        return View(asset);
                    }

                    // Preserve original creation date
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
            ViewBag.Categories = categories;
            return View(asset);
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

            // Check if asset is already deleted
            if (asset.IsDeleted)
            {
                // Set warning message and stay on the delete view
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
