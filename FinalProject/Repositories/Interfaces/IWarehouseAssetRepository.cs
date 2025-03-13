using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IWarehouseAssetRepository : IRepository<WarehouseAsset>
{
    Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByWarehouse(int warehouseId);
    Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByAsset(int assetId);
    Task<WarehouseAsset> GetWarehouseAssetByWarehouseAndAsset(int warehouseId, int assetId);
    Task<bool> UpdateQuantity(int warehouseAssetId, int quantityChange);
    Task<int> GetTotalAssetQuantity(int assetId);
    Task<IEnumerable<WarehouseAsset>> GetLowStockWarehouseAssets(int minQuantity);
}