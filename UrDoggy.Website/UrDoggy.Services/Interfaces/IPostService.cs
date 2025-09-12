using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IPostService
    {
        Task<List<Post>> GetNewsfeed(int userId, int pageNumber = 1, int pageSize = 10);
        Task<Post> GetById(int postId);
        Task<Post> CreatePost(int userId, string content, List<(string path, string mediaType)> mediaItems = null);
        Task EditPost(Post post);
        Task DeletePost(int postId);
        Task Vote(int postId, int userId, bool isUpvote);
        Task<int> GetVoteCount(int postId);
        Task<bool> HasVoted(int postId, int userId);
        Task ReportPost(int postId, int reporterId, string reason);
        Task SharePost(int postId, int userId);
        Task<List<Post>> GetUserPosts(int userId, int pageNumber = 1, int pageSize = 10);
        Task<int> GetPostCount(int userId);
        Task<int> GetTotalPostCount(int userId);
    }
}
