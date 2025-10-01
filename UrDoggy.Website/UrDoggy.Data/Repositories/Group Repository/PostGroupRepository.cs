using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Data.Repositories.Group_Repository
{
    public class GroupPostRepository : PostRepository
    {
        private readonly ApplicationDbContext _context;
        public GroupPostRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<List<Post>> GetAllPost(int? groupId)
        {
            var querry = _context.Posts.AsQueryable();
            if (groupId.HasValue)
            {
                querry = querry.Where(p => p.GroupId == groupId.Value);
            }
            return await querry
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task CreatePost(Post post, IEnumerable<(string path, string mediaType)> media)
        {
            if (!post.GroupId.HasValue)
            {
                throw new ArgumentException("GroupId must be provided for group posts.");
            }
            await base.CreatePost(post, media);

            var status = new GroupPostStatus
            {
                PostId = post.Id,
                GroupId = post.GroupId.Value,
                AuthorId = post.UserId,
                UploaddAt = DateTime.UtcNow,
                Status = StateOfPost.Pending,
                StatusUpdate = DateTime.UtcNow
            };
            _context.GroupPostStatuses.Add(status);
            await _context.SaveChangesAsync();
        }

        public override async Task UpdatePost(Post post)
        {
            if (!post.GroupId.HasValue)
            {
                throw new ArgumentException("GroupId must be provided for group posts.");
            }
            await base.UpdatePost(post);
        }

        public override async Task DeletePost(int postId, int? modId = null)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && p.GroupId.HasValue);

            if (post != null && modId.HasValue)
            {
                await _context.GroupPostStatuses
                    .Where(s => s.PostId == postId)
                    .ForEachAsync(s =>
                    {
                        s.Status = StateOfPost.Removed;
                        s.StatusUpdate = DateTime.UtcNow;
                        s.ModId = modId;
                    });
            }
            post.Content = "[This post has been removed by a moderator]";
            await _context.SaveChangesAsync();
        }

        public override async Task ReportPost(int postId, int reporterId, string reason)
        {
            throw new Exception("Reporting group posts is not implemented yet.");
        }


    }
}
