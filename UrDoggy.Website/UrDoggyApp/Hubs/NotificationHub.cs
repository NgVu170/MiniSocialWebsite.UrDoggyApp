using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace UrDoggy.Website.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private string GetUserId() => Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications_{userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notifications_{userId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToNotifications()
        {
            var userId = GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"notifications_{userId}");
        }

        public async Task UnsubscribeFromNotifications()
        {
            var userId = GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notifications_{userId}");
        }

        // Method để gửi notification real-time
        public async Task SendNotification(int userId, string message, int type, int? postId = null, int? triggerId = null)
        {
            await Clients.Group($"notifications_{userId}").SendAsync("ReceiveNotification", new
            {
                Message = message,
                Type = type,
                PostId = postId,
                TriggerId = triggerId,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
