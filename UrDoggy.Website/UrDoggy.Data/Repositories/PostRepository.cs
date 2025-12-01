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
                .Include(p => p.MediaItems)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .OrderByDescending(p => p.CreatedAt)
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

        // Phương thức tính điểm dựa trên tags và sở thích người dùng
        public async Task<float> CalculatePersonalizedScore(int currentUserId, Post post)
        {
            float score = await RankCalculate(currentUserId, post);

            if (score == float.MinValue) return float.MinValue;

            // Điểm từ tags phổ biến của user
            float tagScore = await CalculateTagSimilarityScore(currentUserId, post);
            score += tagScore;

            return score;
        }

        // Tính điểm tương đồng dựa trên tags
        private async Task<float> CalculateTagSimilarityScore(int currentUserId, Post post)
        {
            // Lấy tags phổ biến nhất của user (dựa trên posts đã tương tác)
            var userFavoriteTags = await GetUserFavoriteTags(currentUserId);

            // Lấy tags của post hiện tại
            var postTags = await _context.PostTags
                .Where(pt => pt.PostId == post.Id)
                .Select(pt => pt.Tag.Name)
                .ToListAsync();

            if (!userFavoriteTags.Any() || !postTags.Any())
                return 0;

            // Tính điểm dựa trên số tags trùng khớp
            int matchingTags = postTags.Count(tag => userFavoriteTags.Contains(tag));
            return matchingTags * 3.0f; // Mỗi tag trùng khớp +3 điểm
        }

        // Lấy tags phổ biến của user
        private async Task<HashSet<string>> GetUserFavoriteTags(int userId)
        {
            // Lấy tất cả posts mà user đã upvote
            var upvotedPosts = await _context.PostVotes
                .Where(v => v.UserId == userId && v.IsUpvote)
                .Select(v => v.PostId)
                .ToListAsync();

            // Lấy tags từ các posts đã upvote
            var favoriteTags = await _context.PostTags
                .Where(pt => upvotedPosts.Contains(pt.PostId))
                .GroupBy(pt => pt.Tag.Name)
                .OrderByDescending(g => g.Count())
                .Take(5) // Lấy 5 tags phổ biến nhất
                .Select(g => g.Key)
                .ToListAsync();

            return favoriteTags.ToHashSet();
        }

        // Phương thức để extract tags từ content
        public List<string> ExtractTagsFromContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return new List<string>();

            // Tìm tất cả hashtags trong content
            var hashtagPattern = @"#\w+";
            var matches = System.Text.RegularExpressions.Regex.Matches(content, hashtagPattern);

            return matches
                .Select(m => m.Value.TrimStart('#').ToLower())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .ToList();
        }

        public async Task<List<string>> GetPostTags(int postId)
        {
            return await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .Select(pt => pt.Tag.Name)
                .ToListAsync();
        }

        public async Task<Tag> GetOrCreateTagAsync(string tagName)
        {
            tagName = tagName.Trim().TrimStart('#').ToLower();

            if (string.IsNullOrWhiteSpace(tagName))
                return null;

            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name == tagName);

            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            return tag;
        }

        public async Task AddTagsToPostAsync(int postId, IEnumerable<string> tagNames)
        {
            if (!tagNames.Any()) return;

            var uniqueTags = tagNames.Select(t => t.ToLower().Trim()).Distinct();

            foreach (var tagName in uniqueTags)
            {
                var tag = await GetOrCreateTagAsync(tagName);

                var exists = await _context.PostTags
                    .AnyAsync(pt => pt.PostId == postId && pt.TagId == tag.Id);

                if (!exists)
                {
                    _context.PostTags.Add(new PostTag
                    {
                        PostId = postId,
                        TagId = tag.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveAllTagsFromPostAsync(int postId)
        {
            var postTags = await _context.PostTags
                .Where(pt => pt.PostId == postId)
                .ToListAsync();

            if (postTags.Any())
            {
                _context.PostTags.RemoveRange(postTags);
                await _context.SaveChangesAsync();
            }
        }
    }
}
