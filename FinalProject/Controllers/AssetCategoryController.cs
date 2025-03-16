using FinalProject.Models;
using FinalProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    //[Authorize(Roles = "WarehouseManager")]
    public class AssetCategoryController : Controller
    {
        private readonly IAssetCategoryService _assetCategoryService;
        private readonly CompanyAssetManagementContext _context;

        public AssetCategoryController(
            IAssetCategoryService assetCategoryService,
            CompanyAssetManagementContext context)
        {
            _assetCategoryService = assetCategoryService;
            _context = context;
        }

        // GET: AssetCategory
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page, bool showDeleted = false)
        {
            // Lưu trạng thái sắp xếp hiện tại để sử dụng trong view
            ViewBag.CurrentSort = sortOrder;

            // Định nghĩa các tham số sắp xếp
            ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            // Xử lý reset pagination khi có tìm kiếm mới
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            // Lưu giá trị filter cho view
            ViewBag.CurrentFilter = searchString;
            ViewBag.ShowDeleted = showDeleted;

            try
            {
                // Lấy dữ liệu danh mục
                IEnumerable<AssetCategory> categories;

                // Get categories based on filter
                if (showDeleted)
                {
                    categories = await _assetCategoryService.GetAllInCludeDeletedAsync();
                    // Filter to only show deleted categories
                    categories = categories.Where(c => c.IsDeleted);
                }
                else
                {
                    categories = await _assetCategoryService.GetAllAsync();
                }

                var query = categories.AsQueryable();

                // Áp dụng bộ lọc tìm kiếm nếu có
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(c =>
                        c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase));
                }

                // Áp dụng sắp xếp
                switch (sortOrder)
                {
                    case "id_desc":
                        query = query.OrderByDescending(c => c.Id);
                        break;
                    case "name_asc":
                        query = query.OrderBy(c => c.Name);
                        break;
                    case "name_desc":
                        query = query.OrderByDescending(c => c.Name);
                        break;
                    case "date_asc":
                        query = query.OrderBy(c => c.DateCreated);
                        break;
                    case "date_desc":
                        query = query.OrderByDescending(c => c.DateCreated);
                        break;
                    default:
                        query = query.OrderBy(c => c.Id);
                        break;
                }

                // Thiết lập phân trang
                int pageSize = 10;
                int pageNumber = page ?? 1;
                int totalItems = query.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Lưu thông tin phân trang cho view
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                // Thực hiện phân trang
                var paginatedCategories = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return View(paginatedCategories.ToList());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<AssetCategory>());
            }
        }

        // GET: AssetCategory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _assetCategoryService.GetByIdIncludeDeletedAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: AssetCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AssetCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetCategory category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a category with the same name already exists
                    var existingCategories = await _assetCategoryService.GetAllAsync();
                    if (existingCategories.Any(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại danh mục với tên này. Vui lòng chọn tên khác.");
                        return View(category);
                    }

                    category.DateCreated = DateTime.Now;
                    await _assetCategoryService.AddAsync(category);
                    TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được tạo thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Tạo danh mục thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo danh mục.");
                }
            }

            return View(category);
        }

        // GET: AssetCategory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _assetCategoryService.GetByIdIncludeDeletedAsync(id.Value);

            if (category.Name == "Chưa phân loại")
            {
                TempData["ErrorMessage"] = "Danh mục 'Chưa phân loại' không thể bị chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: AssetCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetCategory category)
        {
            if (id != category.Id)
            {
                TempData["ErrorMessage"] = "ID danh mục không khớp.";
                return NotFound();
            }

            if (category.Name == "Chưa phân loại")
            {
                TempData["ErrorMessage"] = "Danh mục 'Chưa phân loại' không thể bị chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin danh mục hiện tại
                    var existingCategory = await _assetCategoryService.GetByIdIncludeDeletedAsync(id);
                    if (existingCategory == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy danh mục cần cập nhật.";
                        return NotFound();
                    }

                    // Check if another category with the same name already exists
                    var allCategories = await _assetCategoryService.GetAllAsync();
                    if (allCategories.Any(c => c.Id != id && c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Name", "Đã tồn tại danh mục khác với tên này. Vui lòng chọn tên khác.");
                        return View(category);
                    }

                    // Bảo toàn các thông tin quan trọng
                    category.DateCreated = existingCategory.DateCreated;
                    category.DateModified = DateTime.Now;
                    category.IsDeleted = existingCategory.IsDeleted;
                    category.DeletedDate = existingCategory.DeletedDate;

                    // Detach entity hiện tại
                    _context.Entry(existingCategory).State = EntityState.Detached;

                    await _assetCategoryService.UpdateAsync(category);
                    TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Cập nhật danh mục thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật danh mục.");
                }
            }

            return View(category);
        }

        // GET: AssetCategory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _assetCategoryService.GetByIdIncludeDeletedAsync(id.Value);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục với ID này.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra nếu là danh mục "Chưa phân loại"
            if (category.Name == "Chưa phân loại")
            {
                TempData["ErrorMessage"] = "Danh mục 'Chưa phân loại' không thể bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            if (category.IsDeleted)
            {
                TempData["WarningMessage"] = "Danh mục này đã bị xóa trước đó.";
            }

            return View(category);
        }

        // POST: AssetCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _assetCategoryService.GetByIdIncludeDeletedAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục với ID này.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra nếu là danh mục "Chưa phân loại"
            if (category.Name == "Chưa phân loại")
            {
                TempData["ErrorMessage"] = "Danh mục 'Chưa phân loại' không thể bị xóa.";
                return RedirectToAction(nameof(Index));
            }

            if (category.IsDeleted)
            {
                TempData["WarningMessage"] = "Danh mục này đã bị xóa trước đó.";
                return View(category);
            }

            try
            {
                await _assetCategoryService.SoftDeleteCategoryAsync(id);
                TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được xóa thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Xóa danh mục thất bại: {ex.Message}";
                return View(category);
            }
        }

        // GET: AssetCategory/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _assetCategoryService.GetByIdIncludeDeletedAsync(id.Value);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục với ID này.";
                return RedirectToAction(nameof(Index), new { includeDeleted = true });
            }

            if (!category.IsDeleted)
            {
                TempData["WarningMessage"] = "Danh mục này chưa bị xóa, không cần khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: AssetCategory/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var category = await _assetCategoryService.GetByIdIncludeDeletedAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục với ID này.";
                return RedirectToAction(nameof(Index), new { includeDeleted = true });
            }

            if (!category.IsDeleted)
            {
                TempData["WarningMessage"] = "Danh mục này chưa bị xóa, không cần khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _assetCategoryService.RestoreAsync(id);
                TempData["SuccessMessage"] = $"Danh mục '{category.Name}' đã được khôi phục thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Khôi phục danh mục thất bại: {ex.Message}";
                return RedirectToAction(nameof(Index), new { includeDeleted = true });
            }
        }

        private async Task<bool> CategoryExists(int id)
        {
            var category = await _assetCategoryService.GetByIdAsync(id);
            return category != null;
        }
    }
}