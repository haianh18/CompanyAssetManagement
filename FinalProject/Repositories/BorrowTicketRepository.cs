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

        public async Task<IEnumerable<BorrowTicket>> GetBorrowTicketsByUser(int userId)
        {
            return await _dbSet
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
                .Where(bt => bt.ReturnDate.HasValue && bt.ReturnDate < currentDate && bt.ReturnTickets.Count == 0)
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