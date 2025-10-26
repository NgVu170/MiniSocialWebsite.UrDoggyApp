using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IMediaService
    {
        Task AddMediaItems(int postId, IEnumerable<(string path, string mediaType)> mediaItems);
        Task<List<Media>> GetMediaByPostId(int postId);
        Task DeleteMediaByPostId(int postId);
        Task DeleteMediaById(int postId, int mediaId);
        Task<List<Media>> GetAllMediaItems();
        Task EditMediaByPostId(int postId, IEnumerable<(int mediaId, string path, string mediaType)> mediaItems);
        Task<string> SaveMedia(IFormFile file);
        Task DeleteMediaFile(string filePath);
        Task<List<string>> SaveMultipleMedia(IEnumerable<IFormFile> files);
        Task<string> GetMediaType(IFormFile file);
        Task<bool> IsValidMediaFile(IFormFile file);
    }
}
