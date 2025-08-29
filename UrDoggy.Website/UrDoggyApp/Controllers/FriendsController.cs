using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly IFriendService _friendService;
        private readonly IUserService _userService;

        public FriendsController(
            IFriendService friendService,
            IUserService userService)
        {
            _friendService = friendService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string tab = "all")
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            var friends = await _friendService.GetFriends(userId);
            var rawReqs = await _friendService.GetPendingRequests(userId);

            var requestMap = rawReqs
                .GroupBy(r => r.RequesterId)
                .ToDictionary(g => g.Key, g => g.First().RequestId);

            var incomingIds = requestMap.Keys.ToHashSet();

            var allUsers = await _userService.Search("");
            var friendIds = friends.Select(u => u.Id).ToHashSet();

            var newFriends = allUsers
                .Where(u => u.Id != userId && !friendIds.Contains(u.Id))
                .OrderByDescending(u => incomingIds.Contains(u.Id))
                .ToList();

            ViewBag.ActiveTab = tab.ToLower();
            ViewBag.Friends = friends;
            ViewBag.Requests = rawReqs;
            ViewBag.RequestMap = requestMap;
            ViewBag.IncomingIds = incomingIds;
            ViewBag.NewFriends = newFriends;
            ViewBag.CurrentUserId = userId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(int friendId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            try
            {
                await _friendService.SendRequest(userId, friendId);
                TempData["Success"] = "Đã gửi lời mời kết bạn";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Respond(int requestId, bool accept)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            try
            {
                await _friendService.RespondToRequest(requestId, accept);
                TempData["Success"] = accept ? "Đã chấp nhận lời mời kết bạn" : "Đã từ chối lời mời kết bạn";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index", new { tab = "requests" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfriend(int friendId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Auth");

            try
            {
                await _friendService.RemoveFriend(userId, friendId);
                TempData["Success"] = "Đã hủy kết bạn";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> SearchFriends(string query)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var users = await _userService.Search(query);
            var friends = await _friendService.GetFriends(userId);
            var friendIds = friends.Select(f => f.Id).ToHashSet();

            var results = users
                .Where(u => u.Id != userId)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.DisplayName,
                    u.ProfilePicture,
                    IsFriend = friendIds.Contains(u.Id),
                    CanSendRequest = !friendIds.Contains(u.Id)
                })
                .ToList();

            return Ok(results);
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
