using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;
using UrDoggy.Data.Repositories;
using UrDoggy.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;

namespace UrDoggy.Services.Service
{
    public class MediaService : IMediaService
    {
        private readonly MediaRepository _mediaRepository;
        private const int MaxWidth = 1200;
        private const int ThumbnailSize = 400;
        private const int JpegQuality = 80;

        public MediaService(MediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }

        public async Task AddMediaItems(int postId, IEnumerable<(string path, string mediaType)> mediaItems)
        {
            await _mediaRepository.AddMediatems(postId, mediaItems);
        }

        public async Task<List<Media>> GetMediaByPostId(int postId)
        {
            return await _mediaRepository.GetMediaByPostId(postId);
        }

        public async Task DeleteMediaByPostId(int postId)
        {
            await _mediaRepository.DeleteMediaByPostId(postId);
        }

        public async Task DeleteMediaById(int postId, int mediaId)
        {
            await _mediaRepository.DeleteMediaByPostIdAndMeidaId(postId, mediaId);
        }

        public async Task<List<Media>> GetAllMediaItems()
        {
            return await _mediaRepository.GetAllMediaItems();
        }

        public async Task EditMediaByPostId(int postId, IEnumerable<(int mediaId, string path, string mediaType)> mediaItems)
        {
            await _mediaRepository.EditMediaByPostId(postId, mediaItems);
        }

        public async Task<string> SaveMedia(IFormFile file)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // Nếu là ảnh → resize + nén
            if (file.ContentType.StartsWith("image/"))
            {
                return await SaveResizedImageAsync(file, uploadsPath);
            }

            // Nếu là video → chỉ lưu bình thường (sau này sẽ nén)
            if (file.ContentType.StartsWith("video/"))
            {
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/{fileName}";
            }

            throw new NotSupportedException("Chỉ hỗ trợ ảnh và video");
        }

        public async Task DeleteMediaFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            // Chuyển từ URL path sang physical path
            var relativePath = filePath.TrimStart('/');
            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            await Task.CompletedTask;
        }

        public async Task<List<string>> SaveMultipleMedia(IEnumerable<IFormFile> files)
        {
            var savedPaths = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var path = await SaveMedia(file);
                    savedPaths.Add(path);
                }
            }

            return savedPaths;
        }

        public async Task<string> GetMediaType(IFormFile file)
        {
            if (file == null)
                return "unknown";

            if (file.ContentType.StartsWith("image/"))
                return "image";
            else if (file.ContentType.StartsWith("video/"))
                return "video";
            else
                return "unknown";
        }

        public async Task<bool> IsValidMediaFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Kiểm tra kích thước file
            const long MaxFileSize = 2L * 1024 * 1024 * 1024;

            if (file.Length > MaxFileSize)
                return false;

            // Kiểm tra loại file
            var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            var validVideoTypes = new[] { "video/mp4", "video/quicktime", "video/x-msvideo", "video/webm", "video/x-matroska" };

            return validImageTypes.Contains(file.ContentType) || validVideoTypes.Contains(file.ContentType);
        }

        public async Task<string> SaveResizedImageAsync(IFormFile file, string uploadsFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var originalPath = Path.Combine(uploadsFolder, fileName);

            
            Directory.CreateDirectory(uploadsFolder);

            using var image = await Image.LoadAsync(file.OpenReadStream());

            // Resize ảnh lớn
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(MaxWidth, 0),
                Mode = ResizeMode.Max
            }));

            // Lưu ảnh chính (đã resize + nén)
            var encoder = new JpegEncoder { Quality = JpegQuality };
            await image.SaveAsync(originalPath, encoder);

            // Tạo thumbnail (tùy chọn - rất nên làm)
            var thumbFileName = $"thumb_{fileName}";
            var thumbPath = Path.Combine(uploadsFolder, thumbFileName);

            image.Mutate(x => x.Resize(ThumbnailSize, ThumbnailSize, true));
            await image.SaveAsWebpAsync(thumbPath);

            return $"/uploads/{fileName}";
        }

        public string GetThumbnailPath(string originalPath)
        {
            if (string.IsNullOrEmpty(originalPath)) return "/images/default-avatar.png";
            var fileName = Path.GetFileName(originalPath);
            return $"/uploads/thumb_{fileName}";
        }
    }
}
