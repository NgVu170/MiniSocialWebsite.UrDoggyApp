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
        Task AddMediaItemsAsync(int postId, IEnumerable<(string path, string mediaType)> mediaItems);
        Task<List<Media>> GetMediaByPostIdAsync(int postId);
        Task DeleteMediaByPostIdAsync(int postId);
        Task DeleteMediaByIdAsync(int postId, int mediaId);
        Task<List<Media>> GetAllMediaItemsAsync();
        Task EditMediaByPostIdAsync(int postId, IEnumerable<(int mediaId, string path, string mediaType)> mediaItems);
        Task<string> SaveMediaAsync(IFormFile file);
        Task DeleteMediaFileAsync(string filePath);
        Task<List<string>> SaveMultipleMediaAsync(IEnumerable<IFormFile> files);
        Task<string> GetMediaTypeAsync(IFormFile file);
        Task<bool> IsValidMediaFile(IFormFile file);
    }
}
