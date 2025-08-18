using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models; // Meida

namespace UrDoggy.Data.Repositories
{
    internal class MediaRepository
    {
        private readonly ApplicationDbContext _context;
        public MediaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMediatems(int PostId, IEnumerable<(string path, string mediaType)> mediaItems)
        {
            var findPost = await _context.Posts.FindAsync(PostId);
            if (findPost != null)
            {
                if (findPost.MediaItems == null)
                {
                    findPost.MediaItems = new List<Media>();
                    foreach (var item in mediaItems)
                    {
                        if (!string.IsNullOrWhiteSpace(item.path) && !string.IsNullOrWhiteSpace(item.mediaType))
                        {
                            findPost.MediaItems.Add(new Media
                            {
                                PostId = PostId,
                                Path = item.path,
                                MediaType = item.mediaType,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                    _context.Posts.Update(findPost);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Media>> GetMediaByPostId(int postId)
        {
            return await _context.Media
                .AsNoTracking()
                .Where(m => m.PostId == postId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteMediaByPostId(int postId)
        {
            var mediaItems = await _context.Media
                .Where(m => m.PostId == postId)
                .ToListAsync();
            if (mediaItems.Any())
            {
                _context.Media.RemoveRange(mediaItems);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteMediaByPostIdAndMeidaId(int postId, int mediaId)
        {
            var mediaItems = await _context.Media
                .Where(m => m.PostId == postId)
                .ToListAsync();
            if (mediaItems.Any())
            {
                foreach(var item in mediaItems)
                {
                    if (item.Id == mediaId)
                    {
                        _context.Media.Remove(item);
                        await _context.SaveChangesAsync();
                        break; // chỉ xoá 1 item
                    }
                }
            }
        }

        public async Task<List<Media>> GetAllMediaItems()
        {
            return await _context.Media
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task EditMediaByPostId(int postId, IEnumerable<(int mediaId, string path, string mediaType)> mediaItems)
        {
            var findPost = await _context.Posts.FindAsync(postId);
            if (findPost != null)
            {
                if (findPost.MediaItems == null)
                {
                    findPost.MediaItems = new List<Media>();
                }
                foreach (var item in mediaItems)
                {
                    var existingMedia = findPost.MediaItems.FirstOrDefault(m => m.Id == item.mediaId);
                    if (existingMedia != null)
                    {
                        existingMedia.Path = item.path;
                        existingMedia.MediaType = item.mediaType;
                    }
                    else
                    {
                        findPost.MediaItems.Add(new Media
                        {
                            PostId = postId,
                            Path = item.path,
                            MediaType = item.mediaType,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                _context.Posts.Update(findPost);
                await _context.SaveChangesAsync();
            }
        }
    }
}
