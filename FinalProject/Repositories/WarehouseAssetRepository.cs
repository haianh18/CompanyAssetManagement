using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{
    public class WarehouseAssetRepository : Repository<WarehouseAsset>, IWarehouseAssetRepository
    {
        public WarehouseAssetRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByAssetIncludingDeleted(int assetId)
        {
            return await _context.WarehouseAssets
                .Include(wa => wa.Warehouse)
                .Where(wa => wa.AssetId == assetId)
                .ToListAsync();
        }

        public override async Task<IEnumerable<WarehouseAsset>> GetAllAsync()
        {
            return await _dbSet
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public override async Task<IEnumerable<WarehouseAsset>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByWarehouse(int warehouseId)
        {
            return await _dbSet
                .Where(wa => wa.WarehouseId == warehouseId)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByAsset(int assetId)
        {
            return await _dbSet
                .Where(wa => wa.AssetId == assetId)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<WarehouseAsset> GetWarehouseAssetByWarehouseAndAsset(int assetId, int warehouseId)
        {
            var asset = _dbSet
                 .Include(wa => wa.Asset)
        .Include(wa => wa.Warehouse)
        .FirstOrDefaultAsync(wa => wa.AssetId == assetId && wa.WarehouseId == warehouseId);
            if (asset != null)
                return await asset;
            else return null;
        }

        public async Task<bool> UpdateAssetStatusQuantity(int warehouseAssetId, AssetStatus fromStatus, AssetStatus toStatus, int quantity)
        {
            var warehouseAsset = await _dbSet.FindAsync(warehouseAssetId);
            if (warehouseAsset == null) return false;

            // Decrement from source status
            switch (fromStatus)
            {
                case AssetStatus.GOOD:
                    if ((warehouseAsset.GoodQuantity ?? 0) < quantity) return false;
                    warehouseAsset.GoodQuantity -= quantity;
                    break;
                case AssetStatus.BROKEN:
                    if ((warehouseAsset.BrokenQuantity ?? 0) < quantity) return false;
                    warehouseAsset.BrokenQuantity -= quantity;
                    break;
                case AssetStatus.FIXING:
                    if ((warehouseAsset.FixingQuantity ?? 0) < quantity) return false;
                    warehouseAsset.FixingQuantity -= quantity;
                    break;
                case AssetStatus.DISPOSED:
                    if ((warehouseAsset.DisposedQuantity ?? 0) < quantity) return false;
                    warehouseAsset.DisposedQuantity -= quantity;
                    break;
                default:
                    return false;
            }

            // Increment to target status
            switch (toStatus)
            {
                case AssetStatus.GOOD:
                    warehouseAsset.GoodQuantity = (warehouseAsset.GoodQuantity ?? 0) + quantity;
                    break;
                case AssetStatus.BROKEN:
                    warehouseAsset.BrokenQuantity = (warehouseAsset.BrokenQuantity ?? 0) + quantity;
                    break;
                case AssetStatus.FIXING:
                    warehouseAsset.FixingQuantity = (warehouseAsset.FixingQuantity ?? 0) + quantity;
                    break;
                case AssetStatus.DISPOSED:
                    warehouseAsset.DisposedQuantity = (warehouseAsset.DisposedQuantity ?? 0) + quantity;
                    break;
                default:
                    return false;
            }

            warehouseAsset.DateModified = DateTime.Now;
            _context.Entry(warehouseAsset).State = EntityState.Modified;

            return true;
        }

        public async Task<bool> UpdateBorrowedQuantity(int warehouseAssetId, int quantityChange)
        {
            var warehouseAsset = await _dbSet.FindAsync(warehouseAssetId);
            if (warehouseAsset == null) return false;

            // If decreasing, check if there are enough borrowed assets
            if (quantityChange < 0 && (warehouseAsset.BorrowedGoodQuantity + quantityChange) < 0) return false;

            warehouseAsset.BorrowedGoodQuantity += quantityChange;
            warehouseAsset.DateModified = DateTime.Now;

            _context.Entry(warehouseAsset).State = EntityState.Modified;

            return true;
        }

        public async Task<bool> UpdateHandedOverQuantity(int warehouseAssetId, int quantityChange)
        {
            var warehouseAsset = await _dbSet.FindAsync(warehouseAssetId);
            if (warehouseAsset == null) return false;

            // If decreasing, check if there are enough handed over assets
            if (quantityChange < 0 && (warehouseAsset.HandedOverGoodQuantity + quantityChange) < 0) return false;

            warehouseAsset.HandedOverGoodQuantity += quantityChange;
            warehouseAsset.DateModified = DateTime.Now;

            _context.Entry(warehouseAsset).State = EntityState.Modified;

            return true;
        }
        public async Task<IEnumerable<WarehouseAsset>> GetAssetsWithNonZeroQuantity()
        {
            return await _context.WarehouseAssets
                .Include(wa => wa.Asset)
                .Include(wa => wa.Warehouse)
                .Where(wa =>
                    (wa.GoodQuantity > 0 || wa.BrokenQuantity > 0 || wa.FixingQuantity > 0) &&
                    !wa.IsDeleted)
                .ToListAsync();
        }
        public async Task<IEnumerable<WarehouseAsset>> GetAssetsWithAvailableQuantity()
        {
            return await _dbSet
                .Where(wa => (wa.GoodQuantity ?? 0) - (wa.BorrowedGoodQuantity ?? 0) - (wa.HandedOverGoodQuantity ?? 0) > 0)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<WarehouseAsset>> GetAssetsWithGoodQuantity()
        {
            return await _dbSet
                .Where(wa => wa.GoodQuantity > 0)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<WarehouseAsset>> GetAssetsWithBrokenQuantity()
        {
            return await _dbSet
                .Where(wa => wa.BrokenQuantity > 0)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<WarehouseAsset>> GetAssetsWithFixingQuantity()
        {
            return await _dbSet
                .Where(wa => wa.FixingQuantity > 0)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<WarehouseAsset>> GetAssetsWithDisposedQuantity()
        {
            return await _dbSet
                .Where(wa => wa.DisposedQuantity > 0)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }

        // New method from HandoverService
        public async Task UpdateWarehouseAssetQuantitiesForHandover(int warehouseAssetId, int quantityChange, bool isReturn, AssetStatus status)
        {
            // Cập nhật số lượng tài sản trong kho
            if (isReturn)
            {
                // Trả lại tài sản - giảm số lượng đã bàn giao
                await UpdateHandedOverQuantity(warehouseAssetId, -quantityChange);

                // Cập nhật số lượng tài sản dựa trên trạng thái khi trả
                if (status == AssetStatus.GOOD)
                {
                    // Nếu trả về trong tình trạng tốt, thêm vào số lượng tốt
                    await UpdateAssetStatusQuantity(
                        warehouseAssetId,
                        AssetStatus.GOOD,
                        AssetStatus.GOOD,
                        quantityChange);
                }
                else
                {
                    // Nếu trả về trong tình trạng khác, chuyển từ tốt sang trạng thái đó
                    await UpdateAssetStatusQuantity(
                        warehouseAssetId,
                        AssetStatus.GOOD,
                        status,
                        quantityChange);
                }
            }
            else
            {
                // Bàn giao mới - tăng số lượng đã bàn giao
                await UpdateHandedOverQuantity(warehouseAssetId, quantityChange);
            }
        }
    }
}