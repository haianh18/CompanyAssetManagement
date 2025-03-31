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
    public class HandoverTicketRepository : Repository<HandoverTicket>, IHandoverTicketRepository
    {

        public HandoverTicketRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public override async Task<HandoverTicket> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(ht => ht.HandoverBy)
                .Include(ht => ht.HandoverTo)
                .Include(ht => ht.Owner)
                .Include(ht => ht.Department)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(ht => ht.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync(ht => ht.Id == id);
        }

        public override async Task<IEnumerable<HandoverTicket>> GetAllAsync()
        {
            return await _dbSet
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

        public override async Task<IEnumerable<HandoverTicket>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
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

        public async Task<IEnumerable<HandoverTicket>> GetHandoverTicketsByAssetIdAsync(int assetId)
        {
            return await _dbSet
                 .Where(ht => ht.WarehouseAsset.Asset.Id == assetId)
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
        public async Task<HandoverTicket> GetHandoverTicketWithDetailsAsync(int id)
        {
            return await _context.HandoverTickets
                .Include(h => h.HandoverBy)
                .Include(h => h.HandoverTo)
                .Include(h => h.Department)
                .Include(h => h.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                .Include(h => h.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync(h => h.Id == id);
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

        // New methods from HandoverService
        public async Task UpdateHandoverTicketStatus(int handoverTicketId, bool isActive, DateTime? actualEndDate)
        {
            var handoverTicket = await _dbSet.FindAsync(handoverTicketId);
            if (handoverTicket == null)
                throw new Exception("Handover ticket not found");

            handoverTicket.IsActive = isActive;
            handoverTicket.ActualEndDate = actualEndDate;
            handoverTicket.DateModified = DateTime.Now;

            _context.Entry(handoverTicket).State = EntityState.Modified;
        }

        public async Task<bool> ValidateHandoverOperationAsync(int warehouseAssetId, int quantity, bool isReturn)
        {
            var warehouseAsset = await _context.WarehouseAssets.FindAsync(warehouseAssetId);
            if (warehouseAsset == null)
                return false;

            if (isReturn)
            {
                // Khi trả lại, kiểm tra xem có đủ số lượng đã bàn giao không
                return (warehouseAsset.HandedOverGoodQuantity ?? 0) >= quantity;
            }
            else
            {
                // Khi bàn giao mới, kiểm tra xem có đủ số lượng tốt và có sẵn không
                int availableGood = (warehouseAsset.GoodQuantity ?? 0) -
                                   (warehouseAsset.BorrowedGoodQuantity ?? 0) -
                                   (warehouseAsset.HandedOverGoodQuantity ?? 0);
                return availableGood >= quantity;
            }
        }

        public async Task ProcessHandoverEventAsync(HandoverTicket handoverTicket, string eventType, string note)
        {
            // Ghi lại lịch sử sự kiện
            var eventLog = new Dictionary<string, object>
            {
                { "HandoverTicketId", handoverTicket.Id },
                { "EventType", eventType },
                { "Timestamp", DateTime.Now },
                { "Note", note },
                { "UserId", handoverTicket.HandoverById }
            };

            // Trong một hệ thống thực tế, bạn có thể lưu lịch sử sự kiện vào cơ sở dữ liệu
            // Ví dụ: await _unitOfWork.HandoverEventLogs.AddAsync(eventLog);

            // Cập nhật handover ticket
            handoverTicket.DateModified = DateTime.Now;
            handoverTicket.Note = string.IsNullOrEmpty(handoverTicket.Note)
                ? note
                : $"{handoverTicket.Note}\n{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {eventType}: {note}";

            _context.Entry(handoverTicket).State = EntityState.Modified;
        }

        public async Task<bool> IsAssetAvailableForHandoverAsync(int warehouseAssetId, int quantity)
        {
            var warehouseAsset = await _context.WarehouseAssets.FindAsync(warehouseAssetId);
            if (warehouseAsset == null)
                return false;

            int availableQuantity = (warehouseAsset.GoodQuantity ?? 0) -
                                   (warehouseAsset.BorrowedGoodQuantity ?? 0) -
                                   (warehouseAsset.HandedOverGoodQuantity ?? 0);

            return availableQuantity >= quantity;
        }

        public async Task<IEnumerable<HandoverReturn>> GetHandoverReturnHistoryAsync(int handoverTicketId)
        {
            return await _context.Set<HandoverReturn>()
                .Where(hr => hr.HandoverTicketId == handoverTicketId)
                .Include(hr => hr.ReturnBy)
                .Include(hr => hr.ReceivedBy)
                .OrderByDescending(hr => hr.ReturnDate)
                .ToListAsync();
        }
    }
}