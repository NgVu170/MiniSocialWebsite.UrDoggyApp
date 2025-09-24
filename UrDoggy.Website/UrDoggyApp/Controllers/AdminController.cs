using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IReportService _reportService;
        private readonly INotificationService _notificationService;

        public AdminController(
            IUserService userService,
            IPostService postService,
            IReportService reportService,
            INotificationService notificationService)
        {
            _userService = userService;
            _postService = postService;
            _reportService = reportService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var me = await _userService.GetById(userId.Value);
            if (me == null || !me.IsAdmin)
                return Forbid();

            var allUsers = await _userService.GetAllUsers();
            var normalUsers = allUsers.Where(u => !u.IsAdmin).ToList();
            var allPosts = await _postService.GetNewsfeed();
            var reports = await _reportService.GetAllReports();

            ViewBag.UserCount = normalUsers.Count;
            ViewBag.Users = normalUsers;
            ViewBag.PostCount = allPosts.Count();
            ViewBag.Posts = allPosts;
            ViewBag.ReportCount = reports.Count;
            ViewBag.Reports = reports;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var me = await _userService.GetById(id);
            if (me == null || !me.IsAdmin)
                return Forbid();

            var target = await _userService.GetById(id);
            if (target != null && !target.IsAdmin)
                await _userService.DeleteUser(id);

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var me = await _userService.GetById(userId.Value);
            if (me == null || !me.IsAdmin)
                return Forbid();

            var post = await _postService.GetById(id);
            if (post != null)
            {
                await _postService.DeletePost(id);
                await _reportService.DeleteReportsForPost(id);
                await _notificationService.AdminDeletedPost(post.UserId, post.Id, me.UserName);
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var userId = await _userService.GetCurrentUserId(User);
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var me = await _userService.GetById(userId);
            if (me == null || !me.IsAdmin)
                return Forbid();

            await _reportService.DeleteReport(id);
            return RedirectToAction("Dashboard");
        }
    }
}
