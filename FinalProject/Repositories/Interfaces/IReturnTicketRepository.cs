using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IReturnTicketRepository : IRepository<ReturnTicket>
{
    Task<IEnumerable<ReturnTicket>> GetReturnTicketsByUser(int userId);
    Task<IEnumerable<ReturnTicket>> GetReturnTicketsByOwner(int ownerId);
    Task<IEnumerable<ReturnTicket>> GetReturnTicketsByBorrowTicket(int borrowTicketId);
    Task<IEnumerable<ReturnTicket>> GetRecentReturnTickets(int count);
    Task<Dictionary<string, int>> GetReturnTicketStatisticsByMonth(int year);
    Task<ReturnTicket> GetReturnTicketWithDetails(int returnTicketId);
}