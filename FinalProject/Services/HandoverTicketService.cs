using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class HandoverTicketService : BaseService<HandoverTicket>, IHandoverTicketService
    {
        public HandoverTicketService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverByAsync(int handoverById)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketsByHandoverBy(handoverById);
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverToAsync(int handoverToId)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketsByHandoverTo(handoverToId);
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByOwnerAsync(int ownerId)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketsByOwner(ownerId);
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByDepartmentAsync(int departmentId)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketsByDepartment(departmentId);
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByWarehouseAssetAsync(int warehouseAssetId)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketsByWarehouseAsset(warehouseAssetId);
        }

        public async Task<IEnumerable<HandoverTicket>> GetRecentHandoverTicketsAsync(int count)
        {
            return await _unitOfWork.HandoverTickets.GetRecentHandoverTickets(count);
        }

        public async Task<Dictionary<string, int>> GetHandoverTicketStatisticsByMonthAsync(int year)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketStatisticsByMonth(year);
        }

        public async Task<HandoverTicket> GetHandoverTicketWithDetailsAsync(int handoverTicketId)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketWithDetails(handoverTicketId);
        }
    }
}









