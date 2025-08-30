using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface ICommentService
    {
        Task<Comment> AddComment(int postId, int userId, string content);
        Task<List<Comment>> GetComments(int postId);
        Task DeleteComment(int commentId);
        Task UpdateComment(Comment comment);
        Task<Comment> GetCommentById(int commentId);
        Task<int> GetCommentCount(int postId);
        Task<bool> IsCommentOwner(int commentId, int userId);
    }
}
