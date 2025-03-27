using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;

namespace FinalProject.Services
{
    public class HandoverReturnService : BaseService<HandoverReturn>, IHandoverReturnService
    {
        private readonly IHandoverService _handoverService;

        public HandoverReturnService(
            IUnitOfWork unitOfWork,
            IHandoverService handoverService) : base(unitOfWork)
        {
            _handoverService = handoverService;
        }

        public async Task<IEnumerable<HandoverReturn>> GetHandoverReturnsByEmployeeAsync(int employeeId)
        {
            return await _unitOfWork.HandoverReturns.GetHandoverReturnsByEmployee(employeeId);
        }

        public async Task<HandoverReturn> GetHandoverReturnWithDetailsAsync(int handoverReturnId)
        {
            return await _unitOfWork.HandoverReturns.GetHandoverReturnWithDetails(handoverReturnId);
        }

        public async Task<IEnumerable<HandoverReturn>> GetPendingHandoverReturnsAsync()
        {
            return await _unitOfWork.HandoverReturns.GetPendingHandoverReturns();
        }

        public async Task<HandoverReturn> CreateHandoverReturnAsync(int handoverTicketId, int returnById, string notes)
        {
            // Get the handover ticket
            var handoverTicket = await _unitOfWork.HandoverTickets.GetByIdAsync(handoverTicketId);
            if (handoverTicket == null)
                throw new Exception("Handover ticket not found");

            if (!handoverTicket.IsActive)
                throw new Exception("This handover ticket is no longer active");

            // Create handover return record
            var handoverReturn = new HandoverReturn
            {
                HandoverTicketId = handoverTicketId,
                ReturnById = returnById,
                ReceivedById = handoverTicket.HandoverById, // Default to original handover person
                ReturnDate = DateTime.Now,
                Note = notes,
                DateCreated = DateTime.Now
            };

            await AddAsync(handoverReturn);

            return handoverReturn;
        }

        public async Task<HandoverReturn> ApproveHandoverReturnAsync(int handoverReturnId, AssetStatus assetCondition, string notes)
        {
            var handoverReturn = await GetByIdAsync(handoverReturnId);
            if (handoverReturn == null)
                throw new Exception("Handover return record not found");

            var handoverTicket = await _unitOfWork.HandoverTickets.GetByIdAsync(handoverReturn.HandoverTicketId);
            if (handoverTicket == null)
                throw new Exception("Related handover ticket not found");

            // Update handover return if notes provided
            if (!string.IsNullOrEmpty(notes))
            {
                handoverReturn.Note = string.IsNullOrEmpty(handoverReturn.Note)
                    ? notes
                    : $"{handoverReturn.Note}\n{notes}";
            }

            // Set asset condition on return
            handoverReturn.AssetConditionOnReturn = assetCondition;
            handoverReturn.DateModified = DateTime.Now;

            // Update handover ticket status
            await _handoverService.UpdateHandoverTicketStatus(
                handoverTicket.Id,
                false,  // isActive = false
                DateTime.Now // actualEndDate
            );

            // Update warehouse asset quantities
            if (handoverTicket.WarehouseAssetId.HasValue)
            {
                await _handoverService.UpdateWarehouseAssetQuantitiesForHandover(
                    handoverTicket.WarehouseAssetId.Value,
                    handoverTicket.Quantity ?? 0,
                    true, // isReturn
                    assetCondition
                );
            }

            await UpdateAsync(handoverReturn);

            return handoverReturn;
        }
    }
}
