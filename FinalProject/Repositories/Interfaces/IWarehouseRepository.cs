using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<IEnumerable<Warehouse>> GetActiveWarehouses();
    Task<Warehouse> GetWarehouseWithAssets(int warehouseId);
    Task<IEnumerable<Warehouse>> GetWarehousesWithAssets();
    Task<Dictionary<string, int>> GetWarehouseStatistics();
}