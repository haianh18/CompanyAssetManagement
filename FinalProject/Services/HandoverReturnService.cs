using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;

namespace FinalProject.Services
{
    public class HandoverReturnService : BaseService<HandoverReturn>, IHandoverReturnService
    {
        private readonly IHandoverTicketService _handoverTicketService;
        private readonly IWarehouseAssetService _warehouseAssetService;

        public HandoverReturnService(
            IUnitOfWork unitOfWork,
            IHandoverTicketService handoverTicketService,
            IWarehouseAssetService warehouseAssetService) : base(unitOfWork)
        {
            _handoverTicketService = handoverTicketService;
            _warehouseAssetService = warehouseAssetService;
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
            var handoverTicket = await _handoverTicketService.GetByIdAsync(handoverTicketId);
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

            var handoverTicket = await _handoverTicketService.GetByIdAsync(handoverReturn.HandoverTicketId);
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

            // Update handover ticket to reflect return
            handoverTicket.IsActive = false;
            handoverTicket.ActualEndDate = DateTime.Now;
            handoverTicket.CurrentCondition = assetCondition;
            handoverTicket.DateModified = DateTime.Now;

            // Update warehouse asset quantities
            var warehouseAsset = handoverTicket.WarehouseAsset;
            if (warehouseAsset != null)
            {
                // Reduce handed over quantity
                await _warehouseAssetService.UpdateHandedOverQuantityAsync(
                    warehouseAsset.Id,
                    -(handoverTicket.Quantity ?? 0));

                // Update asset quantities based on condition
                if (assetCondition == AssetStatus.GOOD)
                {
                    await _warehouseAssetService.UpdateAssetStatusQuantityAsync(
                        warehouseAsset.Id,
                        AssetStatus.GOOD,
                        AssetStatus.GOOD,
                        handoverTicket.Quantity ?? 0);
                }
                else
                {
                    await _warehouseAssetService.UpdateAssetStatusQuantityAsync(
                        warehouseAsset.Id,
                        AssetStatus.GOOD,
                        assetCondition,
                        handoverTicket.Quantity ?? 0);
                }
            }

            await UpdateAsync(handoverReturn);
            await _handoverTicketService.UpdateAsync(handoverTicket);

            return handoverReturn;
        }
    }
}
