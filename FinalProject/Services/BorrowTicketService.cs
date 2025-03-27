using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class BorrowTicketService : BaseService<BorrowTicket>, IBorrowTicketService
    {
        private readonly IWarehouseAssetService _warehouseAssetService;
        public BorrowTicketService(IUnitOfWork unitOfWork, IWarehouseAssetService warehouseAssetService) : base(unitOfWork)
        {
            _warehouseAssetService = warehouseAssetService;
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
            // Sửa truy vấn để đảm bảo lấy được tất cả các yêu cầu mượn chưa trả
            var borrowTickets = await _unitOfWork.BorrowTickets.GetBorrowTicketsWithoutReturn();

            // Log số lượng yêu cầu mượn tìm thấy
            Console.WriteLine($"Service found {borrowTickets.Count()} borrow tickets without return");

            return borrowTickets;
        }

        public async Task<IEnumerable<BorrowTicket>> GetRecentBorrowTicketsAsync(int count)
        {
            return await _unitOfWork.BorrowTickets.GetRecentBorrowTickets(count);
        }

        public async Task<Dictionary<string, int>> GetBorrowTicketStatisticsByMonthAsync(int year)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketStatisticsByMonth(year);
        }

        public async Task<bool> IsUserEligibleForBorrowingAsync(int userId)
        {
            return await _unitOfWork.BorrowTickets.IsUserEligibleForBorrowing(userId);
        }

        public async Task<IEnumerable<BorrowTicket>> GetActiveBorrowTicketsByUserAsync(int userId)
        {
            return await _unitOfWork.BorrowTickets.GetActiveBorrowTicketsByUser(userId);
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsExpiringInDaysAsync(int days)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketsExpiringInDays(days);
        }

        public async Task<BorrowTicket> GetBorrowTicketWithExtensionsAsync(int borrowTicketId)
        {
            return await _unitOfWork.BorrowTickets.GetBorrowTicketWithExtensions(borrowTicketId);
        }

        public async Task<bool> RequestExtensionAsync(int borrowTicketId, DateTime newReturnDate)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null) return false;

            // Check if ticket has already been extended
            if (borrowTicket.IsExtended) return false;

            // Check if extension date is valid (must be after current return date)
            if (newReturnDate <= borrowTicket.ReturnDate) return false;

            // Check if extension is within limits (max 30 days from original return date)
            var maxExtendDate = borrowTicket.ReturnDate.Value.AddDays(30);
            if (newReturnDate > maxExtendDate) return false;

            // Update borrow ticket with extension request
            borrowTicket.ExtensionRequestDate = DateTime.Now;
            borrowTicket.ExtensionApproveStatus = TicketStatus.Pending;

            _unitOfWork.BorrowTickets.Update(borrowTicket);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApproveExtensionAsync(int borrowTicketId)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null) return false;

            // Check if there's a pending extension request
            if (borrowTicket.ExtensionApproveStatus != TicketStatus.Pending || !borrowTicket.ExtensionRequestDate.HasValue)
                return false;

            // Create a new borrow ticket for the extension
            var extensionTicket = new BorrowTicket
            {
                WarehouseAssetId = borrowTicket.WarehouseAssetId,
                BorrowById = borrowTicket.BorrowById,
                OwnerId = borrowTicket.OwnerId,
                Quantity = borrowTicket.Quantity,
                Note = $"Extension of ticket #{borrowTicket.Id}",
                DateCreated = borrowTicket.ReturnDate, // Start date is the end date of original ticket
                ReturnDate = borrowTicket.ReturnDate.Value.AddDays(30), // 30 days extension
                ApproveStatus = TicketStatus.Approved,
                ExtensionBorrowTicketId = borrowTicket.Id,
                DateModified = DateTime.Now
            };

            // Update original borrow ticket
            borrowTicket.IsExtended = true;
            borrowTicket.ExtensionApproveStatus = TicketStatus.Approved;
            borrowTicket.DateModified = DateTime.Now;

            await _unitOfWork.BorrowTickets.AddAsync(extensionTicket);
            _unitOfWork.BorrowTickets.Update(borrowTicket);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectExtensionAsync(int borrowTicketId, string rejectionReason)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null) return false;

            // Check if there's a pending extension request
            if (borrowTicket.ExtensionApproveStatus != TicketStatus.Pending || !borrowTicket.ExtensionRequestDate.HasValue)
                return false;

            // Update reject status
            borrowTicket.ExtensionApproveStatus = TicketStatus.Rejected;
            borrowTicket.Note += $"\nExtension rejected: {rejectionReason}";
            borrowTicket.DateModified = DateTime.Now;

            _unitOfWork.BorrowTickets.Update(borrowTicket);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAsReturnedAsync(int borrowTicketId)
        {
            var borrowTicket = await _unitOfWork.BorrowTickets.GetByIdAsync(borrowTicketId);
            if (borrowTicket == null) return false;

            // Mark as returned
            borrowTicket.IsReturned = true;
            borrowTicket.DateModified = DateTime.Now;

            _unitOfWork.BorrowTickets.Update(borrowTicket);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}





