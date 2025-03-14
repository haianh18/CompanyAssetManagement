using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IDisposalTicketService : IBaseService<DisposalTicket>
    {
        Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByDisposalByAsync(int disposalById);
        Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByOwnerAsync(int ownerId);
        Task<DisposalTicket> GetDisposalTicketWithAssetsAsync(int disposalTicketId);
        Task<IEnumerable<DisposalTicket>> GetDisposalTicketsWithAssetsAsync();
        Task<IEnumerable<DisposalTicket>> GetRecentDisposalTicketsAsync(int count);
        Task<Dictionary<string, int>> GetDisposalTicketStatisticsByMonthAsync(int year);
        Task<double> GetTotalDisposalValueAsync(int year);
    }
}








