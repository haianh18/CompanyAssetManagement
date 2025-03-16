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
            return await _dbSet.Where(c => c.IsDeleted == false)
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

        public async Task SoftDeleteCategoryAsync(int categoryId)
        {
            // Tìm hoặc tạo category mặc định
            var defaultCategory = await _dbSet.IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Name == "Chưa phân loại");

            if (defaultCategory == null)
            {
                defaultCategory = new AssetCategory
                {
                    Name = "Chưa phân loại",
                    DateCreated = DateTime.Now
                };
                _dbSet.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            // Kiểm tra nếu yêu cầu xóa chính danh mục mặc định
            var categoryToDelete = await _dbSet.FindAsync(categoryId);
            if (categoryToDelete != null && categoryToDelete.Name == "Chưa phân loại")
            {
                throw new Exception("Không thể xóa danh mục 'Chưa phân loại'");
            }

            // Soft delete category gốc
            var originalCategory = await _dbSet.FindAsync(categoryId);
            if (originalCategory == null)
                throw new Exception("Category not found");

            originalCategory.IsDeleted = true;
            originalCategory.DeletedDate = DateTime.Now;

            // Chuyển assets sang category mặc định
            var assetsInCategory = await _context.Assets.IgnoreQueryFilters()
                .Where(a => a.AssetCategoryId == categoryId)
                .ToListAsync();

            foreach (var asset in assetsInCategory)
            {
                asset.AssetCategoryId = defaultCategory.Id;
                asset.DateModified = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public override async Task<IEnumerable<AssetCategory>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(c => c.Assets)
                .ToListAsync();
        }

        public override async Task<AssetCategory> GetByIdIncludingDeletedAsync(int id)
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(c => c.Assets)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<IEnumerable<AssetCategory>> GetAllDeletedAsync()
        {
            return await _dbSet
                .Include(c => c.Assets)
                .IgnoreQueryFilters()
                .ToListAsync();
        }

        public override async Task<IEnumerable<AssetCategory>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Assets)
                .ToListAsync();
        }
    }
}