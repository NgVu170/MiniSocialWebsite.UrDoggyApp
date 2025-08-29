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
    public class PostService : IPostService
    {
        private readonly PostRepository _postRepository;
        private readonly MediaRepository _mediaRepository;
        private readonly UserRepository _userRepository;
        private readonly FriendRepository _friendRepository;
        private readonly IMediaService _mediaService;

        public PostService(PostRepository postRepository, MediaRepository mediaRepository,
                          UserRepository userRepository, FriendRepository friendRepository,
                          IMediaService mediaService)
        {
            _postRepository = postRepository;
            _mediaRepository = mediaRepository;
            _userRepository = userRepository;
            _friendRepository = friendRepository;
            _mediaService = mediaService;
        }

        public async Task<List<Post>> GetNewsfeed(int userId, int pageNumber = 1, int pageSize = 10)
        {
            // Lấy danh sách bạn bè
            var friends = await _friendRepository.GetFriends(userId);
            var friendIds = friends.Select(f => f.Id).ToList();
            friendIds.Add(userId); // Bao gồm cả bài viết của chính user

            // Lấy tất cả bài viết và lọc theo bạn bè
            var allPosts = await _postRepository.GetAllPost();
            var newsfeedPosts = allPosts
                .Where(p => friendIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return newsfeedPosts;
        }

        public async Task<Post> GetById(int postId)
        {
            return await _postRepository.GetById(postId);
        }

        public async Task<Post> CreatePost(int userId, string content, List<(string path, string mediaType)> mediaItems = null)
        {
            var post = new Post
            {
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpVotes = 0,
                DownVotes = 0
            };

            await _postRepository.CreatePost(post, mediaItems);
            return post;
        }

        public async Task EditPost(Post post)
        {
            await _postRepository.UpdatePost(post);
        }

        public async Task DeletePost(int postId)
        {
            await _postRepository.DeletePost(postId);
        }

        public async Task Vote(int postId, int userId, bool isUpvote)
        {
            await _postRepository.Vote(postId, userId, isUpvote);
        }

        public async Task<int> GetVoteCount(int postId)
        {
            return await _postRepository.GetVoteCount(postId);
        }

        public async Task<bool> HasVoted(int postId, int userId)
        {
            return await _postRepository.HasVoted(postId, userId);
        }

        public async Task ReportPost(int postId, int reporterId, string reason)
        {
            await _postRepository.ReportPost(postId, reporterId, reason);
        }

        public async Task SharePost(int postId, int userId)
        {
            await _postRepository.SharePost(postId, userId);
        }

        public async Task<List<Post>> GetUserPosts(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var allPosts = await _postRepository.GetAllPost();
            return allPosts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<int> GetPostCount(int userId)
        {
            var allPosts = await _postRepository.GetAllPost();
            return allPosts.Count(p => p.UserId == userId);
        }
    }
}
