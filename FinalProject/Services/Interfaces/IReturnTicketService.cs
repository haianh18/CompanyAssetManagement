using FinalProject.Enums;
using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IReturnTicketService : IBaseService<ReturnTicket>
    {
        Task<IEnumerable<ReturnTicket>> GetReturnTicketsByUserAsync(int userId);
        Task<IEnumerable<ReturnTicket>> GetReturnTicketsByOwnerAsync(int ownerId);
        Task<IEnumerable<ReturnTicket>> GetReturnTicketsByBorrowTicketAsync(int borrowTicketId);
        Task<IEnumerable<ReturnTicket>> GetRecentReturnTicketsAsync(int count);
        Task<Dictionary<string, int>> GetReturnTicketStatisticsByMonthAsync(int year);
        Task<ReturnTicket> GetReturnTicketWithDetailsAsync(int returnTicketId);
        Task<ReturnTicket> CreateReturnRequestAsync(int borrowTicketId, int ownerId, string note);
        Task<ReturnTicket> ProcessEarlyReturnAsync(int borrowTicketId, int returnById, string notes);
        Task<ReturnTicket> ApproveReturnAsync(int returnTicketId, AssetStatus assetCondition, string notes);
        Task<ReturnTicket> RejectReturnAsync(int returnTicketId, string rejectionReason);
        Task<IEnumerable<ReturnTicket>> GetPendingReturnRequestsAsync();
        Task<IEnumerable<ReturnTicket>> GetReturnTicketsWithConditionAsync(AssetStatus condition);
    }
}










