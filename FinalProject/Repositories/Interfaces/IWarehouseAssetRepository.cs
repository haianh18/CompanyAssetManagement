using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IWarehouseAssetRepository : IRepository<WarehouseAsset>
{
    Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByWarehouse(int warehouseId);
    Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByAsset(int assetId);
    Task<WarehouseAsset> GetWarehouseAssetByWarehouseAndAsset(int warehouseId, int assetId);
    Task<bool> UpdateAssetStatusQuantity(int warehouseAssetId, AssetStatus fromStatus, AssetStatus toStatus, int quantity);
    Task<bool> UpdateBorrowedQuantity(int warehouseAssetId, int quantityChange);
    Task<bool> UpdateHandedOverQuantity(int warehouseAssetId, int quantityChange);
    Task<IEnumerable<WarehouseAsset>> GetAssetsWithAvailableQuantity();

    Task<IEnumerable<WarehouseAsset>> GetAssetsWithGoodQuantity();
    Task<IEnumerable<WarehouseAsset>> GetAssetsWithBrokenQuantity();
    Task<IEnumerable<WarehouseAsset>> GetAssetsWithFixingQuantity();
    Task<IEnumerable<WarehouseAsset>> GetAssetsWithDisposedQuantity();
    Task UpdateWarehouseAssetQuantitiesForHandover(int warehouseAssetId, int quantityChange, bool isReturn, AssetStatus status);
    Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByAssetIncludingDeleted(int assetId);
}