using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IBorrowTicketService : IBaseService<BorrowTicket>
    {
        Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByUserAsync(int userId);
        Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByOwnerAsync(int ownerId);
        Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByAssetAsync(int assetId);
        Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByWarehouseAssetAsync(int warehouseAssetId);
        Task<IEnumerable<BorrowTicket>> GetOverdueBorrowTicketsAsync();
        Task<IEnumerable<BorrowTicket>> GetBorrowTicketsWithoutReturnAsync();
        Task<IEnumerable<BorrowTicket>> GetRecentBorrowTicketsAsync(int count);
        Task<Dictionary<string, int>> GetBorrowTicketStatisticsByMonthAsync(int year);
    }
}





