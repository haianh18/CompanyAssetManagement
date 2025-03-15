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
            return await _dbSet.Where(a => a.IsDeleted == false)
                .Include(a => a.AssetCategory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByStatus(AssetStatus status)
        {
            return await _dbSet.Where(a => a.AssetStatus == status)
                .Include(a => a.AssetCategory)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByWarehouse(int warehouseId)
        {
            return await _dbSet.Where(a => a.WarehouseAssets.Any(wa => wa.WarehouseId == warehouseId))
                .Include(a => a.AssetCategory)
                .Include(a => a.WarehouseAssets)
                    .ThenInclude(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> SearchAssets(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return await GetAllIncludingDeletedAsync();

            keyword = keyword.ToLower();
            return await _dbSet.Where(a =>
                    a.Name.ToLower().Contains(keyword) ||
                    (a.Description != null && a.Description.ToLower().Contains(keyword)) ||
                    (a.Note != null && a.Note.ToLower().Contains(keyword)) ||
                    (a.AssetCategory != null && a.AssetCategory.Name.ToLower().Contains(keyword)))
                .Include(a => a.AssetCategory)
                .ToListAsync();
        }

        public async Task<int> CountAssetsByStatus(AssetStatus status)
        {
            return await _dbSet.CountAsync(a => a.AssetStatus == status);
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
    }
}