using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models;

namespace UrDoggy.Data.Repositories
{
    public class PostRepository
    {
        private readonly ApplicationDbContext _context;
        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ: feed
        public async Task<List<Post>> GetAllPost()
        {
            return await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        // READ: detail (with media, comments, tags)
        public async Task<Post?> GetById(int postId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Include(p => p.MediaItems)
                .Include(p => p.Comments)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        // CREATE: create post + optional media list
        public async Task CreatePost(Post post, IEnumerable<(string path, string mediaType)> media)
        {
            if (post.CreatedAt == default) post.CreatedAt = DateTime.UtcNow;

            // create post first to get Id
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // add media rows (if provided)
            if (media != null)
            {
                var rows = media
                    .Where(m => !string.IsNullOrWhiteSpace(m.path) && !string.IsNullOrWhiteSpace(m.mediaType))
                    .Select(m => new Media
                    {
                        PostId = post.Id,
                        Path = m.path,
                        MediaType = m.mediaType,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                if (rows.Count > 0)
                {
                    await _context.Media.AddRangeAsync(rows);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // UPDATE: update content; if post.MediaItems is provided, sync Media table to match it
        public async Task UpdatePost(Post post)
        {
            var existing = await _context.Posts
                .Include(p => p.MediaItems)
                .FirstOrDefaultAsync(p => p.Id == post.Id);

            if (existing == null) return;

            // update scalar fields
            if (post.Content != null)
                existing.Content = post.Content;

            // Sync media only if caller sent a collection (treat as desired final state)
            if (post.MediaItems != null)
            {
                // helper to build a composite key for equality (distinct names to avoid clashes)
                string KeyFromMedia(Media m) => $"{m.Path}||{m.MediaType}";
                string KeyFromValues(string path, string type) => $"{path}||{type}";

                // current media in DB
                var currentDict = existing.MediaItems
                    .ToDictionary(m => KeyFromMedia(m), m => m, StringComparer.OrdinalIgnoreCase);

                // desired media from caller
                var desiredKeys = post.MediaItems
                    .Where(m => !string.IsNullOrWhiteSpace(m.Path) && !string.IsNullOrWhiteSpace(m.MediaType))
                    .Select(m => KeyFromValues(m.Path, m.MediaType))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // remove medias not desired anymore
                var toRemove = existing.MediaItems
                    .Where(m => !desiredKeys.Contains(KeyFromMedia(m)))
                    .ToList();
                if (toRemove.Count > 0)
                    _context.Media.RemoveRange(toRemove);

                // add new medias missing in DB
                var toAdd = desiredKeys
                    .Where(k => !currentDict.ContainsKey(k))
                    .Select(k =>
                    {
                        var parts = k.Split(new[] { "||" }, StringSplitOptions.None);
                        var path = parts.Length > 0 ? parts[0] : string.Empty;
                        var type = parts.Length > 1 ? parts[1] : string.Empty;

                        return new Media
                        {
                            PostId = existing.Id,
                            Path = path,
                            MediaType = type,
                            CreatedAt = DateTime.UtcNow
                        };
                    })
                    .ToList();

                if (toAdd.Count > 0)
                    await _context.Media.AddRangeAsync(toAdd);
            }

            await _context.SaveChangesAsync();
        }

        // DELETE: post (Media/Comments/etc cascade by FK)
        public async Task DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        // VOTE: first vote / toggle off / switch (all consistent via transaction)
        public async Task Vote(int postId, int UserId, bool isUpvote)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var vote = await _context.PostVotes
                .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == UserId);

            if (vote == null)
            {
                // first vote
                _context.PostVotes.Add(new PostVote
                {
                    PostId = postId,
                    UserId = UserId,
                    IsUpvote = isUpvote,
                    CreatedAt = DateTime.UtcNow
                });

                if (isUpvote)
                {
                    await _context.Posts.Where(p => p.Id == postId)
                        .ExecuteUpdateAsync(u => u.SetProperty(x => x.UpVotes, x => x.UpVotes + 1));
                }
                else
                {
                    await _context.Posts.Where(p => p.Id == postId)
                        .ExecuteUpdateAsync(u => u.SetProperty(x => x.DownVotes, x => x.DownVotes + 1));
                }
            }
            else if (vote.IsUpvote == isUpvote)
            {
                // same vote again -> remove (toggle off)
                _context.PostVotes.Remove(vote);

                if (isUpvote)
                {
                    await _context.Posts.Where(p => p.Id == postId)
                        .ExecuteUpdateAsync(u => u.SetProperty(x => x.UpVotes, x => x.UpVotes - 1));
                }
                else
                {
                    await _context.Posts.Where(p => p.Id == postId)
                        .ExecuteUpdateAsync(u => u.SetProperty(x => x.DownVotes, x => x.DownVotes - 1));
                }
            }
            else
            {
                // switch up<->down
                vote.IsUpvote = isUpvote;

                if (isUpvote)
                {
                    await _context.Posts.Where(p => p.Id == postId)
                        .ExecuteUpdateAsync(u => u
                            .SetProperty(x => x.UpVotes, x => x.UpVotes + 1)
                            .SetProperty(x => x.DownVotes, x => x.DownVotes - 1));
                }
                else
                {
                    await _context.Posts.Where(p => p.Id == postId)
                        .ExecuteUpdateAsync(u => u
                            .SetProperty(x => x.UpVotes, x => x.UpVotes - 1)
                            .SetProperty(x => x.DownVotes, x => x.DownVotes + 1));
                }
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }

        // ===== REPORT =====
        public async Task ReportPost(int postId, int reporterId, string reason)
        {
            _context.Reports.Add(new Report
            {
                PostId = postId,
                ReporterId = reporterId,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        // ===== READ: vote count =====
        public async Task<int> GetVoteCount(int postId)
        {
            var row = await _context.Posts
                .AsNoTracking()
                .Where(p => p.Id == postId)
                .Select(p => new { p.UpVotes, p.DownVotes })
                .FirstOrDefaultAsync();

            return row == null ? 0 : row.UpVotes - row.DownVotes;
        }

        // ===== READ: has voted =====
        public async Task<bool> HasVoted(int postId, int userId)
        {
            return await _context.PostVotes
                .AsNoTracking()
                .AnyAsync(v => v.PostId == postId && v.UserId == userId);
        }

        public async Task SharePost(int postId, int userId)
        {
            // If you later model a PostShare entity, replace with _context.PostShares.Add(...) + SaveChangesAsync()
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO PostShares (PostId, UserId) VALUES ({0}, {1})",
                postId, userId);
        }

        public async Task<int> GetTotalPostCount(List<int> userIds)
        {
            return await _context.Posts
                .AsNoTracking()
                .CountAsync(p => userIds.Contains(p.UserId));
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
            }
            else
            {
                result += 1; // Không có quan hệ
            }
            return result += (Post.UpVotes - Post.DownVotes) * 0.1f; // Điểm từ lượt vote
        }
    }
}
