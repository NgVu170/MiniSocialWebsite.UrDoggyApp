using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IFriendService
    {
        Task SendRequest(int userId, int friendId);
        Task RespondToRequest(int requestId, bool accept);
        Task<List<User>> GetFriends(int userId);
        Task<List<(int RequestId, int RequesterId, string UserName)>> GetPendingRequests(int userId);
        Task<bool> HasPendingRequest(int currentUserId, int friendId);
        Task<bool> AreFriends(int user1Id, int user2Id);
        Task RemoveFriend(int userId, int friendId);
        Task<int> GetFriendCount(int userId);
        Task<bool> IsFriendRequestPending(int userId, int friendId);
    }
}
