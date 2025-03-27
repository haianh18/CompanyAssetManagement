using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IHandoverTicketService : IBaseService<HandoverTicket>
    {
        Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverByAsync(int handoverById);
        Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverToAsync(int handoverToId);
        Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByOwnerAsync(int ownerId);
        Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByDepartmentAsync(int departmentId);
        Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByWarehouseAssetAsync(int warehouseAssetId);
        Task<IEnumerable<HandoverTicket>> GetRecentHandoverTicketsAsync(int count);
        Task<Dictionary<string, int>> GetHandoverTicketStatisticsByMonthAsync(int year);
        Task<HandoverTicket> GetHandoverTicketWithDetailsAsync(int handoverTicketId);
        Task<IEnumerable<HandoverTicket>> GetActiveHandoversByEmployeeAsync(int employeeId);
        Task<HandoverTicket> GetHandoverTicketWithAssetDetailsAsync(int handoverTicketId);
        Task<IEnumerable<HandoverTicket>> GetHandoversByDepartmentEmployeeAsync(int departmentId, int employeeId);
        Task ProcessEmployeeTerminationAsync(int employeeId);
        Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByAssetIdAsync(int assetId);

    }
}









