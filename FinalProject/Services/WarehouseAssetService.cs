using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class WarehouseAssetService : BaseService<WarehouseAsset>, IWarehouseAssetService
    {
        public WarehouseAssetService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByWarehouseAsync(int warehouseId)
        {
            return await _unitOfWork.WarehouseAssets.GetWarehouseAssetsByWarehouse(warehouseId);
        }

        public async Task<IEnumerable<WarehouseAsset>> GetWarehouseAssetsByAssetAsync(int assetId)
        {
            return await _unitOfWork.WarehouseAssets.GetWarehouseAssetsByAsset(assetId);
        }

        public async Task<WarehouseAsset> GetWarehouseAssetByWarehouseAndAssetAsync(int warehouseId, int assetId)
        {
            return await _unitOfWork.WarehouseAssets.GetWarehouseAssetByWarehouseAndAsset(warehouseId, assetId);
        }

        public async Task<bool> UpdateAssetStatusQuantityAsync(int warehouseAssetId, AssetStatus fromStatus,
            AssetStatus toStatus, int quantity)
        {
            var result = await _unitOfWork.WarehouseAssets.UpdateAssetStatusQuantity(
                warehouseAssetId, fromStatus, toStatus, quantity);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task<bool> UpdateBorrowedQuantityAsync(int warehouseAssetId, int quantityChange)
        {
            var result = await _unitOfWork.WarehouseAssets.UpdateBorrowedQuantity(warehouseAssetId, quantityChange);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task<bool> UpdateHandedOverQuantityAsync(int warehouseAssetId, int quantityChange)
        {
            var result = await _unitOfWork.WarehouseAssets.UpdateHandedOverQuantity(warehouseAssetId, quantityChange);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task<IEnumerable<WarehouseAsset>> GetAssetsWithAvailableQuantityAsync()
        {
            return await _unitOfWork.WarehouseAssets.GetAssetsWithAvailableQuantity();
        }

        public async Task<IEnumerable<WarehouseAsset>> GetBorrowableAssetsAsync()
        {
            // Get assets that have available good condition items
            var assets = await _unitOfWork.WarehouseAssets.GetAssetsWithAvailableQuantity();
            return assets.Where(a => (a.GoodQuantity ?? 0) > (a.BorrowedGoodQuantity ?? 0) + (a.HandedOverGoodQuantity ?? 0));
        }
    }
}













