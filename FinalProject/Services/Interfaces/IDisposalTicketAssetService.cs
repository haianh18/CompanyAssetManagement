using FinalProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services.Interfaces
{
    public interface IDisposalTicketAssetService : IBaseService<DisposalTicketAsset>
    {
        Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByDisposalTicketAsync(int disposalTicketId);
        Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByWarehouseAssetAsync(int warehouseAssetId);
        Task<double> GetTotalDisposedPriceByDisposalTicketAsync(int disposalTicketId);
        Task<DisposalTicketAsset> GetDisposalTicketAssetWithDetailsAsync(int disposalTicketAssetId);
    }
}







