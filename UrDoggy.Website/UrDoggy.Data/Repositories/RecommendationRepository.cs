using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Data.Repositories
{
    public class RecommendationRepository
    {
        private readonly ApplicationDbContext _context;
        public RecommendationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<float> RankCalculate(int currentUserId, Post Post)
        {
            float result = 0;
            var relationship = _context.Friends.Where(r =>
                (r.UserId == currentUserId && r.FriendId == Post.UserId) 
                || (r.UserId == Post.UserId && r.FriendId == currentUserId))
                .FirstOrDefault();

            if (relationship != null)
            {
                if (relationship.Status == "Accepted")
                {
                    result += 5; // Bạn bè
                }
                else if (relationship.Status == "Pending")
                {
                    result += 2; // Đang chờ kết bạn
                }
                else if (relationship.Status == "Rejected")
                {
                    return result = float.MinValue; // Bị từ chối
                }
            } else
            {
                result += 1; // Không có quan hệ
            }
             return result += (Post.UpVotes - Post.DownVotes) * 0.1f; // Điểm từ lượt vote
        }
    }
}
