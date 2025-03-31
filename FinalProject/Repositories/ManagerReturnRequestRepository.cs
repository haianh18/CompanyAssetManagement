using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Repositories
{
    public class ManagerReturnRequestRepository : Repository<ManagerReturnRequest>, IManagerReturnRequestRepository
    {
        public ManagerReturnRequestRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<ManagerReturnRequest>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.BorrowBy)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .Include(r => r.RequestedBy)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task<ManagerReturnRequest> GetManagerReturnRequestsByBorrowTicket(int borrowTicketId)
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.BorrowBy)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .Include(r => r.RequestedBy)
                .FirstOrDefaultAsync(r => r.BorrowTicketId == borrowTicketId);
        }

        public override async Task<ManagerReturnRequest> GetByIdAsync(int id)
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.BorrowBy)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .Include(r => r.RequestedBy)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IEnumerable<ManagerReturnRequest>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters()
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.BorrowBy)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .Include(r => r.RequestedBy)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task<IEnumerable<ManagerReturnRequest>> GetPendingRequestsForUser(int userId)
        {
            return await _dbSet
                .Where(r => r.BorrowTicket.BorrowById == userId && r.Status == TicketStatus.Pending)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .Include(r => r.RequestedBy)
                .OrderBy(r => r.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ManagerReturnRequest>> GetAllActiveRequests()
        {
            return await _dbSet
                .Where(r => r.Status == TicketStatus.Pending)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.BorrowBy)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .Include(r => r.RequestedBy)
                .OrderBy(r => r.DueDate)
                .ToListAsync();
        }

        public async Task<ManagerReturnRequest> GetRequestWithDetails(int requestId)
        {
            return await _dbSet.IgnoreQueryFilters()
                .Where(r => r.Id == requestId)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.BorrowBy)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .Include(r => r.RequestedBy)
                .Include(r => r.ReturnTicket)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ManagerReturnRequest>> GetRequestsByManager(int managerId)
        {
            return await _dbSet.IgnoreQueryFilters()
                .Where(r => r.RequestedById == managerId)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.BorrowBy)
                .Include(r => r.BorrowTicket)
                    .ThenInclude(b => b.WarehouseAsset)
                        .ThenInclude(w => w.Asset)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task MarkAsCompleted(int requestId, int returnTicketId)
        {
            var request = await _dbSet.FindAsync(requestId);
            if (request != null)
            {
                request.Status = TicketStatus.Approved;
                request.CompletionDate = DateTime.Now;
                request.RelatedReturnTicketId = returnTicketId;
                request.DateModified = DateTime.Now;

                _context.Entry(request).State = EntityState.Modified;
            }
        }
    }
}
