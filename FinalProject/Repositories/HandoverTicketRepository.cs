using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{

    public class HandoverTicketRepository : Repository<HandoverTicket>, IHandoverTicketRepository
    {
        public HandoverTicketRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverBy(int handoverById)
        {
            return await _dbSet
                .Where(ht => ht.HandoverById == handoverById)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(ht => ht.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByHandoverTo(int handoverToId)
        {
            return await _dbSet
                .Where(ht => ht.HandoverToId == handoverToId)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(ht => ht.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByOwner(int ownerId)
        {
            return await _dbSet
                .Where(ht => ht.OwnerId == ownerId)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(ht => ht.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByDepartment(int departmentId)
        {
            return await _dbSet
                .Where(ht => ht.DepartmentId == departmentId)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(ht => ht.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByWarehouseAsset(int warehouseAssetId)
        {
            return await _dbSet
                .Where(ht => ht.WarehouseAssetId == warehouseAssetId)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .OrderByDescending(ht => ht.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<HandoverTicket>> GetRecentHandoverTickets(int count)
        {
            return await _dbSet
                .OrderByDescending(ht => ht.DateCreated)
                .Take(count)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetHandoverTicketStatisticsByMonth(int year)
        {
            var handoverTickets = await _dbSet
                .Where(ht => ht.DateCreated.HasValue && ht.DateCreated.Value.Year == year)
                .ToListAsync();

            var statistics = new Dictionary<string, int>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(year, month, 1).ToString("MMMM");
                var count = handoverTickets.Count(ht => ht.DateCreated.HasValue && ht.DateCreated.Value.Month == month);
                statistics.Add(monthName, count);
            }

            return statistics;
        }

        public async Task<HandoverTicket> GetHandoverTicketWithDetails(int handoverTicketId)
        {
            return await _dbSet
                .Where(ht => ht.Id == handoverTicketId)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<HandoverTicket>> GetActiveHandoversByEmployee(int employeeId)
        {
            return await _dbSet
                .Where(ht => ht.HandoverToId == employeeId && ht.IsActive)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .OrderByDescending(ht => ht.DateCreated)
                .ToListAsync();
        }

        public async Task<HandoverTicket> GetHandoverTicketWithAssetDetails(int handoverTicketId)
        {
            return await _dbSet
                .Where(ht => ht.Id == handoverTicketId)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<HandoverTicket>> GetHandoversByDepartmentEmployee(int departmentId, int employeeId)
        {
            return await _dbSet
                .Where(ht => ht.DepartmentId == departmentId && ht.HandoverToId == employeeId && ht.IsActive)
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                .OrderByDescending(ht => ht.DateCreated)
                .ToListAsync();
        }
    }
}