using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IWarehouseService : IBaseService<Warehouse>
    {
        Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync();
        Task<Warehouse> GetWarehouseWithAssetsAsync(int warehouseId);
        Task<IEnumerable<Warehouse>> GetWarehousesWithAssetsAsync();
        Task<Dictionary<string, int>> GetWarehouseStatisticsAsync();
        Task SoftDeleteWarehouseAsync(int warehouseId);
    }
}













