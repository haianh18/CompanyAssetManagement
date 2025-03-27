using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class ReturnTicketService : BaseService<ReturnTicket>, IReturnTicketService
    {
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IWarehouseAssetService _warehouseAssetService;
        public ReturnTicketService(
            IUnitOfWork unitOfWork,
            IBorrowTicketService borrowTicketService,
            IWarehouseAssetService warehouseAssetService) : base(unitOfWork)
        {
            _borrowTicketService = borrowTicketService;
            _warehouseAssetService = warehouseAssetService;
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

        public async Task<ReturnTicket> CreateReturnRequestAsync(int borrowTicketId, int ownerId, string note)
        {
            var borrowTicket = await _borrowTicketService.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null)
                throw new Exception("Borrow ticket not found");

            if (borrowTicket.IsReturned)
                throw new Exception("This borrow ticket has already been returned");

            // Create return request (initiated by warehouse manager)
            var returnTicket = new ReturnTicket
            {
                BorrowTicketId = borrowTicketId,
                ReturnById = borrowTicket.BorrowById,
                OwnerId = ownerId,
                Quantity = borrowTicket.Quantity,
                Note = note,
                ApproveStatus = TicketStatus.Pending,
                ReturnRequestDate = DateTime.Now,
                DateCreated = DateTime.Now
            };

            await AddAsync(returnTicket);

            return returnTicket;
        }

        public async Task<ReturnTicket> ProcessEarlyReturnAsync(int borrowTicketId, int returnById, string notes)
        {
            var borrowTicket = await _borrowTicketService.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null)
                throw new Exception("Borrow ticket not found");

            if (borrowTicket.IsReturned)
                throw new Exception("This borrow ticket has already been returned");

            // Create return ticket for early return (initiated by borrower)
            var returnTicket = new ReturnTicket
            {
                BorrowTicketId = borrowTicketId,
                ReturnById = returnById,
                OwnerId = borrowTicket.OwnerId,
                Quantity = borrowTicket.Quantity,
                Note = notes,
                ApproveStatus = TicketStatus.Pending,
                ReturnRequestDate = DateTime.Now,
                IsEarlyReturn = DateTime.Now < borrowTicket.ReturnDate,
                DateCreated = DateTime.Now
            };

            await AddAsync(returnTicket);

            return returnTicket;
        }

        public async Task<ReturnTicket> ApproveReturnAsync(int returnTicketId, AssetStatus assetCondition, string notes)
        {
            var returnTicket = await _unitOfWork.ReturnTickets.GetReturnTicketWithBorrowDetails(returnTicketId);
            if (returnTicket == null)
                throw new Exception("Return ticket not found");

            var borrowTicket = returnTicket.BorrowTicket;
            if (borrowTicket == null)
                throw new Exception("Related borrow ticket not found");

            // Add notes if provided
            if (!string.IsNullOrEmpty(notes))
            {
                returnTicket.Note = string.IsNullOrEmpty(returnTicket.Note)
                    ? notes
                    : $"{returnTicket.Note}\n{notes}";
            }

            // Update return ticket
            returnTicket.ApproveStatus = TicketStatus.Approved;
            returnTicket.ActualReturnDate = DateTime.Now;
            returnTicket.AssetConditionOnReturn = assetCondition;
            returnTicket.DateModified = DateTime.Now;

            // Mark borrow ticket as returned
            await _borrowTicketService.MarkAsReturnedAsync(borrowTicket.Id);

            // Update warehouse asset quantities
            var warehouseAsset = borrowTicket.WarehouseAsset;
            if (warehouseAsset != null)
            {
                // Reduce borrowed quantity
                await _warehouseAssetService.UpdateBorrowedQuantityAsync(
                    warehouseAsset.Id,
                    -(borrowTicket.Quantity ?? 0));

                // Update asset quantities based on condition
                if (assetCondition == AssetStatus.GOOD)
                {
                    await _warehouseAssetService.UpdateAssetStatusQuantityAsync(
                        warehouseAsset.Id,
                        AssetStatus.GOOD,
                        AssetStatus.GOOD,
                        borrowTicket.Quantity ?? 0);
                }
                else
                {
                    await _warehouseAssetService.UpdateAssetStatusQuantityAsync(
                        warehouseAsset.Id,
                        AssetStatus.GOOD,
                        assetCondition,
                        borrowTicket.Quantity ?? 0);
                }
            }

            await UpdateAsync(returnTicket);

            return returnTicket;
        }

        public async Task<ReturnTicket> RejectReturnAsync(int returnTicketId, string rejectionReason)
        {
            var returnTicket = await GetByIdAsync(returnTicketId);
            if (returnTicket == null)
                throw new Exception("Return ticket not found");

            // Update return ticket
            returnTicket.ApproveStatus = TicketStatus.Rejected;
            returnTicket.Note = string.IsNullOrEmpty(returnTicket.Note)
                ? $"Rejected: {rejectionReason}"
                : $"{returnTicket.Note}\nRejected: {rejectionReason}";
            returnTicket.DateModified = DateTime.Now;

            await UpdateAsync(returnTicket);

            return returnTicket;
        }

        public async Task<IEnumerable<ReturnTicket>> GetPendingReturnRequestsAsync()
        {
            return await _unitOfWork.ReturnTickets.GetPendingReturnRequests();
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsWithConditionAsync(AssetStatus condition)
        {
            return await _unitOfWork.ReturnTickets.GetReturnTicketsWithCondition(condition);
        }
    }
}










