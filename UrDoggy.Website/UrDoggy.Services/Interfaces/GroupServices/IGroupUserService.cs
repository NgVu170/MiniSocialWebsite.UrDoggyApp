using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Data.Repositories.Group_Repository;

namespace UrDoggy.Services.Interfaces.GroupServices
{
    public interface IGroupUserService
    {
        //Service for querrying group and post information for normal users
        Task<List<Post>> GetAllGroup();
        Task<List<Post>> GetAllPost(int? groupId);
        Task<List<GroupDetail>> GetAllMemberInGroup( int groupId);
        Task<List<GroupDetail>> GetAllGroupOfUser(int userId);
        //Service for joining and leaving groups
        Task<bool> JoinGroup(int userId, int groupId);
        Task<bool> LeaveGroup(int userId, int groupId);
        //Service for action
        Task<Post> CreatePost(Post post, IEnumerable<(string path, string mediaType)> media);
        Task<bool> DeletePost(int postId, int? modId = null);
        Task<bool> ReportPort(int postId, int userId, string reason);
    }
}
