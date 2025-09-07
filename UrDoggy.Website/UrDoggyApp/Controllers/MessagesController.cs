using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Services.Interfaces;
using UrDoggy.Website.Hubs;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IFriendService _friendService;
        private readonly IUserService _userService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(
            IMessageService messageService,
            IFriendService friendService,
            IUserService userService,
            IHubContext<ChatHub> hubContext)
        {
            _messageService = messageService;
            _friendService = friendService;
            _userService = userService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? otherUserId)
        {
            var meId = await _userService.GetCurrentUserId(User);
            if (meId == 0)
                return RedirectToAction("Login", "Auth");

            int me = meId;

            // Lấy danh sách bạn bè
            var friends = await _friendService.GetFriends(me);
            var friendConversations = friends.Select(f => new ConversationDto
            {
                OtherId = f.Id,
                OtherUsername = f.UserName,
                OtherPicture = f.ProfilePicture,
                Preview = "",
                PreviewSenderId = 0,
                Time = f.CreatedAt
            }).ToList();

            // Lấy danh sách người đã chat
            var partnerIds = await _messageService.GetChatPartnerIds(me);
            var partnerConversations = new List<ConversationDto>();

            foreach (var partnerId in partnerIds)
            {
                var lastMessage = await _messageService.GetLastMessage(me, partnerId);
                var partnerUser = await _userService.GetById(partnerId);

                if (partnerUser != null)
                {
                    partnerConversations.Add(new ConversationDto
                    {
                        OtherId = partnerId,
                        OtherUsername = partnerUser.UserName,
                        OtherPicture = partnerUser.ProfilePicture,
                        Preview = lastMessage?.Content ?? "",
                        PreviewSenderId = lastMessage?.SenderId ?? 0,
                        Time = lastMessage?.SentAt ?? DateTime.MinValue
                    });
                }
            }

            // Kết hợp và sắp xếp conversations
            var allConversations = friendConversations
                .Concat(partnerConversations)
                .GroupBy(c => c.OtherId)
                .Select(g => g.OrderByDescending(x => x.Time).First())
                .OrderByDescending(c => c.Time)
                .ToList();

            // Xác định active conversation
            int activeUserId = otherUserId ?? allConversations.FirstOrDefault()?.OtherId ?? 0;

            // Lấy thread messages
            List<Message> thread = new List<Message>();
            if (activeUserId > 0)
            {
                thread = await _messageService.GetThread(me, activeUserId);
            }

            ViewBag.Chats = allConversations;
            ViewBag.ActiveUser = activeUserId;
            ViewBag.Thread = thread;
            ViewBag.CurrentUserId = me;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int toUserId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                TempData["Error"] = "Tin nhắn không được để trống";
                return RedirectToAction("Index", new { otherUserId = toUserId });
            }

            var meId = await _userService.GetCurrentUserId(User);
            if (meId == 0)
                return RedirectToAction("Login", "Auth");

            try
            {
                // Gửi tin nhắn
                var message = await _messageService.SendMessage(meId, toUserId, content);

                // Gửi real-time notification qua SignalR
                await _hubContext.Clients.Group(toUserId.ToString())
                    .SendAsync("ReceiveMessage", meId, message.Content, message.SentAt);

                return RedirectToAction("Index", new { otherUserId = toUserId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Gửi tin nhắn thất bại: " + ex.Message;
                return RedirectToAction("Index", new { otherUserId = toUserId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int messageId, int? otherUserId)
        {
            var meId = await _userService.GetCurrentUserId(User);
            if (meId == 0)
                return RedirectToAction("Login", "Auth");

            try
            {
                await _messageService.DeleteMessage(meId, messageId);
                TempData["Success"] = "Đã xóa tin nhắn";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Xóa tin nhắn thất bại: " + ex.Message;
            }

            return RedirectToAction("Index", new { otherUserId });
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var meId = await _userService.GetCurrentUserId(User);
            if (meId == 0)
                return Unauthorized();

            var conversations = await _messageService.GetConversations(meId);
            return Ok(conversations);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int otherUserId)
        {
            var meId = await _userService.GetCurrentUserId(User);
            if (meId == 0)
                return Unauthorized();

            await _messageService.MarkAsRead(meId, otherUserId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var meId = await _userService.GetCurrentUserId(User);
            if (meId == 0)
                return Unauthorized();

            // Tính tổng unread messages từ tất cả conversations
            var partnerIds = await _messageService.GetChatPartnerIds(meId);
            int totalUnread = 0;

            foreach (var partnerId in partnerIds)
            {
                totalUnread += await _messageService.GetUnreadCount(meId, partnerId);
            }

            return Ok(totalUnread);
        }

    }
}
