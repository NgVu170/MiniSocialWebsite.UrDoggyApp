using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using UrDoggy.Services.Interfaces;
using UrDoggy.Website.Hubs;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(INotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var notifications = await _notificationService.GetNotifications(userId);

            // Đánh dấu tất cả là đã đọc
            await _notificationService.MarkAllRead(userId);

            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var notifications = await _notificationService.GetNotifications(userId);
            return Ok(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            var unreadCount = await _notificationService.GetUnreadCount(userId);
            return Ok(unreadCount);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            // Lấy tất cả notifications và tìm cái cần mark as read
            var notifications = await _notificationService.GetNotifications(userId);
            var notification = notifications.FirstOrDefault(n => n.Id == notificationId);

            if (notification != null)
            {
                notification.IsRead = true;
                // Ở đây cần thêm method UpdateNotification trong service nếu muốn update individual
            }

            // Hiện tại chỉ có MarkAllRead, nên có thể cần thêm method mới trong service
            await _notificationService.MarkAllRead(userId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            await _notificationService.MarkAllRead(userId);

            // Gửi real-time update
            await _hubContext.Clients.User(userId.ToString()).SendAsync("NotificationsMarkedAsRead");

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            // Ở đây cần thêm method DeleteNotification trong service
            // Tạm thời redirect về index
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized();
            }

            // Ở đây cần thêm method ClearAllNotifications trong service
            // Tạm thời sử dụng MarkAllRead
            await _notificationService.MarkAllRead(userId);

            return RedirectToAction("Index");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
