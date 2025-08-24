using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;
using UrDoggy.Data.Repositories;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Services.Service
{
    public class FriendService : IFriendService
    {
        private readonly FriendRepository _friendRepository;
        private readonly UserRepository _userRepository;

        public FriendService(FriendRepository friendRepository, UserRepository userRepository)
        {
            _friendRepository = friendRepository;
            _userRepository = userRepository;
        }

        public async Task SendRequest(int userId, int friendId)
        {
            // Check if already friends or request pending
            if (await AreFriends(userId, friendId) || await HasPendingRequest(userId, friendId))
            {
                throw new InvalidOperationException("Đã gửi lời mời kết bạn hoặc đã là bạn bè");
            }

            await _friendRepository.SendRequest(userId, friendId);
        }

        public async Task RespondToRequest(int requestId, bool accept)
        {
            await _friendRepository.RespondToRequest(requestId, accept);
        }

        public async Task<List<User>> GetFriends(int userId)
        {
            return await _friendRepository.GetFriends(userId);
        }

        public async Task<List<(int RequestId, int RequesterId, string UserName)>> GetPendingRequests(int userId)
        {
            return await _friendRepository.GetPendingRequest(userId);
        }

        public async Task<bool> HasPendingRequest(int currentUserId, int friendId)
        {
            return await _friendRepository.HasPendingRequest(currentUserId, friendId);
        }

        public async Task<bool> AreFriends(int user1Id, int user2Id)
        {
            return await _friendRepository.AreFriends(user1Id, user2Id);
        }

        public async Task RemoveFriend(int userId, int friendId)
        {
            await _friendRepository.RemoveFriend(userId, friendId);
        }

        public async Task<int> GetFriendCount(int userId)
        {
            var friends = await _friendRepository.GetFriends(userId);
            return friends.Count;
        }

        public async Task<bool> IsFriendRequestPending(int userId, int friendId)
        {
            var pendingRequests = await _friendRepository.GetPendingRequest(userId);
            return pendingRequests.Any(r => r.RequesterId == friendId);
        }
    }

}
