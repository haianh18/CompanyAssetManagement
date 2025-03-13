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

    public class AssetCategoryRepository : Repository<AssetCategory>, IAssetCategoryRepository
    {
        public AssetCategoryRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AssetCategory>> GetActiveCategories()
        {
            return await _dbSet.Where(c => c.ActiveStatus == (int)ActiveStatus.ACTIVE)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetCategory>> GetCategoriesWithAssets()
        {
            return await _dbSet
                .Include(c => c.Assets)
                .ToListAsync();
        }

        public async Task<AssetCategory> GetCategoryWithAssets(int categoryId)
        {
            return await _dbSet
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<IEnumerable<AssetCategory>> SearchCategories(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return await GetAllAsync();

            keyword = keyword.ToLower();
            return await _dbSet.Where(c => c.Name.ToLower().Contains(keyword))
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetCategoryStatistics()
        {
            var categories = await _dbSet
                .Include(c => c.Assets)
                .ToListAsync();

            return categories.ToDictionary(
                c => c.Name,
                c => c.Assets.Count
            );
        }
    }
}