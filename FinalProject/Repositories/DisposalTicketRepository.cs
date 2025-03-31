using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{


    public class DisposalTicketRepository : Repository<DisposalTicket>, IDisposalTicketRepository
    {
        public DisposalTicketRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public override async Task<DisposalTicket> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .FirstOrDefaultAsync(dt => dt.Id == id);
        }

        public override async Task<IEnumerable<DisposalTicket>> GetAllAsync()
        {
            return await _dbSet
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .OrderByDescending(dt => dt.DateCreated)
                .ToListAsync();
        }

        public override async Task<IEnumerable<DisposalTicket>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .OrderByDescending(dt => dt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByDisposalBy(int disposalById)
        {
            return await _dbSet
                .Where(dt => dt.DisposalById == disposalById)
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(dt => dt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisposalTicket>> GetDisposalTicketsByOwner(int ownerId)
        {
            return await _dbSet
                .Where(dt => dt.OwnerId == ownerId)
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(dt => dt.DateCreated)
                .ToListAsync();
        }

        public async Task<DisposalTicket> GetDisposalTicketWithAssets(int disposalTicketId)
        {
            return await _dbSet
                .Where(dt => dt.Id == disposalTicketId)
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DisposalTicket>> GetDisposalTicketsWithAssets()
        {
            return await _dbSet
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(dt => dt.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisposalTicket>> GetRecentDisposalTickets(int count)
        {
            return await _dbSet
                .OrderByDescending(dt => dt.DateCreated)
                .Take(count)
                .Include(dt => dt.DisposalBy)
                .Include(dt => dt.Owner)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(dt => dt.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetDisposalTicketStatisticsByMonth(int year)
        {
            var disposalTickets = await _dbSet
                .Where(dt => dt.DateCreated.HasValue && dt.DateCreated.Value.Year == year)
                .ToListAsync();

            var statistics = new Dictionary<string, int>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(year, month, 1).ToString("MMMM");
                var count = disposalTickets.Count(dt => dt.DateCreated.HasValue && dt.DateCreated.Value.Month == month);
                statistics.Add(monthName, count);
            }

            return statistics;
        }

        public async Task<double> GetTotalDisposalValue(int year)
        {
            var disposalTickets = await _dbSet
                .Where(dt => dt.DateCreated.HasValue && dt.DateCreated.Value.Year == year)
                .Include(dt => dt.DisposalTicketAssets)
                .ToListAsync();

            double totalValue = 0;

            foreach (var ticket in disposalTickets)
            {
                totalValue += ticket.DisposalTicketAssets.Sum(dta => dta.DisposedPrice ?? 0);
            }

            return totalValue;
        }
        public async Task<DisposalTicket> GetDisposalTicketWithDetails(int id)
        {
            return await _context.DisposalTickets
                .Include(d => d.DisposalBy)
                .Include(d => d.Owner)
                .Include(d => d.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .Include(d => d.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<DisposalTicket>> GetAllWithDetailsAsync()
        {
            return await _context.DisposalTickets
                .Include(d => d.DisposalBy)
                .Include(d => d.Owner)
                .Include(d => d.DisposalTicketAssets)
                    .ThenInclude(dta => dta.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .OrderByDescending(d => d.DateCreated)
                .ToListAsync();
        }
    }
}