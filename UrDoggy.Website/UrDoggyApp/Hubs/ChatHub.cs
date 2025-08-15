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

        public Task JoinConversation(string conversationId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public Task LeaveConversation(string conversationId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendToConversation (string conversationId, string message)
        {
            var userId = GetUserId();
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", userId, message);
        }
    }
}
