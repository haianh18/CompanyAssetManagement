using FinalProject.Models;
using FinalProject.Repositories.Common;
using FinalProject.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Services
{
    public class DisposalTicketAssetService : BaseService<DisposalTicketAsset>, IDisposalTicketAssetService
    {
        public DisposalTicketAssetService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByDisposalTicketAsync(int disposalTicketId)
        {
            return await _unitOfWork.DisposalTicketAssets.GetDisposalTicketAssetsByDisposalTicket(disposalTicketId);
        }

        public async Task<IEnumerable<DisposalTicketAsset>> GetDisposalTicketAssetsByWarehouseAssetAsync(int warehouseAssetId)
        {
            return await _unitOfWork.DisposalTicketAssets.GetDisposalTicketAssetsByWarehouseAsset(warehouseAssetId);
        }

        public async Task<double> GetTotalDisposedPriceByDisposalTicketAsync(int disposalTicketId)
        {
            return await _unitOfWork.DisposalTicketAssets.GetTotalDisposedPriceByDisposalTicket(disposalTicketId);
        }

        public async Task<DisposalTicketAsset> GetDisposalTicketAssetWithDetailsAsync(int disposalTicketAssetId)
        {
            return await _unitOfWork.DisposalTicketAssets.GetDisposalTicketAssetWithDetails(disposalTicketAssetId);
        }
    }
}







