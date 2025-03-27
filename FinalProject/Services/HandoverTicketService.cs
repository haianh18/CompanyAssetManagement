using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class HandoverTicketService : BaseService<HandoverTicket>, IHandoverTicketService
    {
        private readonly IHandoverReturnService _handoverReturnService;

        public HandoverTicketService(
            IUnitOfWork unitOfWork,
            IHandoverReturnService handoverReturnService) : base(unitOfWork)
        {
            _handoverReturnService = handoverReturnService;
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
        public async Task<IEnumerable<HandoverTicket>> GetActiveHandoversByEmployeeAsync(int employeeId)
        {
            return await _unitOfWork.HandoverTickets.GetActiveHandoversByEmployee(employeeId);
        }

        public async Task<HandoverTicket> GetHandoverTicketWithAssetDetailsAsync(int handoverTicketId)
        {
            return await _unitOfWork.HandoverTickets.GetHandoverTicketWithAssetDetails(handoverTicketId);
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoversByDepartmentEmployeeAsync(int departmentId, int employeeId)
        {
            return await _unitOfWork.HandoverTickets.GetHandoversByDepartmentEmployee(departmentId, employeeId);
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByAssetIdAsync(int assetId)
        {
            var handoverTickets = await _unitOfWork.HandoverTickets.GetAllAsync();
            return handoverTickets.Where(ht => ht.WarehouseAsset?.AssetId == assetId);
        }

        public async Task ProcessEmployeeTerminationAsync(int employeeId)
        {
            // Get all active handover tickets for this employee
            var activeHandovers = await GetActiveHandoversByEmployeeAsync(employeeId);

            foreach (var handover in activeHandovers)
            {
                // Create handover return record for each asset
                await _handoverReturnService.CreateHandoverReturnAsync(
                    handover.Id,
                    employeeId,
                    "Employee termination - automatic return request");
            }
        }
    }
}









