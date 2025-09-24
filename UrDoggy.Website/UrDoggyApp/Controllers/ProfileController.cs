using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Services.Interfaces;
using UrDoggy.Website.Models;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IFriendService _friendService;
        private readonly IMediaService _mediaService;

        public ProfileController(
            IUserService userService,
            IPostService postService,
            ICommentService commentService,
            IFriendService friendService,
            IMediaService mediaService)
        {
            _userService = userService;
            _postService = postService;
            _commentService = commentService;
            _friendService = friendService;
            _mediaService = mediaService;
        }

        [HttpGet("/Profile/{id:int?}")]
        public async Task<IActionResult> Index(int? id)
        {
            var uId = HttpContext.Session.GetInt32("UserId");
            var sessionUserId = uId.Value;
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            int profileUserId = id ?? sessionUserId;
            var profileUser = await _userService.GetById(profileUserId);
            if (profileUser == null)
            {
                return NotFound();
            }

            var userPosts = await _postService.GetUserPosts(profileUserId);

            foreach (var post in userPosts)
            {
                post.Comments = await _commentService.GetComments(post.Id);
            }

            var isFriend = await _friendService.AreFriends(sessionUserId, profileUserId);
            var hasSent = await _friendService.HasPendingRequest(sessionUserId, profileUserId);
            var hasReceived = await _friendService.HasPendingRequest(profileUserId, sessionUserId);
            var canSend = !(profileUserId == sessionUserId || isFriend || hasSent || hasReceived);

            ViewBag.Posts = userPosts;
            ViewBag.IsOwn = (profileUserId == sessionUserId);
            ViewBag.IsFriend = isFriend;
            ViewBag.HasSent = hasSent;
            ViewBag.HasReceived = hasReceived;
            ViewBag.CanSend = canSend;
            ViewBag.FriendCount = await _friendService.GetFriendCount(profileUserId);
            ViewBag.PostCount = await _postService.GetPostCount(profileUserId);

            return View("ProfilePage", profileUser);
        }

        [HttpGet("/Profile/Detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var uId = HttpContext.Session.GetInt32("UserId");
            var sessionUserId = uId.Value;
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var profileUser = await _userService.GetById(id);
            if (profileUser == null)
            {
                return NotFound();
            }

            var userPosts = await _postService.GetUserPosts(id);

            var commentsMap = new Dictionary<int, List<Comment>>();
            foreach (var post in userPosts)
            {
                commentsMap[post.Id] = await _commentService.GetComments(post.Id);
            }

            var isOwnProfile = (sessionUserId == id);
            var isFriend = await _friendService.AreFriends(sessionUserId, id);
            var hasSentRequest = await _friendService.HasPendingRequest(sessionUserId, id);
            var hasReceivedRequest = await _friendService.HasPendingRequest(id, sessionUserId);
            var canSendRequest = !(isOwnProfile || isFriend || hasSentRequest || hasReceivedRequest);

            var viewModel = new ProfileDetailViewModel
            {
                User = profileUser,
                Posts = userPosts,
                CommentsMap = commentsMap,
                IsOwnProfile = isOwnProfile,
                IsFriend = isFriend,
                HasSentRequest = hasSentRequest,
                HasReceivedRequest = hasReceivedRequest,
                CanSendRequest = canSendRequest
            };

            ViewBag.FriendCount = await _friendService.GetFriendCount(id);
            ViewBag.PostCount = await _postService.GetPostCount(id);

            return View("ProfileDetailPage", viewModel);
        }

        [HttpGet("/Profile/Edit")]
        public async Task<IActionResult> Edit()
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var profileUser = await _userService.GetById(sessionUserId.Value);
            return View("Edit", profileUser);
        }

        [HttpPost("/Profile/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            IFormFile avatar,
            string displayName,
            string bio)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            string newAvatarPath = null;

            if (avatar != null && avatar.Length > 0)
            {
                if (!await _mediaService.IsValidMediaFile(avatar))
                {
                    TempData["Error"] = "Ảnh đại diện không hợp lệ. Chỉ chấp nhận ảnh dưới 10MB";
                    return RedirectToAction("Edit");
                }

                try
                {
                    newAvatarPath = await _mediaService.SaveMedia(avatar);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Lỗi khi upload ảnh: " + ex.Message;
                    return RedirectToAction("Edit");
                }
            }

            try
            {
                await _userService.UpdateProfile(sessionUserId.Value, displayName, bio, newAvatarPath);
                TempData["Success"] = "Cập nhật hồ sơ thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cập nhật hồ sơ thất bại: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var searchResults = await _userService.Search(q ?? string.Empty);

            ViewBag.SearchQuery = q;
            return View("~/Views/Profile/SearchResults.cshtml", searchResults);
        }

    }
}
