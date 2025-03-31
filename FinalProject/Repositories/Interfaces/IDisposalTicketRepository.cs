using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IDisposalTicketRepository : IRepository<DisposalTicket>
{
    Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByDisposalBy(int disposalById);
    Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByOwner(int ownerId);
    Task<DisposalTicket> GetDisposalTicketWithAssets(int disposalTicketId);
    Task<IEnumerable<DisposalTicket>> GetDisposalTicketsWithAssets();
    Task<IEnumerable<DisposalTicket>> GetRecentDisposalTickets(int count);
    Task<Dictionary<string, int>> GetDisposalTicketStatisticsByMonth(int year);
    Task<double> GetTotalDisposalValue(int year);
    Task<DisposalTicket> GetDisposalTicketWithDetails(int id);
    Task<IEnumerable<DisposalTicket>> GetAllWithDetailsAsync();
}