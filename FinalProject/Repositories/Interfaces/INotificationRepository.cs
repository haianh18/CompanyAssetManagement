using FinalProject.Models;

namespace FinalProject.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notification> CreateAsync(Notification notification);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadForUserAsync(int userId);
    }
}
