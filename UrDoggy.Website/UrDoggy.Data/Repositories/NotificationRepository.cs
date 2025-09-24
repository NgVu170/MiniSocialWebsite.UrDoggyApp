using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models; // Notification

namespace UrDoggy.Data.Repositories
{
    public class NotificationRepository
    {
        private readonly ApplicationDbContext _context;
        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Notification>> GetNotificationsByUser(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task Add(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int userId, int type, int? postId, int? triggerId)
        {
            return await _context.Notifications.AnyAsync(x =>
                x.UserId == userId && x.Type == type 
                && (x.PostId ?? 0) == (postId ?? 0) 
                && (x.TriggerId ?? 0) == (triggerId ?? 0));
        }

        public async Task<List<Notification>> GetByUser(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task MarkAllRead(int userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(n => n.SetProperty(n => n.IsRead, true));
        }

        public async Task<int> UnreadCount(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task DeleteByPostId(int postId)
        {
             await _context.Notifications.Where(n => n.PostId == postId)
                .ExecuteDeleteAsync();
        }

        public async Task DeleteByTypeAndTrigger(int userId, int type, int triggerId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && n.Type == type && n.TriggerId == triggerId)
                .ExecuteDeleteAsync();
        }

        public async Task<bool> MarkAsRead(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearAllNotifications(int userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId)
                .ExecuteDeleteAsync();
        }
    }
}
