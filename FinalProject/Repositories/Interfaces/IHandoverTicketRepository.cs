using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IHandoverTicketRepository : IRepository<HandoverTicket>
{
    Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverBy(int handoverById);
    Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverTo(int handoverToId);
    Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByOwner(int ownerId);
    Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByDepartment(int departmentId);
    Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByWarehouseAsset(int warehouseAssetId);
    Task<IEnumerable<HandoverTicket>> GetRecentHandoverTickets(int count);
    Task<Dictionary<string, int>> GetHandoverTicketStatisticsByMonth(int year);
    Task<HandoverTicket> GetHandoverTicketWithDetails(int handoverTicketId);
    Task<IEnumerable<HandoverTicket>> GetActiveHandoversByEmployee(int employeeId);
    Task<HandoverTicket> GetHandoverTicketWithAssetDetails(int handoverTicketId);
    Task<IEnumerable<HandoverTicket>> GetHandoversByDepartmentEmployee(int departmentId, int employeeId);

    Task UpdateHandoverTicketStatus(int handoverTicketId, bool isActive, DateTime? actualEndDate);
    Task<bool> ValidateHandoverOperationAsync(int warehouseAssetId, int quantity, bool isReturn);
    Task ProcessHandoverEventAsync(HandoverTicket handoverTicket, string eventType, string note);
    Task<bool> IsAssetAvailableForHandoverAsync(int warehouseAssetId, int quantity);
    Task<IEnumerable<HandoverReturn>> GetHandoverReturnHistoryAsync(int handoverTicketId);
    Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByAssetIdAsync(int assetId);
}
