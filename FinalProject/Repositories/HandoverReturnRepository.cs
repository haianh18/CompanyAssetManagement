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

        public async Task<HandoverReturn> GetHandoverReturnWithDetails(int handoverReturnId)
        {
            return await _dbSet
                .Where(hr => hr.Id == handoverReturnId)
                .Include(hr => hr.HandoverTicket)
                    .ThenInclude(ht => ht.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                            .ThenInclude(a => a.AssetCategory)
                .Include(hr => hr.HandoverTicket)
                    .ThenInclude(ht => ht.HandoverTo)
                .Include(hr => hr.ReturnBy)
                .Include(hr => hr.ReceivedBy)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<HandoverReturn>> GetPendingHandoverReturns()
        {
            return await _dbSet
                .Where(hr => hr.ReturnDate.HasValue && !hr.HandoverTicket.ActualEndDate.HasValue)
                .Include(hr => hr.HandoverTicket)
                    .ThenInclude(ht => ht.WarehouseAsset)
                        .ThenInclude(wa => wa.Asset)
                .Include(hr => hr.ReturnBy)
                .OrderByDescending(hr => hr.ReturnDate)
                .ToListAsync();
        }

    }
}
