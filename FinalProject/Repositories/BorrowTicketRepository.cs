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
            return await _dbSet
                .Where(bt => bt.ReturnTickets.Count == 0)
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
    }
}