using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IAssetCategoryService : IBaseService<AssetCategory>
    {
        Task<IEnumerable<AssetCategory>> GetActiveCategoriesAsync();
        Task<IEnumerable<AssetCategory>> GetCategoriesWithAssetsAsync();
        Task<AssetCategory> GetCategoryWithAssetsAsync(int categoryId);
        Task<IEnumerable<AssetCategory>> SearchCategoriesAsync(string keyword);
        Task<Dictionary<string, int>> GetCategoryStatisticsAsync();
        Task SoftDeleteCategoryAsync(int categoryId);
    }
}




