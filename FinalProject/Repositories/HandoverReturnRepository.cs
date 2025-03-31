using FinalProject.Enums;
using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Repositories
{
    public class HandoverReturnRepository : Repository<HandoverReturn>, IHandoverReturnRepository
    {
        public HandoverReturnRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public override async Task<HandoverReturn> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(hr => hr.HandoverTicket)
                .FirstOrDefaultAsync(hr => hr.Id == id);
        }

        public async Task<IEnumerable<HandoverReturn>> GetHandoverReturnsByEmployee(int employeeId)
        {
            return await _dbSet
                .Where(hr => hr.ReturnById == employeeId)
                .Include(hr => hr.HandoverTicket)
                    .ThenInclude(ht => ht.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .Include(hr => hr.ReturnBy)
                .Include(hr => hr.ReceivedBy)
                .OrderByDescending(hr => hr.ReturnDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<HandoverReturn>> GetHandoverReturnsByTicketId(int handoverTicketId)
        {
            return await _context.HandoverReturn
                .Include(hr => hr.ReturnBy)
                .Include(hr => hr.ReceivedBy)
                .Where(hr => hr.HandoverTicketId == handoverTicketId)
                .OrderByDescending(hr => hr.DateCreated)
                .ToListAsync();
        }

        public async Task<HandoverReturn> GetHandoverReturnWithDetails(int id)
        {
            return await _context.HandoverReturn
                .Include(hr => hr.HandoverTicket)
                    .ThenInclude(ht => ht.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .Include(hr => hr.ReturnBy)
                .Include(hr => hr.ReceivedBy)
                .FirstOrDefaultAsync(hr => hr.Id == id);
        }


        public async Task<IEnumerable<HandoverReturn>> GetPendingHandoverReturns()
        {
            return await _context.HandoverReturn
                .Include(hr => hr.HandoverTicket)
                    .ThenInclude(ht => ht.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .Include(hr => hr.ReturnBy)
                .Where(hr => !hr.DateModified.HasValue)
                .ToListAsync();
        }

    }
}
