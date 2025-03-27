using FinalProject.Enums;
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
        Task<bool> UpdateAssetStatusQuantityAsync(int warehouseAssetId, AssetStatus fromStatus,
            AssetStatus toStatus, int quantity);
        Task<bool> UpdateBorrowedQuantityAsync(int warehouseAssetId, int quantityChange);
        Task<bool> UpdateHandedOverQuantityAsync(int warehouseAssetId, int quantityChange);
        Task<IEnumerable<WarehouseAsset>> GetAssetsWithAvailableQuantityAsync();
        Task<IEnumerable<WarehouseAsset>> GetBorrowableAssetsAsync();
    }
}













