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
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
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

            return View("ReportPage");
        }

        [HttpPost("/Report/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(int postId, string reason)
        {
            var reporterId = HttpContext.Session.GetInt32("UserId");
            if (reporterId == null)
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
                    ReporterId = reporterId.Value,
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
    }
}
