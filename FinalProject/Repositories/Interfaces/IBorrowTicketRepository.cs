using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IBorrowTicketRepository : IRepository<BorrowTicket>
{
    Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByUser(int userId);
    Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByOwner(int ownerId);
    Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByAsset(int assetId);
    Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByWarehouseAsset(int warehouseAssetId);
    Task<IEnumerable<BorrowTicket>> GetOverdueBorrowTickets();
    Task<IEnumerable<BorrowTicket>> GetBorrowTicketsWithoutReturn();
    Task<IEnumerable<BorrowTicket>> GetRecentBorrowTickets(int count);
    Task<Dictionary<string, int>> GetBorrowTicketStatisticsByMonth(int year);

    Task<bool> IsUserEligibleForBorrowing(int userId);
    Task<IEnumerable<BorrowTicket>> GetActiveBorrowTicketsByUser(int userId);
    Task<IEnumerable<BorrowTicket>> GetBorrowTicketsExpiringInDays(int days);
    Task<BorrowTicket> GetBorrowTicketWithExtensions(int borrowTicketId);
    Task<bool> HasUserOverdueTickets(int userId);
    Task<bool> HasUserExtendedBorrowTicket(int borrowTicketId);
}