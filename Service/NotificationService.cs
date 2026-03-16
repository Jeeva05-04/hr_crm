using hr_crm.Data;
using hr_crm.Entities;
using hr_crm.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Service
{
    public class NotificationService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(IDbContextFactory<AppDbContext> dbFactory, IHubContext<NotificationHub> hub)
        {
            _dbFactory = dbFactory;
            _hub = hub;
        }

        // Always uses a fresh DbContext — never affected by the caller's context state
        public async Task CreateNotification(int userId, string title, string message, string module, int referenceId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Module = module,
                ReferenceId = referenceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync();

            try
            {
                await _hub.Clients.Group($"user_{userId}").SendAsync("NewNotification", new
                {
                    notification.NotificationId,
                    notification.Title,
                    notification.Message,
                    notification.Module,
                    notification.ReferenceId,
                    notification.IsRead,
                    notification.CreatedAt
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Notification] SignalR push failed for userId={userId}: {ex.Message}");
            }
        }

        public async Task<List<Notification>> GetNotifications(int userId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsRead(int notificationId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var notification = await db.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await db.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsRead(int userId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var unread = await db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            unread.ForEach(n => n.IsRead = true);
            await db.SaveChangesAsync();
        }

        public async Task<bool> DeleteNotification(int notificationId, int userId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var notification = await db.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);
            if (notification == null) return false;

            db.Notifications.Remove(notification);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAllNotifications(int userId)
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var notifications = await db.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            db.Notifications.RemoveRange(notifications);
            await db.SaveChangesAsync();
        }
    }
}
