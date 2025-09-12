using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Services.Interfaces;
using UrDoggy.Services.Service;
using UrDoggy.Website.Hubs;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUserService _userService;

        public NotificationsController(INotificationService notificationService, IHubContext<NotificationHub> hubContext, IUserService userService)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = await _userService.GetCurrentUserId(User);
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
            var userId = await _userService.GetCurrentUserId(User);
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
            var userId = await _userService.GetCurrentUserId(User);
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
            var userId = await _userService.GetCurrentUserId(User);
            if (userId == 0)
            {
                return Unauthorized();
            }

            var success = await _notificationService.MarkAsRead(notificationId, userId);

            if (success)
            {
                // Gửi real-time update
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("NotificationMarkedAsRead", notificationId);
            }

            return Ok(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = await _userService.GetCurrentUserId(User);
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
            var userId = await _userService.GetCurrentUserId(User);
            if (userId == 0)
            {
                return Unauthorized();
            }
            await _notificationService.DeleteNotificationsForPost(notificationId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            var userId = await _userService.GetCurrentUserId(User);
            if (userId == 0)
            {
                return Unauthorized();
            }

            await _notificationService.ClearAllNotifications(userId);

            // Gửi real-time update
            await _hubContext.Clients.User(userId.ToString()).SendAsync("AllNotificationsCleared");

            return RedirectToAction("Index");
        }
    }
}
