using Microsoft.AspNetCore.Http;
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
    public class MediaService : IMediaService
    {
        private readonly MediaRepository _mediaRepository;

        public MediaService(MediaRepository mediaRepository)
        {
            _mediaRepository = mediaRepository;
        }

        public async Task AddMediaItemsAsync(int postId, IEnumerable<(string path, string mediaType)> mediaItems)
        {
            await _mediaRepository.AddMediatems(postId, mediaItems);
        }

        public async Task<List<Media>> GetMediaByPostIdAsync(int postId)
        {
            return await _mediaRepository.GetMediaByPostId(postId);
        }

        public async Task DeleteMediaByPostIdAsync(int postId)
        {
            await _mediaRepository.DeleteMediaByPostId(postId);
        }

        public async Task DeleteMediaByIdAsync(int postId, int mediaId)
        {
            await _mediaRepository.DeleteMediaByPostIdAndMeidaId(postId, mediaId);
        }

        public async Task<List<Media>> GetAllMediaItemsAsync()
        {
            return await _mediaRepository.GetAllMediaItems();
        }

        public async Task EditMediaByPostIdAsync(int postId, IEnumerable<(int mediaId, string path, string mediaType)> mediaItems)
        {
            await _mediaRepository.EditMediaByPostId(postId, mediaItems);
        }

        public async Task<string> SaveMediaAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Tạo tên file unique
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn URL
            return $"/uploads/{fileName}";
        }

        public async Task DeleteMediaFileAsync(string filePath)
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

        public async Task<List<string>> SaveMultipleMediaAsync(IEnumerable<IFormFile> files)
        {
            var savedPaths = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var path = await SaveMediaAsync(file);
                    savedPaths.Add(path);
                }
            }

            return savedPaths;
        }

        public async Task<string> GetMediaTypeAsync(IFormFile file)
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

            // Kiểm tra kích thước file (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return false;

            // Kiểm tra loại file
            var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            var validVideoTypes = new[] { "video/mp4", "video/quicktime", "video/x-msvideo" };

            return validImageTypes.Contains(file.ContentType) || validVideoTypes.Contains(file.ContentType);
        }
    }
}
