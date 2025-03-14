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


    public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Warehouse>> GetActiveWarehouses()
        {
            return await GetAllAsync();
        }

        public async Task<Warehouse> GetWarehouseWithAssets(int warehouseId)
        {
            return await _dbSet
                .Include(w => w.WarehouseAssets)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .FirstOrDefaultAsync(w => w.Id == warehouseId);
        }

        public async Task<IEnumerable<Warehouse>> GetWarehousesWithAssets()
        {
            return await _dbSet
                .Include(w => w.WarehouseAssets)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetWarehouseStatistics()
        {
            var warehouses = await _dbSet
                .Include(w => w.WarehouseAssets)
                .ToListAsync();

            return warehouses.ToDictionary(
                w => w.Name,
                w => w.WarehouseAssets.Sum(wa => wa.Quantity ?? 0)
            );
        }

        public async Task SoftDeleteWarehouseAsync(int warehouseId)
        {
            // Tìm hoặc tạo warehouse mặc định
            var defaultWarehouse = await _dbSet
                .FirstOrDefaultAsync(w => w.Name == "Unassigned Storage");

            if (defaultWarehouse == null)
            {
                defaultWarehouse = new Warehouse
                {
                    Name = "Unassigned Storage",
                    DateCreated = DateTime.Now
                };
                _context.Warehouses.Add(defaultWarehouse);
            }

            // Soft delete warehouse gốc
            var originalWarehouse = await _dbSet.FindAsync(warehouseId);
            if (originalWarehouse == null)
                throw new Exception("Warehouse not found");

            originalWarehouse.IsDeleted = true;
            originalWarehouse.DeletedDate = DateTime.Now;

            // Chuyển warehouse assets sang warehouse mặc định
            var warehouseAssetsInWarehouse = await _context.WarehouseAssets
                .Where(wa => wa.WarehouseId == warehouseId)
                .ToListAsync();

            foreach (var warehouseAsset in warehouseAssetsInWarehouse)
            {
                warehouseAsset.WarehouseId = defaultWarehouse.Id;
                warehouseAsset.DateModified = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}