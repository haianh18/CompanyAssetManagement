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
    public class AssetRepository : Repository<Asset>, IAssetRepository
    {
        public AssetRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Asset>> GetAssetsByCategory(int categoryId)
        {
            return await _dbSet.Where(a => a.AssetCategoryId == categoryId)
                .Include(a => a.AssetCategory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetActiveAssets()
        {
            return await _dbSet.Where(a => !a.IsDeleted)
                .Include(a => a.AssetCategory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByStatus(AssetStatus status)
        {
            // Now filtering based on WarehouseAsset quantities
            IQueryable<Asset> query = _dbSet
                .Include(a => a.AssetCategory)
                .Include(a => a.WarehouseAssets);

            switch (status)
            {
                case AssetStatus.GOOD:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.GoodQuantity > 0));
                    break;
                case AssetStatus.BROKEN:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.BrokenQuantity > 0));
                    break;
                case AssetStatus.FIXING:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.FixingQuantity > 0));
                    break;
                case AssetStatus.DISPOSED:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.DisposedQuantity > 0));
                    break;
                default:
                    return Enumerable.Empty<Asset>();
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByWarehouse(int warehouseId)
        {
            return await _dbSet.Where(a => a.WarehouseAssets.Any(wa => wa.WarehouseId == warehouseId))
                .Include(a => a.AssetCategory)
                .Include(a => a.WarehouseAssets)
                    .ThenInclude(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> SearchAssets(string keyword, bool includeDeleted = false)
        {
            if (string.IsNullOrEmpty(keyword))
                return includeDeleted ? await GetAllIncludingDeletedAsync() : await GetAllAsync();

            keyword = keyword.ToLower();
            var query = includeDeleted ? _dbSet.IgnoreQueryFilters() : _dbSet;

            return await query.Where(a =>
                    a.Name.ToLower().Contains(keyword) ||
                    (a.Description != null && a.Description.ToLower().Contains(keyword)) ||
                    (a.Note != null && a.Note.ToLower().Contains(keyword)) ||
                    (a.AssetCategory != null && a.AssetCategory.Name.ToLower().Contains(keyword)))
                .Include(a => a.AssetCategory)
                .ToListAsync();
        }

        public async Task<int> CountAssetsByStatus(AssetStatus status)
        {
            // Count based on WarehouseAsset quantities
            IQueryable<Asset> query = _dbSet.Include(a => a.WarehouseAssets);

            switch (status)
            {
                case AssetStatus.GOOD:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.GoodQuantity > 0));
                    break;
                case AssetStatus.BROKEN:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.BrokenQuantity > 0));
                    break;
                case AssetStatus.FIXING:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.FixingQuantity > 0));
                    break;
                case AssetStatus.DISPOSED:
                    query = query.Where(a => a.WarehouseAssets.Any(wa => wa.DisposedQuantity > 0));
                    break;
                default:
                    return 0;
            }

            return await query.CountAsync();
        }

        public async Task<double> GetTotalAssetsValue()
        {
            return await _dbSet.SumAsync(a => a.Price);
        }

        public async Task<IEnumerable<Asset>> GetAssetsPaginated(int pageIndex, int pageSize)
        {
            return await _dbSet
                .OrderByDescending(a => a.DateCreated)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(a => a.AssetCategory)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Asset>> GetAllAsync()
        {
            return await _dbSet.Include(a => a.AssetCategory).ToListAsync();
        }

        public override async Task<IEnumerable<Asset>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().Include(a => a.AssetCategory).ToListAsync();
        }

        public override async Task<Asset> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(a => a.AssetCategory)
                .Include(a => a.WarehouseAssets)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public override async Task<Asset> GetByIdIncludingDeletedAsync(int id)
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(a => a.AssetCategory)
                .Include(a => a.WarehouseAssets)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}