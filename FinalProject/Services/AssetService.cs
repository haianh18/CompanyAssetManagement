using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class AssetService : BaseService<Asset>, IAssetService
    {
        public AssetService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(int categoryId)
        {
            return await _unitOfWork.Assets.GetAssetsByCategory(categoryId);
        }

        public async Task<IEnumerable<Asset>> GetActiveAssetsAsync()
        {
            return await _unitOfWork.Assets.GetActiveAssets();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status)
        {
            return await _unitOfWork.Assets.GetAssetsByStatus(status);
        }

        public async Task<IEnumerable<Asset>> GetAssetsByWarehouseAsync(int warehouseId)
        {
            return await _unitOfWork.Assets.GetAssetsByWarehouse(warehouseId);
        }

        public async Task<IEnumerable<Asset>> SearchAssetsAsync(string keyword)
        {
            return await _unitOfWork.Assets.SearchAssets(keyword);
        }

        public async Task<int> CountAssetsByStatusAsync(AssetStatus status)
        {
            return await _unitOfWork.Assets.CountAssetsByStatus(status);
        }

        public async Task<double> GetTotalAssetsValueAsync()
        {
            return await _unitOfWork.Assets.GetTotalAssetsValue();
        }

        public async Task<IEnumerable<Asset>> GetAssetsPaginatedAsync(int pageIndex, int pageSize)
        {
            return await _unitOfWork.Assets.GetAssetsPaginated(pageIndex, pageSize);
        }

        public async Task<IEnumerable<Asset>> SearchAssetAsync(string keyword)
        {
            return await _unitOfWork.Assets.SearchAssets(keyword);
        }
    }
}







