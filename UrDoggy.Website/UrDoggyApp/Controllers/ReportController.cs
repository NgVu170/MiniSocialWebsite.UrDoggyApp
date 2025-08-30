using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Services.Interfaces;
using UrDoggy.Services.Service;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public ReportController(IUserService userService, IReportService reportService, IPostService postService)
        {
            _reportService = reportService;
            _postService = postService;
            _userService = userService;
        }

        [HttpGet("/Report/Create/{postId:int}")]
        public async Task<IActionResult> Create(int postId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var post = await _postService.GetById(postId);
            if (post == null)
            {
                TempData["Error"] = "Bài viết không tồn tại";
                return RedirectToAction("Index", "Newsfeed");
            }

            ViewBag.PostId = postId;
            ViewBag.PostContent = post.Content.Length > 50 ? post.Content.Substring(0, 50) + "..." : post.Content;

            return View("ReportPage");
        }

        [HttpPost("/Report/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(int postId, string reason)
        {
            var reporterId = GetCurrentUserId();
            if (reporterId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Vui lòng nhập lý do báo cáo";
                return RedirectToAction("Create", new { postId });
            }

            try
            {
                var report = new Report
                {
                    PostId = postId,
                    ReporterId = reporterId,
                    Reason = reason,
                    CreatedAt = DateTime.UtcNow
                };

                await _reportService.AddReport(report);
                TempData["Success"] = "Đã gửi báo cáo thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Gửi báo cáo thất bại: " + ex.Message;
            }

            return RedirectToAction("Index", "Newsfeed");
        }

        [HttpGet("/Report/List")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> List()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetById(userId);
            if (user == null || !user.IsAdmin)
            {
                return Forbid();
            }

            var reports = await _reportService.GetAllReports();
            return View("ReportList", reports);
        }

        [HttpPost("/Report/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userService.GetById(userId);
            if (user == null || !user.IsAdmin)
            {
                return Forbid();
            }

            try
            {
                await _reportService.DeleteReport(id);
                TempData["Success"] = "Đã xóa báo cáo";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Xóa báo cáo thất bại: " + ex.Message;
            }

            return RedirectToAction("List");
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
