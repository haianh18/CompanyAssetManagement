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
            return await _dbSet.Where(w => w.ActiveStatus == (int)ActiveStatus.ACTIVE)
                .ToListAsync();
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
    }
}