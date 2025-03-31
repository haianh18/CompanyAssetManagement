using FinalProject.Models;
using FinalProject.Repositories.Interfaces;

namespace FinalProject.Repositories
{
    public class NotificationRepository /*: INotificationRepository*/
    {

        //public NotificationRepository(CompanyAssetManagementContext context)
        //{

        //}

        //public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
        //{
        //    return await _dbSet.Notifications
        //        .Where(n => n.UserId == userId)
        //        .OrderByDescending(n => n.DateCreated)
        //        .ToListAsync();
        //}

        //public async Task<int> GetUnreadCountAsync(int userId)
        //{
        //    return await _context.Notifications
        //        .CountAsync(n => n.UserId == userId && !n.IsRead);
        //}

        //public async Task<Notification> CreateAsync(Notification notification)
        //{
        //    notification.DateCreated = DateTime.Now;
        //    _context.Notifications.Add(notification);
        //    await _context.SaveChangesAsync();
        //    return notification;
        //}

        //public async Task MarkAsReadAsync(int id)
        //{
        //    var notification = await _context.Notifications.FindAsync(id);
        //    if (notification != null)
        //    {
        //        notification.IsRead = true;
        //        notification.DateModified = DateTime.Now;
        //        _context.Update(notification);
        //        await _context.SaveChangesAsync();
        //    }
        //}

        //public async Task MarkAllAsReadForUserAsync(int userId)
        //{
        //    var notifications = await _context.Notifications
        //        .Where(n => n.UserId == userId && !n.IsRead)
        //        .ToListAsync();

        //    foreach (var notification in notifications)
        //    {
        //        notification.IsRead = true;
        //        notification.DateModified = DateTime.Now;
        //    }

        //    await _context.SaveChangesAsync();
        //}
    }
}
