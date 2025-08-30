using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;
using UrDoggy.Data.Repositories;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Services.Service
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationRepository _notificationRepository;

        public NotificationService(NotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task EnsureCommentNotif(int userId, int postId, int commenterId, string commenterName)
        {
            var message = $"{commenterName} đã bình luận bài viết của bạn";

            if (!await _notificationRepository.Exists(userId, 1, postId, commenterId))
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Type = 1, // Comment type
                    PostId = postId,
                    TriggerId = commenterId
                };

                await _notificationRepository.Add(notification);
            }
        }

        public async Task EnsureUpvoteNotif(int userId, int postId, int voterId, string voterName)
        {
            var message = $"{voterName} đã thích bài viết của bạn";

            if (!await _notificationRepository.Exists(userId, 2, postId, voterId))
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Type = 2, // Upvote type
                    PostId = postId,
                    TriggerId = voterId
                };

                await _notificationRepository.Add(notification);
            }
        }

        public async Task EnsureDownvoteNotif(int userId, int postId, int voterId, string voterName)
        {
            var message = $"{voterName} đã không thích bài viết của bạn";

            if (!await _notificationRepository.Exists(userId, 3, postId, voterId))
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Type = 3, // Downvote type
                    PostId = postId,
                    TriggerId = voterId
                };

                await _notificationRepository.Add(notification);
            }
        }

        public async Task AdminDeletedPost(int userId, int postId, string adminName)
        {
            var message = $"{adminName} (quản trị viên) đã xóa bài viết của bạn";

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Type = 4, // Admin action type
                PostId = postId,
                TriggerId = null
            };

            await _notificationRepository.Add(notification);
        }

        public async Task EnsureFriendCommentNotif(int userId, int postId, int commenterId, string commenterName, string friendName)
        {
            var message = $"{commenterName} đã bình luận về bài viết của {friendName}";

            if (!await _notificationRepository.Exists(userId, 5, postId, commenterId))
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Type = 5, // Friend comment type
                    PostId = postId,
                    TriggerId = commenterId
                };

                await _notificationRepository.Add(notification);
            }
        }

        public async Task EnsureMessageNotif(int userId, int senderId, string senderName, string messagePreview)
        {
            var message = $"{senderName} đã gửi tin nhắn cho bạn: {messagePreview}";

            if (!await _notificationRepository.Exists(userId, 6, null, senderId))
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    Type = 6, // Message type
                    PostId = null,
                    TriggerId = senderId
                };

                await _notificationRepository.Add(notification);
            }
        }

        public async Task DeleteMessageNotificationsFor(int userId, int senderId)
        {
            await _notificationRepository.DeleteByTypeAndTrigger(userId, 6, senderId);
        }

        public async Task<List<Notification>> GetNotifications(int userId)
        {
            return await _notificationRepository.GetNotificationsByUser(userId);
        }

        public async Task<int> GetUnreadCount(int userId)
        {
            return await _notificationRepository.UnreadCount(userId);
        }

        public async Task MarkAllRead(int userId)
        {
            await _notificationRepository.MarkAsRead(userId);
        }

        public async Task DeleteNotificationsForPost(int postId)
        {
            await _notificationRepository.DeleteByPostId(postId);
        }
    }
}
