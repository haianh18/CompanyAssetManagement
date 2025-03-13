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

        public async Task<bool> UpdateQuantity(int warehouseAssetId, int quantityChange)
        {
            var warehouseAsset = await _dbSet.FindAsync(warehouseAssetId);
            if (warehouseAsset == null) return false;

            // Nếu quantityChange âm, kiểm tra xem sau khi thay đổi có nhỏ hơn 0 không
            if (quantityChange < 0 && (warehouseAsset.Quantity + quantityChange) < 0) return false;

            warehouseAsset.Quantity += quantityChange;
            return true;
        }

        public async Task<int> GetTotalAssetQuantity(int assetId)
        {
            return await _dbSet
                .Where(wa => wa.AssetId == assetId)
                .SumAsync(wa => wa.Quantity ?? 0);
        }

        public async Task<IEnumerable<WarehouseAsset>> GetLowStockWarehouseAssets(int minQuantity)
        {
            return await _dbSet
                .Where(wa => wa.Quantity <= minQuantity)
                .Include(wa => wa.Asset)
                    .ThenInclude(a => a.AssetCategory)
                .Include(wa => wa.Warehouse)
                .ToListAsync();
        }
    }
}