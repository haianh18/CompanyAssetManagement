using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class DisposalTicketService : BaseService<DisposalTicket>, IDisposalTicketService
    {
        public DisposalTicketService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByDisposalByAsync(int disposalById)
        {
            return await _unitOfWork.DisposalTickets.GetDisposalTicketsByDisposalBy(disposalById);
        }

        public async Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByOwnerAsync(int ownerId)
        {
            return await _unitOfWork.DisposalTickets.GetDisposalTicketsByOwner(ownerId);
        }

        public async Task<DisposalTicket> GetDisposalTicketWithAssetsAsync(int disposalTicketId)
        {
            return await _unitOfWork.DisposalTickets.GetDisposalTicketWithAssets(disposalTicketId);
        }

        public async Task<IEnumerable<DisposalTicket>> GetDisposalTicketsWithAssetsAsync()
        {
            return await _unitOfWork.DisposalTickets.GetDisposalTicketsWithAssets();
        }

        public async Task<IEnumerable<DisposalTicket>> GetRecentDisposalTicketsAsync(int count)
        {
            return await _unitOfWork.DisposalTickets.GetRecentDisposalTickets(count);
        }

        public async Task<Dictionary<string, int>> GetDisposalTicketStatisticsByMonthAsync(int year)
        {
            return await _unitOfWork.DisposalTickets.GetDisposalTicketStatisticsByMonth(year);
        }

        public async Task<double> GetTotalDisposalValueAsync(int year)
        {
            return await _unitOfWork.DisposalTickets.GetTotalDisposalValue(year);
        }
    }
}








