using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Data.Repositories
{
    public class CommentRepository
    {
        private readonly ApplicationDbContext _context;
        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Comment>> GetCommentsByPostId(int postId)
        {
            return Task.FromResult(_context.Comments.Where(c => c.PostId == postId).ToList());
        }

        public async Task AddComment(Comment comment)
        {
            _context.Comments.Add(comment);
           await _context.SaveChangesAsync();
        }

        public async Task UpdateComment(Comment comment)
        {
            var existingComment = await _context.Comments.FindAsync(comment.Id);
            if (existingComment != null)
            {
                existingComment.Content = comment.Content;
                existingComment.CreatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteComment(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

    }
}
