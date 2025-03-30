using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{


    public class BorrowTicketRepository : Repository<BorrowTicket>, IBorrowTicketRepository
    {
        public BorrowTicketRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<BorrowTicket>> GetAllAsync()
        {
            return await _dbSet
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderByDescending(bt => bt.DateCreated)
                .ToListAsync();
        }

        public override async Task<IEnumerable<BorrowTicket>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderByDescending(bt => bt.DateCreated)
                .ToListAsync();
        }

        public async Task<bool> RequestExtensionAsync(int borrowTicketId, DateTime newReturnDate)
        {
            var borrowTicket = await GetByIdAsync(borrowTicketId);
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

            Update(borrowTicket);
            return true;
        }

        public async Task<bool> ApproveExtensionAsync(int borrowTicketId)
        {
            var borrowTicket = await GetByIdAsync(borrowTicketId);
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

            await AddAsync(extensionTicket);
            Update(borrowTicket);
            return true;
        }

        public async Task<bool> RejectExtensionAsync(int borrowTicketId, string rejectionReason)
        {
            var borrowTicket = await GetByIdAsync(borrowTicketId);
            if (borrowTicket == null) return false;

            // Check if there's a pending extension request
            if (borrowTicket.ExtensionApproveStatus != TicketStatus.Pending || !borrowTicket.ExtensionRequestDate.HasValue)
                return false;

            // Update reject status
            borrowTicket.ExtensionApproveStatus = TicketStatus.Rejected;
            borrowTicket.Note += $"\nExtension rejected: {rejectionReason}";
            borrowTicket.DateModified = DateTime.Now;

            Update(borrowTicket);
            return true;
        }

        public async Task<bool> MarkAsReturnedAsync(int borrowTicketId)
        {
            var borrowTicket = await GetByIdAsync(borrowTicketId);
            if (borrowTicket == null) return false;

            // Mark as returned
            borrowTicket.IsReturned = true;
            borrowTicket.DateModified = DateTime.Now;

            Update(borrowTicket);
            return true;
        }

        public override async Task<BorrowTicket> GetByIdAsync(int id)
        {
            return await _dbSet
                 .Include(bt => bt.BorrowBy)
                 .Include(bt => bt.Owner)
                 .Include(bt => bt.WarehouseAsset)
                     .ThenInclude(wa => wa.Asset)
                         .ThenInclude(a => a.AssetCategory)
                 .Include(bt => bt.WarehouseAsset)
                     .ThenInclude(wa => wa.Warehouse)
                 .Include(bt => bt.ReturnTickets)
                 .FirstOrDefaultAsync(bt => bt.Id == id);
        }

        // Kiểm tra lại phương thức này, đảm bảo nó truy vấn đúng
        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByUser(int userId)
        {
            // Thêm logging để debug
            Console.WriteLine($"Looking for borrow tickets for user ID: {userId}");

            var tickets = await _dbSet
                .Where(bt => bt.BorrowById == userId)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderByDescending(bt => bt.DateCreated)
                .ToListAsync();

            // Thêm logging để debug
            Console.WriteLine($"Found {tickets.Count()} tickets");

            return tickets;
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByOwner(int ownerId)
        {
            return await _dbSet
                .Where(bt => bt.OwnerId == ownerId)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderByDescending(bt => bt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByAsset(int assetId)
        {
            return await _dbSet
                .Where(bt => bt.WarehouseAsset.AssetId == assetId)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderByDescending(bt => bt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByWarehouseAsset(int warehouseAssetId)
        {
            return await _dbSet
                .Where(bt => bt.WarehouseAssetId == warehouseAssetId)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderByDescending(bt => bt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowTicket>> GetOverdueBorrowTickets()
        {
            var currentDate = DateTime.Now;
            return await _dbSet
                .Where(bt => bt.ReturnDate.HasValue && bt.ReturnDate < currentDate && bt.ReturnTickets.Count == 0 && !bt.IsReturned)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderBy(bt => bt.ReturnDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsWithoutReturn()
        {
            // Lấy các yêu cầu mượn chưa trả hoặc chưa được đánh dấu là đã trả
            return await _dbSet
                .Where(bt => !bt.IsReturned)  // Tập trung vào điều kiện này
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .OrderByDescending(bt => bt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowTicket>> GetRecentBorrowTickets(int count)
        {
            return await _dbSet
                .OrderByDescending(bt => bt.DateCreated)
                .Take(count)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .Include(bt => bt.ReturnTickets)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetBorrowTicketStatisticsByMonth(int year)
        {
            var borrowTickets = await _dbSet
                .Where(bt => bt.DateCreated.HasValue && bt.DateCreated.Value.Year == year)
                .ToListAsync();

            var statistics = new Dictionary<string, int>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(year, month, 1).ToString("MMMM");
                var count = borrowTickets.Count(bt => bt.DateCreated.HasValue && bt.DateCreated.Value.Month == month);
                statistics.Add(monthName, count);
            }

            return statistics;
        }

        public async Task<bool> IsUserEligibleForBorrowing(int userId)
        {
            // Check if user has any overdue tickets
            var currentDate = DateTime.Now;
            var overdueTickets = await _dbSet
                .Where(bt => bt.BorrowById == userId &&
                             bt.ReturnDate < currentDate &&
                             !bt.IsReturned &&
                             bt.ApproveStatus == TicketStatus.Approved)
                .ToListAsync();

            return !overdueTickets.Any();
        }

        public async Task<IEnumerable<BorrowTicket>> GetActiveBorrowTicketsByUser(int userId)
        {
            return await _dbSet
                .Where(bt => bt.BorrowById == userId &&
                             !bt.IsReturned &&
                             bt.ApproveStatus == TicketStatus.Approved)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(bt => bt.ReturnDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsExpiringInDays(int days)
        {
            var currentDate = DateTime.Now;
            var expirationDate = currentDate.AddDays(days);

            return await _dbSet
                .Where(bt => !bt.IsReturned &&
                             bt.ApproveStatus == TicketStatus.Approved &&
                             bt.ReturnDate >= currentDate &&
                             bt.ReturnDate <= expirationDate)
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                .OrderBy(bt => bt.ReturnDate)
                .ToListAsync();
        }

        public async Task<BorrowTicket> GetBorrowTicketWithExtensions(int borrowTicketId)
        {
            return await _dbSet
                .Include(bt => bt.BorrowBy)
                .Include(bt => bt.Owner)
                .Include(bt => bt.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                .Include(bt => bt.ExtendedBorrowTickets)
                .Include(bt => bt.OriginalBorrowTicket)
                .FirstOrDefaultAsync(bt => bt.Id == borrowTicketId);
        }

        public async Task<bool> HasUserOverdueTickets(int userId)
        {
            var currentDate = DateTime.Now;
            return await _dbSet
                .AnyAsync(bt => bt.BorrowById == userId &&
                              bt.ReturnDate < currentDate &&
                              !bt.IsReturned &&
                              bt.ApproveStatus == TicketStatus.Approved);
        }

        public async Task<bool> HasUserExtendedBorrowTicket(int borrowTicketId)
        {
            var borrowTicket = await _dbSet.FindAsync(borrowTicketId);
            if (borrowTicket == null)
                return false;

            return borrowTicket.IsExtended;
        }
    }
}