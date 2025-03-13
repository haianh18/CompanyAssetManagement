using FinalProject.Models;
using FinalProject.Repositories.Common;

public interface IDisposalTicketAssetRepository : IRepository<DisposalTicketAsset>
{
    Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByDisposalTicket(int disposalTicketId);
    Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByWarehouseAsset(int warehouseAssetId);
    Task<double> GetTotalDisposedPriceByDisposalTicket(int disposalTicketId);
    Task<DisposalTicketAsset> GetDisposalTicketAssetWithDetails(int disposalTicketAssetId);
}