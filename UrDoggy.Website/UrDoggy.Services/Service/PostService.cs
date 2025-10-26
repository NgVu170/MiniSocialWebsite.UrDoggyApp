﻿using Microsoft.AspNetCore.Http;
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

        public async Task<List<Post>> GetNewsfeed() => await _postRepository.GetAllPost();

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

        public async Task<List<Post>> GetUserPosts(int userId)
        {
            var allPosts = await _postRepository.GetAllPost();
            return allPosts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
        }

        public async Task<int> GetPostCount(int userId)
        {
            var allPosts = await _postRepository.GetAllPost();
            return allPosts.Count(p => p.UserId == userId);
        }

        public async Task<int> GetTotalPostCount(int userId)
        {
            var friends = await _friendRepository.GetFriends(userId);
            var friendIds = friends.Select(f => f.Id).ToList();
            friendIds.Add(userId);

            return await _postRepository.GetTotalPostCount(friendIds);
        }

        public async Task<List<Post>> GetRecommendedPosts(int userId, HashSet<int> excludedPostIds = null, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Lấy tất cả bài viết (trừ của chính user)
                var allPosts = await _postRepository.GetAllPost();

                if (allPosts == null || !allPosts.Any())
                    return new List<Post>();

                var filteredPosts = allPosts.Where(p => p.UserId != userId).ToList();

                // Loại bỏ posts đã xem nếu có
                if (excludedPostIds != null && excludedPostIds.Any())
                {
                    filteredPosts = filteredPosts.Where(p => !excludedPostIds.Contains(p.Id)).ToList();
                }
                if (!filteredPosts.Any())
                    return new List<Post>();

                // Tính điểm recommendation cho mỗi bài viết
                var postScores = new List<(Post Post, float Score)>();

                foreach (var post in filteredPosts)
                {
                    var score = await _postRepository.RankCalculate(userId, post);
                    if (score > float.MinValue) // Loại bỏ bài viết bị rejected
                    {
                        postScores.Add((post, score));
                    }
                }

                // Sắp xếp theo điểm giảm dần và phân trang
                var recommendedPosts = postScores
                    .OrderByDescending(x => x.Score)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.Post)
                    .ToList();

                return recommendedPosts;
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về danh sách rỗng
                Console.WriteLine($"Error in GetRecommendedPosts: {ex.Message}");
                return new List<Post>();
            }
        }
    }
}
