using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IWarehouseAssetService : IBaseService<WarehouseAsset>
    {
        Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByWarehouseAsync(int warehouseId);
        Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByAssetAsync(int assetId);
        Task<WarehouseAsset> GetWarehouseAssetByWarehouseAndAssetAsync(int warehouseId, int assetId);
        Task<bool> UpdateQuantityAsync(int warehouseAssetId, int quantityChange);
        Task<int> GetTotalAssetQuantityAsync(int assetId);
        Task<IEnumerable<WarehouseAsset>> GetLowStockWarehouseAssetsAsync(int minQuantity);
    }
}













