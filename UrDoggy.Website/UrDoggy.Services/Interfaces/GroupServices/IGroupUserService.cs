using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces.GroupServices
{
    public interface IGroupUserService
    {
        Task<Post> CreatePost(Post post, IEnumerable<(string path, string mediaType)> media);
        Task<bool> DeletePost(int postId, int? modId = null);
        Task<List<Post>> GetAllPost(int? groupId);
        Task<bool> JoinGroup(int userId, int groupId);
        Task<bool> LeaveGroup(int userId, int groupId);
    }
}
