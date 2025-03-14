using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IAssetCategoryRepository : IRepository<AssetCategory>
{
    Task<IEnumerable<AssetCategory>> GetActiveCategories();
    Task<IEnumerable<AssetCategory>> GetCategoriesWithAssets();
    Task<AssetCategory> GetCategoryWithAssets(int categoryId);
    Task<IEnumerable<AssetCategory>> SearchCategories(string keyword);
    Task<Dictionary<string, int>> GetCategoryStatistics();

    Task SoftDeleteCategoryAsync(int categoryId);
}
