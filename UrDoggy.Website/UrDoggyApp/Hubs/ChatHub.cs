using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace UrDoggy.Website.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private string GetUserId() => Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinConversation(int otherUserId)
        {
            var userId = GetUserId();
            var conversationId = GetConversationId(userId, otherUserId.ToString());
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task LeaveConversation(int otherUserId)
        {
            var userId = GetUserId();
            var conversationId = GetConversationId(userId, otherUserId.ToString());
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendToConversation(int otherUserId, string message)
        {
            var userId = GetUserId();
            var conversationId = GetConversationId(userId, otherUserId.ToString());

            await Clients.Group(conversationId).SendAsync("ReceiveMessage",
                int.Parse(userId), message, DateTime.UtcNow);

            // Gửi notification đến người nhận
            await Clients.Group(otherUserId.ToString()).SendAsync("NewMessageNotification",
                int.Parse(userId), message);
        }

        private string GetConversationId(string user1, string user2)
        {
            // Tạo conversation ID duy nhất từ 2 user IDs
            var ids = new[] { user1, user2 }.OrderBy(id => id).ToArray();
            return $"conversation_{ids[0]}_{ids[1]}";
        }
    }
}
