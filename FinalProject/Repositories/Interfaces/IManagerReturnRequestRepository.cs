using FinalProject.Models;
using FinalProject.Repositories.Common;

namespace FinalProject.Repositories.Interfaces
{
    public interface IManagerReturnRequestRepository : IRepository<ManagerReturnRequest>
    {
        Task<IEnumerable<ManagerReturnRequest>> GetPendingRequestsForUser(int userId);
        Task<IEnumerable<ManagerReturnRequest>> GetRequestsByManager(int managerId);
        Task<IEnumerable<ManagerReturnRequest>> GetAllActiveRequests();
        Task<ManagerReturnRequest> GetRequestWithDetails(int requestId);
        Task MarkAsCompleted(int requestId, int returnTicketId);
    }
}
