using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class BorrowTicketService : BaseService<BorrowTicket>, IBorrowTicketService
    {
        public BorrowTicketService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByUserAsync(int userId)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketsByUser(userId);
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByOwnerAsync(int ownerId)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketsByOwner(ownerId);
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByAssetAsync(int assetId)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketsByAsset(assetId);
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByWarehouseAssetAsync(int warehouseAssetId)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketsByWarehouseAsset(warehouseAssetId);
        }

        public async Task<IEnumerable<BorrowTicket>> GetOverdueBorrowTicketsAsync()
        {
            return await _unitOfWork.BorrowTickets.GetOverdueBorrowTickets();
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsWithoutReturnAsync()
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketsWithoutReturn();
        }

        public async Task<IEnumerable<BorrowTicket>> GetRecentBorrowTicketsAsync(int count)
        {
            return await _unitOfWork.BorrowTickets.GetRecentBorrowTickets(count);
        }

        public async Task<Dictionary<string, int>> GetBorrowTicketStatisticsByMonthAsync(int year)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketStatisticsByMonth(year);
        }
    }
}





