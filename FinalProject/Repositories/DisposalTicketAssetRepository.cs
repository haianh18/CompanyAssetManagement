using FinalProject.Models;
using FinalProject.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Repositories
{


    public class DisposalTicketAssetRepository : Repository<DisposalTicketAsset>, IDisposalTicketAssetRepository
    {
        public DisposalTicketAssetRepository(CompanyAssetManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByDisposalTicket(int disposalTicketId)
        {
            return await _dbSet
                .Where(dta => dta.DisposalTicketId == disposalTicketId)
                .Include(dta => dta.DisposalTicket)
                    .ThenInclude(dt => dt.DisposalBy)
                .Include(dta => dta.DisposalTicket)
                    .ThenInclude(dt => dt.Owner)
                .Include(dta => dta.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(dta => dta.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByWarehouseAsset(int warehouseAssetId)
        {
            return await _dbSet
                .Where(dta => dta.WarehouseAssetId == warehouseAssetId)
                .Include(dta => dta.DisposalTicket)
                    .ThenInclude(dt => dt.DisposalBy)
                .Include(dta => dta.DisposalTicket)
                    .ThenInclude(dt => dt.Owner)
                .Include(dta => dta.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(dta => dta.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .ToListAsync();
        }

        public async Task<double> GetTotalDisposedPriceByDisposalTicket(int disposalTicketId)
        {
            var disposalTicketAssets = await _dbSet
                .Where(dta => dta.DisposalTicketId == disposalTicketId)
                .ToListAsync();

            return disposalTicketAssets.Sum(dta => dta.DisposedPrice ?? 0);
        }

        public async Task<DisposalTicketAsset> GetDisposalTicketAssetWithDetails(int disposalTicketAssetId)
        {
            return await _dbSet
                .Where(dta => dta.Id == disposalTicketAssetId)
                .Include(dta => dta.DisposalTicket)
                    .ThenInclude(dt => dt.DisposalBy)
                .Include(dta => dta.DisposalTicket)
                    .ThenInclude(dt => dt.Owner)
                .Include(dta => dta.WarehouseAsset)
                    .ThenInclude(wa => wa.Asset)
                        .ThenInclude(a => a.AssetCategory)
                .Include(dta => dta.WarehouseAsset)
                    .ThenInclude(wa => wa.Warehouse)
                .FirstOrDefaultAsync();
        }
    }
}