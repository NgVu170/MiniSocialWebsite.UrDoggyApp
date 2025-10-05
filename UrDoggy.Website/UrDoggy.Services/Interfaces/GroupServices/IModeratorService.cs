using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Services.Interfaces.GroupServices
{
    public interface IModeratorService : IGroupUserService
    {
        Task<bool> ApprovePost(int groupPostStatusId, int modId);
        Task<bool> RejectPost(int grouPostStatusId, int modId, string reason);
        Task<List<GroupPostStatus>> GetAllPendingPost(int groupId);
        Task<List<Core.Models.Group_Models.GroupReport>> GetAllReportPost(int groupId);
        Task<bool> DeletePost(int postId, int modId);
        Task<bool> BanUser(int userId, int groupId, int modId, string reason);
        Task<bool> UnbanUser(int userId, int groupId, int modId);
        Task<bool> KickUser(int userId, int groupId, int modId);
    }
}
