using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class WarehouseService : BaseService<Warehouse>, IWarehouseService
    {
        public WarehouseService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
        {
            return await _unitOfWork.Warehouses.GetActiveWarehouses();
        }

        public async Task<Warehouse> GetWarehouseWithAssetsAsync(int warehouseId)
        {
            return await _unitOfWork.Warehouses.GetWarehouseWithAssets(warehouseId);
        }

        public async Task<IEnumerable<Warehouse>> GetWarehousesWithAssetsAsync()
        {
            return await _unitOfWork.Warehouses.GetWarehousesWithAssets();
        }

        public async Task<Dictionary<string, int>> GetWarehouseStatisticsAsync()
        {
            return await _unitOfWork.Warehouses.GetWarehouseStatistics();
        }

        public async Task SoftDeleteWarehouseAsync(int warehouseId)
        {
            await _unitOfWork.Warehouses.SoftDeleteWarehouseAsync(warehouseId);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}













