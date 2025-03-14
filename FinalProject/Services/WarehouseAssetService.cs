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

        public async Task<bool> UpdateQuantityAsync(int warehouseAssetId, int quantityChange)
        {
            var result = await _unitOfWork.WarehouseAssets.UpdateQuantity(warehouseAssetId, quantityChange);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }

        public async Task<int> GetTotalAssetQuantityAsync(int assetId)
        {
            return await _unitOfWork.WarehouseAssets.GetTotalAssetQuantity(assetId);
        }

        public async Task<IEnumerable<WarehouseAsset>> GetLowStockWarehouseAssetsAsync(int minQuantity)
        {
            return await _unitOfWork.WarehouseAssets.GetLowStockWarehouseAssets(minQuantity);
        }
    }
}













