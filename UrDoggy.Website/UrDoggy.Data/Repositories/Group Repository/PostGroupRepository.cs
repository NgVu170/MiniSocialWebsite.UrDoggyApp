using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.Group_Models;
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
        //======================= NORMAL USER =======================
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
            var status = new GroupPostStatus
            {
                GroupId = post.GroupId.Value,
                AuthorId = post.UserId,
                Content = post.Content,
                UploaddAt = DateTime.UtcNow,
                Status = StateOfPost.Pending,
                StatusUpdate = DateTime.UtcNow
            };
            _context.GroupPostStatuses.Add(status);
            await _context.SaveChangesAsync();

            foreach(var(path, mediaType) in media)
    {
                _context.Media.Add(new Media
                {
                    Path = path,
                    MediaType = mediaType,
                    GroupPostStatusId = status.Id
                });
            }
            await _context.SaveChangesAsync();
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
            _context.GroupReports.Add(new GroupReport
            {
                GroupPostId = postId,
                ReporterId = reporterId,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        //======================= MODERATOR, ADMIN =====================
        public async Task<List<Post>> GetAllPendingPost(int groupId)
        {
            var pendingPostIds = await _context.GroupPostStatuses
                .Where(s => s.GroupId == groupId && s.Status == StateOfPost.Pending)
                .Select(s => s.PostId)
                .ToListAsync();
            return await _context.Posts
                .Where(p => pendingPostIds.Contains(p.Id))
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Post> ApprovedPost(int statusId, int modId)
        {
            var status = await _context.GroupPostStatuses
                .Include(s => s.MediaItems)
                .FirstOrDefaultAsync(s => s.Id == statusId);

            if (status == null)
                throw new InvalidOperationException("Pending post not found.");

            var newPost = new Post
            {
                UserId = status.AuthorId,
                GroupId = status.GroupId,
                Content = status.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(newPost);
            await _context.SaveChangesAsync();

            foreach (var media in status.MediaItems)
            {
                _context.Media.Add(new Media
                {
                    Path = media.Path,
                    MediaType = media.MediaType,
                    PostId = newPost.Id
                });
            }

            status.Status = StateOfPost.Approved;
            status.StatusUpdate = DateTime.UtcNow;
            status.ModId = modId;

            await _context.SaveChangesAsync();
            return newPost;
        }
        public async Task DeniedPost(int statusId, int modId)
        {
            var status = await _context.GroupPostStatuses
        .Include(s => s.MediaItems)
        .FirstOrDefaultAsync(s => s.Id == statusId);

            if (status == null)
                throw new InvalidOperationException("Pending post not found.");

            _context.Media.RemoveRange(status.MediaItems);
            _context.GroupPostStatuses.Remove(status);
            await _context.SaveChangesAsync();
        }
        public async Task<Post> ChangeStatus(int postId, int modId, StateOfPost statusOfPost)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && p.GroupId.HasValue);
            if (post == null)
            {
                throw new ArgumentException("Post not found or is not a group post.");
            }
            var status = await _context.GroupPostStatuses
                .FirstOrDefaultAsync(s => s.PostId == postId);
            if (status == null)
            {
                throw new InvalidOperationException("Post status not found.");
            }
            status.Status = statusOfPost;
            status.StatusUpdate = DateTime.UtcNow;
            status.ModId = modId;
            await _context.SaveChangesAsync();
            return post;
        }
    }
}
