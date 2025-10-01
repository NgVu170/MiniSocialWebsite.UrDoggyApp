using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Services.Interfaces;
using UrDoggy.Services.Service;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class NewsfeedController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IMediaService _mediaService;
        private readonly IFriendService _friendService;
        private const string HiddenPostsCookieName = "HiddenPosts";
        private const string ViewedPostsCookieName = "ViewedPosts";

        public NewsfeedController(
            IPostService postService,
            ICommentService commentService,
            IUserService userService,
            INotificationService notificationService,
            IMediaService mediaService,
            IFriendService friendService)
        {
            _postService = postService;
            _commentService = commentService;
            _userService = userService;
            _notificationService = notificationService;
            _mediaService = mediaService;
            _friendService = friendService;
        }

        [HttpGet]
        [Route("/Newsfeed")]
        [Route("/Newsfeed/Index")]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            string cookieValue = Request.Cookies[HiddenPostsCookieName] ?? "";
            List<int> hiddenIds = cookieValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(text => int.TryParse(text, out var v) ? v : -1)
                .Where(id => id >= 0)
                .ToList();

            // Lấy danh sách posts đã xem từ cookie
            string viewedCookieValue = Request.Cookies[ViewedPostsCookieName] ?? "";
            HashSet<int> viewedPostIds = viewedCookieValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(text => int.TryParse(text, out var v) ? v : -1)
                .Where(id => id >= 0)
                .ToHashSet();

            // Lấy bài viết recommended và loại bỏ những post đã xem
            var allPosts = await _postService.GetRecommendedPosts(userId.Value, viewedPostIds);

            var visiblePosts = allPosts
                .Where(post => !hiddenIds.Contains(post.Id))
                .ToList();

            // THÊM: Cập nhật danh sách posts đã xem
            UpdateViewedPostsCookie(viewedPostIds, visiblePosts);

            // Lấy comments cho mỗi post
            foreach (var post in visiblePosts)
            {
                post.Comments = await _commentService.GetComments(post.Id);
                foreach (var comment in post.Comments)
                {
                    comment.User = await _userService.GetById(comment.UserId);
                }
            }

            var currentUser = userId.HasValue ? await _userService.GetById(userId.Value) : null;
            int unreadCount = await _notificationService.GetUnreadCount(userId.Value);
            var friends = userId.HasValue ? await _friendService.GetFriends(userId.Value) : new List<User>();

            ViewBag.CurrentUser = currentUser;
            ViewBag.UnreadCount = unreadCount;
            ViewBag.Friends = friends;
            ViewBag.HiddenPostIds = hiddenIds;

            return View("NewsfeedPage", visiblePosts);
        }

        [HttpGet]
        [Route("/Newsfeed/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var post = await _postService.GetById(id);
            if (post == null)
                return NotFound();

            post.Comments = await _commentService.GetComments(id);
            foreach (var comment in post.Comments)
            {
                comment.User = await _userService.GetById(comment.UserId);
            }
            return View("NewsfeedDetailPage", post);
        }

        [HttpPost]
        [Route("/Newsfeed/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<IFormFile> mediaFiles, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var mediaItems = new List<(string path, string mediaType)>();

                if (mediaFiles != null && mediaFiles.Count > 0)
                {
                    foreach (var mediaFile in mediaFiles)
                    {
                        if (mediaFile.Length > 0 && await _mediaService.IsValidMediaFile(mediaFile))
                        {
                            var filePath = await _mediaService.SaveMedia(mediaFile);
                            var mediaType = await _mediaService.GetMediaType(mediaFile);
                            mediaItems.Add((filePath, mediaType));
                        }
                    }
                }

                await _postService.CreatePost(userId.Value, content, mediaItems);
                TempData["Success"] = "Đã đăng bài viết thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đăng bài viết thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var post = await _postService.GetById(postId);
                if (post == null || post.UserId != userId)
                {
                    TempData["Error"] = "Bạn không có quyền xóa bài viết này";
                    return RedirectToAction("Index");
                }

                await _postService.DeletePost(postId);
                TempData["Success"] = "Đã xóa bài viết thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Xóa bài viết thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int postId, string content, List<IFormFile> mediaFiles)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                // Lấy post hiện tại để kiểm tra quyền sở hữu
                var existingPost = await _postService.GetById(postId);
                if (existingPost == null || existingPost.UserId != userId)
                {
                    TempData["Error"] = "Bạn không có quyền sửa bài viết này";
                    return RedirectToAction("Index");
                }

                // Xử lý media files nếu có
                var mediaItems = new List<(string path, string mediaType)>();
                if (mediaFiles != null && mediaFiles.Count > 0)
                {
                    foreach (var mediaFile in mediaFiles)
                    {
                        if (mediaFile.Length > 0 && await _mediaService.IsValidMediaFile(mediaFile))
                        {
                            var filePath = await _mediaService.SaveMedia(mediaFile);
                            var mediaType = await _mediaService.GetMediaType(mediaFile);
                            mediaItems.Add((filePath, mediaType));
                        }
                    }
                }

                // Tạo post object mới với dữ liệu updated
                var updatedPost = new Post
                {
                    Id = postId,
                    Content = content,
                    UserId = userId.Value,
                    MediaItems = mediaItems.Select(m => new Media
                    {
                        Path = m.path,
                        MediaType = m.mediaType
                    }).ToList()
                };

                // Gọi service để update
                await _postService.EditPost(updatedPost);
                TempData["Success"] = "Đã sửa bài viết thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Sửa bài viết thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/Vote")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int postId, bool isUpvote)
        {
            var uId = HttpContext.Session.GetInt32("UserId");
            var userId = uId.Value;
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                await _postService.Vote(postId, userId, isUpvote);

                // Gửi notification
                var post = await _postService.GetById(postId);
                var voter = await _userService.GetById(userId);

                if (post != null && voter != null)
                {
                    if (isUpvote)
                    {
                        await _notificationService.EnsureUpvoteNotif(post.UserId, postId, userId, voter.UserName);
                    }
                    else
                    {
                        await _notificationService.EnsureDownvoteNotif(post.UserId, postId, userId, voter.UserName);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Vote thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/Hide")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            string cookieValue = Request.Cookies[HiddenPostsCookieName] ?? "";
            var hiddenIds = GetHiddenPostIdsFromCookie(cookieValue);

            if (!hiddenIds.Contains(postId))
                hiddenIds.Add(postId);

            UpdateHiddenPostsCookie(hiddenIds);
            TempData["Success"] = "Đã ẩn bài viết";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/Unhide")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unhide(int postId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            string cookieValue = Request.Cookies[HiddenPostsCookieName] ?? "";
            var hiddenIds = GetHiddenPostIdsFromCookie(cookieValue);

            if (hiddenIds.Contains(postId))
                hiddenIds.Remove(postId);

            UpdateHiddenPostsCookie(hiddenIds);
            TempData["Success"] = "Đã hiện lại bài viết";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/AddComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var uId = HttpContext.Session.GetInt32("UserId");
            var userId = uId.Value;
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Nội dung bình luận không được để trống";
                return RedirectToAction("Index");
            }

            try
            {
                var comment = await _commentService.AddComment(postId, userId, content);

                // Gửi notification
                var post = await _postService.GetById(postId);
                var commenter = await _userService.GetById(userId);

                if (post != null && commenter != null)
                {
                    await _notificationService.EnsureCommentNotif(post.UserId, postId, userId, commenter.UserName);
                }

                TempData["Success"] = "Đã thêm bình luận";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Thêm bình luận thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/DeleteComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            try
            {
                var isOwner = await _commentService.IsCommentOwner(commentId, userId.Value);
                if (!isOwner)
                {
                    TempData["Error"] = "Bạn không có quyền xóa bình luận này";
                    return RedirectToAction("Index");
                }

                await _commentService.DeleteComment(commentId);
                TempData["Success"] = "Đã xóa bình luận";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Xóa bình luận thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Newsfeed/EditComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, int postId, string content)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Nội dung bình luận không được để trống";
                return RedirectToAction("Index");
            }

            try
            {
                var isOwner = await _commentService.IsCommentOwner(commentId, userId.Value);
                if (!isOwner)
                {
                    TempData["Error"] = "Bạn không có quyền sửa bình luận này";
                    return RedirectToAction("Index");
                }

                var comment = await _commentService.GetCommentById(commentId);
                if (comment != null)
                {
                    comment.Content = content;
                    await _commentService.UpdateComment(comment);
                    TempData["Success"] = "Đã sửa bình luận";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Sửa bình luận thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private List<int> GetHiddenPostIdsFromCookie(string cookieValue)
        {
            return cookieValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(text => int.TryParse(text, out var v) ? v : -1)
                .Where(id => id >= 0)
                .ToList();
        }

        private void UpdateHiddenPostsCookie(List<int> hiddenIds)
        {
            string newCookieValue = string.Join(",", hiddenIds);
            Response.Cookies.Append(
                HiddenPostsCookieName,
                newCookieValue,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
        }

        // Method để cập nhật cookie posts đã xem
        private void UpdateViewedPostsCookie(HashSet<int> viewedPostIds, List<Post> newPosts)
        {
            // Thêm các post mới vào danh sách đã xem
            foreach (var post in newPosts)
            {
                if (!viewedPostIds.Contains(post.Id))
                {
                    viewedPostIds.Add(post.Id);
                }
            }

            // Giới hạn số lượng posts đã xem (tránh cookie quá lớn)
            // Giữ lại 100 posts gần đây nhất
            if (viewedPostIds.Count > 100)
            {
                var recentPosts = viewedPostIds.Take(100).ToHashSet();
                viewedPostIds = recentPosts;
            }

            string newCookieValue = string.Join(",", viewedPostIds);
            Response.Cookies.Append(
                ViewedPostsCookieName,
                newCookieValue,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7), // Lưu 7 ngày
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
        }
    }
}
