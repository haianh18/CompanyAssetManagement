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

        public async Task<WarehouseAsset> GetWarehouseAssetByWarehouseAndAsset(int warehouseId, int assetId)
        {
            return await _dbSet
                .Where(wa => wa.WarehouseId == warehouseId && wa.AssetId == assetId)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .FirstOrDefaultAsync();
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

            return true;
        }

        public async Task<bool> UpdateBorrowedQuantity(int warehouseAssetId, int quantityChange)
        {
            var warehouseAsset = await _dbSet.FindAsync(warehouseAssetId);
            if (warehouseAsset == null) return false;

            // If decreasing, check if there are enough borrowed assets
            if (quantityChange < 0 && (warehouseAsset.BorrowedGoodQuantity + quantityChange) < 0) return false;

            warehouseAsset.BorrowedGoodQuantity += quantityChange;
            return true;
        }

        public async Task<bool> UpdateHandedOverQuantity(int warehouseAssetId, int quantityChange)
        {
            var warehouseAsset = await _dbSet.FindAsync(warehouseAssetId);
            if (warehouseAsset == null) return false;

            // If decreasing, check if there are enough handed over assets
            if (quantityChange < 0 && (warehouseAsset.HandedOverGoodQuantity + quantityChange) < 0) return false;

            warehouseAsset.HandedOverGoodQuantity += quantityChange;
            return true;
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
    }
}