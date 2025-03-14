using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class ReturnTicketService : BaseService<ReturnTicket>, IReturnTicketService
    {
        public ReturnTicketService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsByUserAsync(int userId)
        {
            return await _unitOfWork.ReturnTickets.GetReturnTicketsByUser(userId);
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsByOwnerAsync(int ownerId)
        {
            return await _unitOfWork.ReturnTickets.GetReturnTicketsByOwner(ownerId);
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsByBorrowTicketAsync(int borrowTicketId)
        {
            return await _unitOfWork.ReturnTickets.GetReturnTicketsByBorrowTicket(borrowTicketId);
        }

        public async Task<IEnumerable<ReturnTicket>> GetRecentReturnTicketsAsync(int count)
        {
            return await _unitOfWork.ReturnTickets.GetRecentReturnTickets(count);
        }

        public async Task<Dictionary<string, int>> GetReturnTicketStatisticsByMonthAsync(int year)
        {
            return await _unitOfWork.ReturnTickets.GetReturnTicketStatisticsByMonth(year);
        }

        public async Task<ReturnTicket> GetReturnTicketWithDetailsAsync(int returnTicketId)
        {
            return await _unitOfWork.ReturnTickets.GetReturnTicketWithDetails(returnTicketId);
        }
    }
}










