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
}
