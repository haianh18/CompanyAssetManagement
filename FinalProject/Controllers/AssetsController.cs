using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using FinalProject.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Authorize(Roles = "WarehouseManager")]
    public class AssetsController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public AssetsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Assets/Index
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString,
     string status, int? warehouseId, int? page, bool showDeleted = false)
        {
            try
            {
                // Set up sorting parameters
                ViewBag.CurrentSort = sortOrder;
                ViewBag.IdSortParam = string.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
                ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";

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
                ViewBag.WarehouseId = warehouseId;
                ViewBag.ShowDeleted = showDeleted;

                // Lưu các tham số vào TempData để giữ giữa các request
                TempData["SearchString"] = searchString;

                // Load warehouses for dropdown
                ViewBag.Warehouses = await _unitOfWork.Warehouses.GetAllAsync();

                // Tạo query
                IEnumerable<WarehouseAsset> warehouseAssets;
                IQueryable<WarehouseAsset> query;

                // With or Without deleted items
                if (showDeleted)
                {
                    warehouseAssets = await _unitOfWork.WarehouseAssets.GetAllIncludingDeletedAsync();
                    // Đảm bảo rằng warehouseAssets không null trước khi sử dụng
                    warehouseAssets = warehouseAssets ?? Enumerable.Empty<WarehouseAsset>();
                    query = warehouseAssets.Where(wa => wa.IsDeleted).AsQueryable();
                }
                else
                {
                    warehouseAssets = await _unitOfWork.WarehouseAssets.GetAllAsync();
                    // Đảm bảo rằng warehouseAssets không null trước khi sử dụng
                    warehouseAssets = warehouseAssets ?? Enumerable.Empty<WarehouseAsset>();
                    query = warehouseAssets.AsQueryable();
                }

                // Lọc theo tìm kiếm - xử lý trường hợp thuộc tính null
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(wa =>
                        (wa.Asset != null && wa.Asset.Name != null && wa.Asset.Name.Contains(searchString)) ||
                        (wa.Asset != null && wa.Asset.Description != null && wa.Asset.Description.Contains(searchString)) ||
                        (wa.Warehouse != null && wa.Warehouse.Name != null && wa.Warehouse.Name.Contains(searchString))
                    );
                }

                // Lọc theo warehouse nếu có
                if (warehouseId.HasValue && warehouseId.Value > 0)
                {
                    query = query.Where(wa => wa.WarehouseId == warehouseId.Value);
                }

                // Lọc theo trạng thái nếu không ở chế độ hiển thị đã xóa
                if (!showDeleted && !string.IsNullOrEmpty(status) && Enum.TryParse<AssetStatus>(status, out var assetStatus))
                {
                    switch (assetStatus)
                    {
                        case AssetStatus.GOOD:
                            query = query.Where(wa => wa.GoodQuantity.HasValue && wa.GoodQuantity > 0);
                            break;
                        case AssetStatus.BROKEN:
                            query = query.Where(wa => wa.BrokenQuantity.HasValue && wa.BrokenQuantity > 0);
                            break;
                        case AssetStatus.FIXING:
                            query = query.Where(wa => wa.FixingQuantity.HasValue && wa.FixingQuantity > 0);
                            break;
                        case AssetStatus.DISPOSED:
                            query = query.Where(wa => wa.DisposedQuantity.HasValue && wa.DisposedQuantity > 0);
                            break;
                    }
                }

                // Apply sorting
                query = ApplySorting(query, sortOrder);

                // Đảm bảo query không null trước khi đếm
                if (query == null)
                {
                    query = Enumerable.Empty<WarehouseAsset>().AsQueryable();
                }

                // Áp dụng phân trang - đảm bảo không bị lỗi khi đếm
                int pageSize = 10;
                int pageNumber = (page ?? 1);

                // Sử dụng cách an toàn hơn để đếm tổng số mục
                int totalItems;
                try
                {
                    // Convert to list first to avoid IQueryable execution errors
                    var queryList = query.ToList();
                    totalItems = queryList.Count;

                    // Now create a new query from this list
                    query = queryList.AsQueryable();
                }
                catch
                {
                    totalItems = 0;
                    query = Enumerable.Empty<WarehouseAsset>().AsQueryable();
                }

                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                var paginatedItems = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                // Tạo view models với xử lý null an toàn
                var viewModels = paginatedItems.Select(wa => new WarehouseAssetViewModel
                {
                    Asset = wa.Asset,
                    WarehouseAsset = wa,
                    WarehouseName = wa.Warehouse.Name ?? "Unknown",
                    TotalGoodQuantity = wa.GoodQuantity ?? 0,
                    TotalBrokenQuantity = wa.BrokenQuantity ?? 0,
                    TotalFixingQuantity = wa.FixingQuantity ?? 0,
                    TotalDisposedQuantity = wa.DisposedQuantity ?? 0,
                    TotalQuantity = wa.TotalQuantity,
                    AvailableQuantity = wa.AvailableQuantity
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi khi tải dữ liệu: {ex.Message}";
                return View(new List<WarehouseAssetViewModel>());
            }
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asset = await _unitOfWork.Assets.GetByIdIncludingDeletedAsync(id.Value);
            if (asset == null)
            {
                return NotFound();
            }

            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetWarehouseAssetsByAsset(id.Value);
            var borrowTickets = await _unitOfWork.BorrowTickets.GetBorrowTicketsByAsset(id.Value);
            var handoverTickets = await _unitOfWork.HandoverTickets.GetHandoverTicketsByAssetIdAsync(id.Value);

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
            // Chuẩn bị danh mục
            await PrepareAssetCategoriesForView();

            // Chuẩn bị danh sách kho
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();

            // Tìm kho chính (nếu có)
            var mainWarehouse = warehouses.FirstOrDefault(w => w.Name.Equals("Main Warehouse")) ?? warehouses.FirstOrDefault();

            // Tạo SelectList và đặt kho chính làm mặc định
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", mainWarehouse?.Id);

            // Tạo ViewModel với giá trị mặc định
            var viewModel = new AssetCreateViewModel();

            // Nếu có kho chính, đặt mặc định
            if (mainWarehouse != null)
            {
                viewModel.WarehouseId = mainWarehouse.Id;
            }

            return View(viewModel);
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetCreateViewModel viewModel)
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                await PrepareAssetCategoriesForView();
                var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
                ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");
                return View(viewModel);
            }

            try
            {
                // Đặt các giá trị mặc định cho tài sản
                viewModel.Asset.DateCreated = DateTime.Now;
                viewModel.Asset.IsDeleted = false;

                // Thêm tài sản mới
                await _unitOfWork.Assets.AddAsync(viewModel.Asset);
                await _unitOfWork.SaveChangesAsync();

                // Luôn thêm vào kho đã chọn với số lượng tốt
                var warehouseAsset = new WarehouseAsset
                {
                    AssetId = viewModel.Asset.Id,
                    WarehouseId = viewModel.WarehouseId,  // Bây giờ là required, không cần kiểm tra null
                    GoodQuantity = viewModel.GoodQuantity,
                    BrokenQuantity = 0,  // Mặc định là 0, giá trị từ form bị bỏ qua
                    FixingQuantity = 0,
                    DisposedQuantity = 0,
                    DateCreated = DateTime.Now,
                    IsDeleted = false
                };

                await _unitOfWork.WarehouseAssets.AddAsync(warehouseAsset);
                await _unitOfWork.SaveChangesAsync();


                TempData["SuccessMessage"] = $"Tài sản '{viewModel.Asset.Name}' đã được tạo thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi tạo tài sản: {ex.Message}");

                await PrepareAssetCategoriesForView();
                var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
                ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");

                return View(viewModel);
            }
        }


        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var asset = await _unitOfWork.Assets.GetByIdIncludingDeletedAsync(id.Value);

                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                    return NotFound();
                }

                // Chuẩn bị danh mục cho dropdown
                await PrepareAssetCategoriesForView();

                // Lấy danh sách WarehouseAsset của tài sản này
                var warehouseAssets = await _unitOfWork.WarehouseAssets.GetWarehouseAssetsByAsset(id.Value);

                // Lấy tất cả các kho
                var allWarehouses = await _unitOfWork.Warehouses.GetAllAsync();

                // Tạo danh sách các kho mà tài sản này chưa có (để hiển thị trong dropdown thêm mới)
                var existingWarehouseIds = warehouseAssets.Select(wa => wa.WarehouseId).ToList();
                var availableWarehouses = allWarehouses.Where(w => !existingWarehouseIds.Contains(w.Id)).ToList();

                ViewBag.AvailableWarehouses = new SelectList(availableWarehouses, "Id", "Name");

                // Tạo ViewModel
                var viewModel = new AssetEditViewModel
                {
                    Asset = asset,
                    WarehouseQuantities = warehouseAssets.Select(wa => new WarehouseAssetQuantityViewModel
                    {
                        WarehouseAssetId = wa.Id,
                        WarehouseId = wa.Warehouse.Id,
                        WarehouseName = wa.Warehouse?.Name ?? "Unknown",
                        GoodQuantity = wa.GoodQuantity ?? 0,
                        BrokenQuantity = wa.BrokenQuantity ?? 0,
                        FixingQuantity = wa.FixingQuantity ?? 0,
                        DisposedQuantity = wa.DisposedQuantity ?? 0
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetEditViewModel viewModel)
        {
            if (id != viewModel.Asset.Id)
            {
                TempData["ErrorMessage"] = "ID tài sản không khớp.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing asset for comparison
                    var existingAsset = await _unitOfWork.Assets.GetByIdIncludingDeletedAsync(id);
                    if (existingAsset == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy tài sản cần cập nhật.";
                        return NotFound();
                    }

                    // Check for duplicate names except self
                    var allAssets = await _unitOfWork.Assets.GetAllAsync();
                    if (allAssets.Any(a => a.Id != id && a.Name.Equals(viewModel.Asset.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        ModelState.AddModelError("Asset.Name", "Đã tồn tại tài sản khác với tên này. Vui lòng chọn tên khác.");
                        await PrepareEditViewData(id);
                        return View(viewModel);
                    }

                    // Cập nhật thông tin cơ bản của tài sản
                    existingAsset.Name = viewModel.Asset.Name;
                    existingAsset.Description = viewModel.Asset.Description;
                    existingAsset.Note = viewModel.Asset.Note;
                    existingAsset.Price = viewModel.Asset.Price;
                    existingAsset.Unit = viewModel.Asset.Unit;
                    existingAsset.AssetCategoryId = viewModel.Asset.AssetCategoryId;
                    existingAsset.DateModified = DateTime.Now;

                    _unitOfWork.Assets.Update(existingAsset);
                    await _unitOfWork.SaveChangesAsync();

                    // Cập nhật số lượng trong các kho hiện có
                    foreach (var warehouseQuantity in viewModel.WarehouseQuantities)
                    {
                        var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(warehouseQuantity.WarehouseAssetId);
                        if (warehouseAsset != null)
                        {
                            warehouseAsset.GoodQuantity = warehouseQuantity.GoodQuantity;
                            warehouseAsset.BrokenQuantity = warehouseQuantity.BrokenQuantity;
                            warehouseAsset.FixingQuantity = warehouseQuantity.FixingQuantity;
                            warehouseAsset.DisposedQuantity = warehouseQuantity.DisposedQuantity;
                            warehouseAsset.DateModified = DateTime.Now;

                            _unitOfWork.WarehouseAssets.Update(warehouseAsset);
                        }
                    }
                    await _unitOfWork.SaveChangesAsync();

                    // Thêm vào kho mới nếu có
                    if (viewModel.NewWarehouseId.HasValue &&
                        (viewModel.NewGoodQuantity > 0 || viewModel.NewBrokenQuantity > 0 ||
                        viewModel.NewFixingQuantity > 0 || viewModel.NewDisposedQuantity > 0))
                    {
                        var newWarehouseAsset = new WarehouseAsset
                        {
                            AssetId = id,
                            WarehouseId = viewModel.NewWarehouseId.Value,
                            GoodQuantity = viewModel.NewGoodQuantity,
                            BrokenQuantity = viewModel.NewBrokenQuantity,
                            FixingQuantity = viewModel.NewFixingQuantity,
                            DisposedQuantity = viewModel.NewDisposedQuantity,
                            DateCreated = DateTime.Now,
                            IsDeleted = false
                        };

                        await _unitOfWork.WarehouseAssets.AddAsync(newWarehouseAsset);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = $"Tài sản '{existingAsset.Name}' đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Cập nhật tài sản thất bại: {ex.Message}";
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật tài sản.");
                }
            }

            await PrepareEditViewData(id);
            return View(viewModel);
        }


        // POST: Assets/TransferAssetStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferAssetStatus(AssetStatusTransferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToAction("Details", new { id = model.AssetId });
            }

            if (model.FromStatus == model.ToStatus)
            {
                TempData["ErrorMessage"] = "Trạng thái đích không thể giống trạng thái nguồn.";
                return RedirectToAction("Details", new { id = model.AssetId });
            }

            var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdAsync(model.WarehouseAssetId);
            if (warehouseAsset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản trong kho.";
                return RedirectToAction("Details", new { id = model.AssetId });
            }

            // Validate quantity
            int availableQuantity = model.FromStatus switch
            {
                AssetStatus.GOOD => warehouseAsset.GoodQuantity ?? 0,
                AssetStatus.BROKEN => warehouseAsset.BrokenQuantity ?? 0,
                AssetStatus.FIXING => warehouseAsset.FixingQuantity ?? 0,
                AssetStatus.DISPOSED => warehouseAsset.DisposedQuantity ?? 0,
                _ => 0
            };

            if (model.Quantity > availableQuantity)
            {
                TempData["ErrorMessage"] = "Số lượng chuyển trạng thái vượt quá số lượng hiện có.";
                return RedirectToAction("Details", new { id = model.AssetId });
            }

            // Update quantities
            bool updateResult = await _unitOfWork.WarehouseAssets.UpdateAssetStatusQuantity(model.WarehouseAssetId, model.FromStatus, model.ToStatus, model.Quantity);
            if (!updateResult)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái tài sản.";
                return RedirectToAction("Details", new { id = model.AssetId });
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["SuccessMessage"] = "Trạng thái tài sản đã được cập nhật thành công.";
            return RedirectToAction("Details", new { id = model.AssetId });
        }


        // GET: Assets/Delete/5?2
        public async Task<IActionResult> Delete(int? id, int? warehouseId)
        {
            if (id == null || warehouseId == null)
            {
                return NotFound();
            }

            var asset = await _unitOfWork.Assets.GetByIdAsync(id.Value);
            if (asset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                return RedirectToAction(nameof(Index));
            }

            var warehouseAsset = await _unitOfWork.WarehouseAssets.GetWarehouseAssetByWarehouseAndAsset(id.Value, warehouseId.Value);
            if (warehouseAsset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài sản trong kho này.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.WarehouseAsset = warehouseAsset;
            ViewBag.Warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId.Value);

            return View(asset);
        }

        // POST: Assets/Delete/5?2
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int warehouseId)
        {
            try
            {
                // Get asset 
                var asset = await _unitOfWork.Assets.GetByIdAsync(id);
                if (asset == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài sản với ID này.";
                    return RedirectToAction(nameof(Index));
                }

                // Chỉ xóa liên kết giữa tài sản và kho được chọn
                var warehouseAsset = await _unitOfWork.WarehouseAssets.GetWarehouseAssetByWarehouseAndAsset(id, warehouseId);
                if (warehouseAsset == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy tài sản trong kho này.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra nếu tài sản đang được mượn hoặc bàn giao từ kho này
                if ((warehouseAsset.BorrowedGoodQuantity ?? 0) > 0 || (warehouseAsset.HandedOverGoodQuantity ?? 0) > 0)
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài sản này vì đang được mượn hoặc bàn giao từ kho này.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete chỉ bản ghi WarehouseAsset
                await _unitOfWork.WarehouseAssets.SoftDeleteAsync(warehouseAsset.Id);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tài sản '{asset.Name}' đã được xóa khỏi kho thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Xóa tài sản thất bại: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Assets/Restore/5
        public async Task<IActionResult> Restore(int? id, int? assetId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdIncludingDeletedAsync(id.Value);
            if (warehouseAsset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi với ID này.";
                return RedirectToAction(nameof(Index), new { showDeleted = true });
            }

            if (!warehouseAsset.IsDeleted)
            {
                TempData["WarningMessage"] = "Bản ghi này chưa bị xóa, không cần khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy thông tin tài sản để hiển thị
            var asset = assetId.HasValue ? await _unitOfWork.Assets.GetByIdAsync(assetId.Value) : null;
            ViewBag.Asset = asset;
            ViewBag.Warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseAsset.WarehouseId.Value);

            return View(warehouseAsset);
        }

        // POST: Assets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var warehouseAsset = await _unitOfWork.WarehouseAssets.GetByIdIncludingDeletedAsync(id);
            if (warehouseAsset == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi với ID này.";
                return RedirectToAction(nameof(Index), new { showDeleted = true });
            }

            if (!warehouseAsset.IsDeleted)
            {
                TempData["WarningMessage"] = "Bản ghi này chưa bị xóa, không cần khôi phục.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Khôi phục chỉ bản ghi WarehouseAsset
                await _unitOfWork.WarehouseAssets.RestoreDeletedAsync(id);
                await _unitOfWork.SaveChangesAsync();

                // Lấy thông tin tài sản để hiển thị thông báo
                var asset = await _unitOfWork.Assets.GetByIdAsync(warehouseAsset.AssetId.Value);
                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseAsset.WarehouseId.Value);

                TempData["SuccessMessage"] = $"Tài sản '{asset?.Name ?? "N/A"}' đã được khôi phục thành công vào kho '{warehouse?.Name ?? "N/A"}'.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Khôi phục bản ghi thất bại: {ex.Message}";
                return RedirectToAction(nameof(Index), new { showDeleted = true });
            }
        }

        #region Helper Methods

        // Helper method to check if asset exists
        private async Task<bool> AssetExists(int id)
        {
            var asset = await _unitOfWork.Assets.GetByIdAsync(id);
            return asset != null;
        }

        // Helper method để chuẩn bị dữ liệu cho Edit view
        private async Task PrepareEditViewData(int assetId)
        {
            await PrepareAssetCategoriesForView();

            var warehouseAssets = await _unitOfWork.WarehouseAssets.GetWarehouseAssetsByAsset(assetId);
            var allWarehouses = await _unitOfWork.Warehouses.GetAllAsync();

            var existingWarehouseIds = warehouseAssets.Select(wa => wa.WarehouseId).ToList();
            var availableWarehouses = allWarehouses.Where(w => !existingWarehouseIds.Contains(w.Id)).ToList();

            ViewBag.AvailableWarehouses = new SelectList(availableWarehouses, "Id", "Name");
        }

        // Helper method to apply sorting
        private IQueryable<WarehouseAsset> ApplySorting(IQueryable<WarehouseAsset> query, string sortOrder)
        {
            return sortOrder switch
            {
                "id_desc" => query.OrderByDescending(a => a.Asset.Id),
                "name_asc" => query.OrderBy(a => a.Asset.Name),
                "name_desc" => query.OrderByDescending(a => a.Asset.Name),
                _ => query.OrderBy(a => a.Id)
            };
        }


        // Helper method to prepare asset categories for view
        private async Task PrepareAssetCategoriesForView()
        {
            var categories = await _unitOfWork.AssetCategories.GetAllAsync();

            // Ensure non-null categories
            if (categories == null)
            {
                categories = Enumerable.Empty<AssetCategory>();
            }
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }

        #endregion
    }
}