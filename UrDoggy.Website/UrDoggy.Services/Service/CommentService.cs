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
    public class CommentService : ICommentService
    {
        private readonly CommentRepository _commentRepository;
        private readonly PostRepository _postRepository;
        private readonly UserRepository _userRepository;

        public CommentService(CommentRepository commentRepository, PostRepository postRepository, UserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<Comment> AddComment(int postId, int userId, string content)
        {
            // Verify post exists
            var post = await _postRepository.GetById(postId);
            if (post == null)
            {
                throw new ArgumentException("Bài viết không tồn tại");
            }

            // Verify user exists
            var user = await _userRepository.GetByI(userId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại");
            }

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddComment(comment);
            return comment;
        }

        public async Task<List<Comment>> GetComments(int postId)
        {
            return await _commentRepository.GetCommentsByPostId(postId);
        }

        public async Task DeleteComment(int commentId)
        {
            await _commentRepository.DeleteComment(commentId);
        }

        public async Task UpdateComment(Comment comment)
        {
            await _commentRepository.UpdateComment(comment);
        }

        public async Task<Comment> GetCommentById(int commentId)
        {
            var allComments = await _commentRepository.GetCommentsByPostId(0); // Get all then filter
            return allComments.FirstOrDefault(c => c.Id == commentId);
        }

        public async Task<int> GetCommentCount(int postId)
        {
            var comments = await _commentRepository.GetCommentsByPostId(postId);
            return comments.Count;
        }

        public async Task<bool> IsCommentOwner(int commentId, int userId)
        {
            var comment = await GetCommentById(commentId);
            return comment != null && comment.UserId == userId;
        }
    }
}
