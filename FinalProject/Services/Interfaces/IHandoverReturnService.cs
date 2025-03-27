using FinalProject.Enums;
using FinalProject.Models;

namespace FinalProject.Services.Interfaces
{
    public interface IHandoverReturnService : IBaseService<HandoverReturn>
    {
        Task<IEnumerable<HandoverReturn>> GetHandoverReturnsByEmployeeAsync(int employeeId);
        Task<HandoverReturn> GetHandoverReturnWithDetailsAsync(int handoverReturnId);
        Task<IEnumerable<HandoverReturn>> GetPendingHandoverReturnsAsync();
        Task<HandoverReturn> CreateHandoverReturnAsync(int handoverTicketId, int returnById, string notes);
        Task<HandoverReturn> ApproveHandoverReturnAsync(int handoverReturnId, AssetStatus assetCondition, string notes);
    }
}
