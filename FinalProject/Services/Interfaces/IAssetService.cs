using FinalProject.Enums;
using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IAssetService : IBaseService<Asset>
    {
        Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(int categoryId);
        Task<IEnumerable<Asset>> SearchAssetAsync(string keyword, bool includeDeleted = false);
        Task<IEnumerable<Asset>> GetActiveAssetsAsync();
        Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
        Task<IEnumerable<Asset>> GetAssetsByWarehouseAsync(int warehouseId);
        Task<int> CountAssetsByStatusAsync(AssetStatus status);
        Task<double> GetTotalAssetsValueAsync();
        Task<IEnumerable<Asset>> GetAssetsPaginatedAsync(int pageIndex, int pageSize);
    }

}


