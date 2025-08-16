using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models; // Friend

namespace UrDoggy.Data.Repositories
{
    public class FriendRepository
    {
        private readonly ApplicationDbContext _context;
        public FriendRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendRequest(int UserId, int FriendId)
        {
            var friend = new Friend
            {
                UserId = UserId,
                FriendId = FriendId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.Friends.Add(friend);
            await _context.SaveChangesAsync();
        }

        public async Task RespondToRequest(int requestId, bool accept)
        {
            var friendRequest = await _context.Friends.FindAsync(requestId);
            if (friendRequest != null)
            {
                if (accept)
                {
                    friendRequest.Status = "Accepted";
                }
                else
                {
                    friendRequest.Status = "Rejected";
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetFriends(int currentUserId)
        {
            var friendsList = from f in _context.Friends
                              where f.Status == "Accepted" && (f.UserId == currentUserId || f.FriendId == currentUserId)
                              join u in _context.Users on (f.UserId == currentUserId ? f.FriendId : f.UserId) equals u.Id
                              select new User
                              {
                                  Id = u.Id,
                                  UserName = u.UserName,
                                  ProfilePicture = u.ProfilePicture,
                                  DisplayName = u.DisplayName,
                                  Email = u.Email,
                              };

            return await friendsList.AsNoTracking().Distinct().ToListAsync();
        }

        public async Task<List<(int RequestId, int RequesterId, string UserName)>> GetPendingRequest(int CurrentUserId)
        {
            return await _context.Friends
                .Where(f => f.FriendId == CurrentUserId && f.Status == "Pending")
                .Join(_context.Users, f => f.UserId, u => u.Id, (f, u) => new { f.Id, f.UserId, u.UserName })
                .Select(x => new ValueTuple<int, int, string>(x.Id, x.UserId, x.UserName!))
                .ToListAsync();
        }

        public async Task<bool> HasPendingRequest(int currentUserId, int friendId)
        {
            return await _context.Friends.AnyAsync(f =>
                f.UserId == currentUserId && f.FriendId == friendId && f.Status == "Pending");
        }

        public async Task<bool> AreFriends(int userId, int friendId)
        {
            return await _context.Friends.AnyAsync(f =>
                (f.UserId == userId && f.FriendId == friendId || f.UserId == friendId && f.FriendId == userId) &&
                f.Status == "Accepted");
        }

        public async Task RemoveFriend(int userId, int friendId)
        {
            var friend = await _context.Friends.FirstOrDefaultAsync(f =>
                (f.UserId == userId && f.FriendId == friendId) || (f.UserId == friendId && f.FriendId == userId) && f.Status == "Accepted");
            if (friend != null)
            {
                _context.Friends.Remove(friend);
                await _context.SaveChangesAsync();
            }
        }
    }
}
