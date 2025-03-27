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


    public class ReturnTicketRepository : Repository<ReturnTicket>, IReturnTicketRepository
    {
        public ReturnTicketRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsByUser(int userId)
        {
            return await _dbSet
                .Where(rt => rt.ReturnById == userId)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(rt => rt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsByOwner(int ownerId)
        {
            return await _dbSet
                .Where(rt => rt.OwnerId == ownerId)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(rt => rt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsByBorrowTicket(int borrowTicketId)
        {
            return await _dbSet
                .Where(rt => rt.BorrowTicketId == borrowTicketId)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(rt => rt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReturnTicket>> GetRecentReturnTickets(int count)
        {
            return await _dbSet
                .OrderByDescending(rt => rt.DateCreated)
                .Take(count)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetReturnTicketStatisticsByMonth(int year)
        {
            var returnTickets = await _dbSet
                .Where(rt => rt.DateCreated.HasValue && rt.DateCreated.Value.Year == year)
                .ToListAsync();

            var statistics = new Dictionary<string, int>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(year, month, 1).ToString("MMMM");
                var count = returnTickets.Count(rt => rt.DateCreated.HasValue && rt.DateCreated.Value.Month == month);
                statistics.Add(monthName, count);
            }

            return statistics;
        }

        public async Task<ReturnTicket> GetReturnTicketWithDetails(int returnTicketId)
        {
            return await _dbSet
                .Where(rt => rt.Id == returnTicketId)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync();
        }

        public async Task<ReturnTicket> GetReturnTicketWithBorrowDetails(int returnTicketId)
        {
            return await _dbSet
                .Where(rt => rt.Id == returnTicketId)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.BorrowBy)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ReturnTicket>> GetPendingReturnRequests()
        {
            return await _dbSet
                .Where(rt => rt.ApproveStatus == TicketStatus.Pending)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .OrderByDescending(rt => rt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReturnTicket>> GetReturnTicketsWithCondition(AssetStatus condition)
        {
            return await _dbSet
                .Where(rt => rt.AssetConditionOnReturn == condition)
                .Include(rt => rt.ReturnBy)
                .Include(rt => rt.Owner)
                .Include(rt => rt.BorrowTicket)
                    .ThenInclude(bt => bt.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .OrderByDescending(rt => rt.DateCreated)
                .ToListAsync();
        }
    }
}