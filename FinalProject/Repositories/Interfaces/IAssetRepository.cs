using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IAssetRepository : IRepository<Asset>
{
    Task<IEnumerable<Asset>> GetAssetsByCategory(int categoryId);
    Task<IEnumerable<Asset>> GetActiveAssets();
    Task<IEnumerable<Asset>> GetAssetsByStatus(AssetStatus status);
    Task<IEnumerable<Asset>> GetAssetsByWarehouse(int warehouseId);
    Task<IEnumerable<Asset>> SearchAssets(string keyword, bool includeDeleted = false);
    Task<int> CountAssetsByStatus(AssetStatus status);
    Task<double> GetTotalAssetsValue();
    Task<IEnumerable<Asset>> GetAssetsPaginated(int pageIndex, int pageSize);
}