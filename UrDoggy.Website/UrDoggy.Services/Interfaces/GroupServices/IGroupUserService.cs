using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Services.Interfaces.GroupServices
{
    public interface IGroupUserService
    {
        // ====================== GROUP QUERY ======================

        Task<List<Group>> GetAllGroup();
        Task<Group?> getGroupById(int groupId);
        Task<Group?> GetGroupByIdWithOwner(int groupId);

        Task<List<GroupDetail>> GetAllMemberInGroup(int groupId);
        Task<List<GroupDetail>> GetAllGroupOfUser(int userId);
        Task<List<GroupDetail>> GetUserGroupDetails(int userId);
        Task<HashSet<int>> GetModeratorGroupIds(int userId);
        Task<List<GroupDetail>> GetModerators(int groupId);

        // ====================== ROLE & PERMISSION ======================

        Task<GroupRole> getRole(int userId, int groupId);
        Task<bool> IsModeratorOrAdmin(int userId, int groupId);
        Task<bool> IsAdmin(int userId, int groupId);
        Task<bool> IsActiveMember(int userId, int groupId);

        // ====================== POSTS ======================

        Task<List<Post>> GetAllPost(int groupId);
        Task<Post?> GetPostById(int postId);

        Task CreatePendingPostAsync(GroupPostStatus post);
        Task DeletePost(int postId, int? moderatorId = null);

        // ====================== MEMBERSHIP ======================

        Task<bool> JoinGroup(int userId, int groupId);
        Task<bool> LeaveGroup(int userId, int groupId);

        // ====================== REPORT ======================
        Task<bool> ReportPost(int userId, int postId, string reason);
    }
}
