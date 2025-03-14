using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class AssetCategoryService : BaseService<AssetCategory>, IAssetCategoryService
    {
        public AssetCategoryService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<AssetCategory>> GetActiveCategoriesAsync()
        {
            return await _unitOfWork.AssetCategories.GetActiveCategories();
        }

        public async Task<IEnumerable<AssetCategory>> GetCategoriesWithAssetsAsync()
        {
            return await _unitOfWork.AssetCategories.GetCategoriesWithAssets();
        }

        public async Task<AssetCategory> GetCategoryWithAssetsAsync(int categoryId)
        {
            return await _unitOfWork.AssetCategories.GetCategoryWithAssets(categoryId);
        }

        public async Task<IEnumerable<AssetCategory>> SearchCategoriesAsync(string keyword)
        {
            return await _unitOfWork.AssetCategories.SearchCategories(keyword);
        }

        public async Task<Dictionary<string, int>> GetCategoryStatisticsAsync()
        {
            return await _unitOfWork.AssetCategories.GetCategoryStatistics();
        }

        public async Task SoftDeleteCategoryAsync(int categoryId)
        {
            await _unitOfWork.AssetCategories.SoftDeleteCategoryAsync(categoryId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}




