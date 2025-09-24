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
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var notifications = await _notificationService.GetNotifications(userId.Value);

            // Đánh dấu tất cả là đã đọc
            await _notificationService.MarkAllRead(userId.Value);

            return View(notifications);
        }
    }
}
