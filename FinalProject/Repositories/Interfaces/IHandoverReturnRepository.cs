using FinalProject.Models;
using FinalProject.Repositories.Common;

namespace FinalProject.Repositories.Interfaces
{
    public interface IHandoverReturnRepository : IRepository<HandoverReturn>
    {
        Task<IEnumerable<HandoverReturn>> GetHandoverReturnsByEmployee(int employeeId);
        Task<HandoverReturn> GetHandoverReturnWithDetails(int handoverReturnId);
        Task<IEnumerable<HandoverReturn>> GetPendingHandoverReturns();
    }
}
