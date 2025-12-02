using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface INotificationService
    {
        Task EnsureCommentNotif(int userId, int postId, int commenterId, string commenterName);
        Task EnsureUpvoteNotif(int userId, int postId, int voterId, string voterName);
        Task EnsureDownvoteNotif(int userId, int postId, int voterId, string voterName);
        Task AdminDeletedPost(int userId, int postId, string adminName);
        Task EnsureFriendCommentNotif(int userId, int postId, int commenterId, string commenterName, string friendName);
        Task EnsureMessageNotif(int userId, int senderId, string senderName, string messagePreview);
        Task EnsureTagNotif(int userId, int receiverId, string receiverName, int postId);
        Task DeleteMessageNotificationsFor(int userId, int senderId);
        Task<List<Notification>> GetNotifications(int userId);
        Task<int> GetUnreadCount(int userId);
        Task MarkAllRead(int userId);
        Task DeleteNotificationsForPost(int postId);
        Task<bool> MarkAsRead(int notificationId, int userId);
        Task ClearAllNotifications(int userId);
        Task EnsureTagNotif(int userId, int receiverId, string senderName, int postId);
    }
}
