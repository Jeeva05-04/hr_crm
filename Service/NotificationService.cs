using hr_crm.Data;
using hr_crm.Entities;
using Microsoft.EntityFrameworkCore;

namespace hr_crm.Service
{
    public class NotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotification(
            int userId,
            string title,
            string message,
            string module,
            int referenceId)
        {
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

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetNotifications(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}   