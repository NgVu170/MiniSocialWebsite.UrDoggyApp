using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Data;
using UrDoggy.Services.Interfaces.GroupServices;

namespace UrDoggy.Services.Service.GroupServices
{
    public class GroupUserService : IGroupUserService
    {
        private readonly ApplicationDbContext _context;

        public GroupUserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================== GROUP QUERY ======================

        public async Task<List<Group>> GetAllGroup()
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.GroupStatus == Status.Active)
                .ToListAsync();
        }

        public async Task<Group?> getGroupById(int groupId)
        {
            return await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<Group?> GetGroupByIdWithOwner(int groupId)
        {
            return await _context.Groups
                .Include(g => g.Owner)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<List<GroupDetail>> GetAllMemberInGroup(int groupId)
        {
            return await _context.GroupDetails
                .Include(gd => gd.User)
                .Where(gd => gd.GroupId == groupId && gd.MemberStatus == MemberStatus.Active)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<GroupDetail>> GetAllGroupOfUser(int userId)
        {
            return await _context.GroupDetails
                .Include(gd => gd.Group)
                .Where(gd => gd.UserId == userId && gd.MemberStatus == MemberStatus.Active)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<GroupDetail>> GetModerators(int groupId){
            return await _context.GroupDetails
                .Include(gd => gd.User)
                .Where(gd => gd.GroupId == groupId &&
                             (gd.Role == GroupRole.Moderator || gd.Role == GroupRole.Admin) &&
                             gd.MemberStatus == MemberStatus.Active)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<HashSet<int>> GetModeratorGroupIds(int userId)
        {
            return await _context.GroupDetails
                .Where(gd => gd.UserId == userId &&
                             (gd.Role == GroupRole.Moderator || gd.Role == GroupRole.Admin) &&
                             gd.MemberStatus == MemberStatus.Active)
                .Select(gd => gd.GroupId)
                .ToHashSetAsync();
        }

        public async Task<List<GroupDetail>> GetUserGroupDetails(int userId)
        {
            return await _context.GroupDetails
                .Include(gd => gd.Group)
                .Where(gd => gd.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        // ====================== ROLE & PERMISSION ======================

        public async Task<GroupRole> getRole(int userId, int groupId)
        {
            var member = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);

            if (member == null)
                throw new Exception("User is not in this group");

            return member.Role;
        }

        public async Task<bool> IsModeratorOrAdmin(int userId, int groupId)
        {
            var role = await getRole(userId, groupId);
            return role == GroupRole.Admin || role == GroupRole.Moderator;
        }

        public async Task<bool> IsAdmin(int userId, int groupId)
        {
            var role = await getRole(userId, groupId);
            return role == GroupRole.Admin;
        }

        public async Task<bool> IsActiveMember(int userId, int groupId)
        {
            return await _context.GroupDetails.AnyAsync(g =>
                g.UserId == userId &&
                g.GroupId == groupId &&
                g.MemberStatus == MemberStatus.Active);
        }

        // ====================== POSTS ======================

        public async Task<List<Post>> GetAllPost(int groupId)
        {
            return await _context.Posts
                .Where(p => p.GroupId == groupId)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Post?> GetPostById(int postId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.MediaItems)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        // ✅ NEW: CREATE PENDING POST (REPLACES OLD CreatePost)
        public async Task CreatePendingPostAsync(GroupPostStatus post)
        {
            post.UploaddAt = DateTime.UtcNow;
            post.Status = StateOfPost.Pending;
            post.StatusUpdate = DateTime.UtcNow;

            _context.GroupPostStatuses.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePost(int postId, int? moderatorId = null)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return;

            post.Content = "[This post has been removed by moderator]";
            await _context.SaveChangesAsync();

            if (moderatorId.HasValue)
            {
                var status = await _context.GroupPostStatuses
                    .FirstOrDefaultAsync(s => s.PostId == postId);

                if (status != null)
                {
                    status.Status = StateOfPost.Removed;
                    status.StatusUpdate = DateTime.UtcNow;
                    status.ModId = moderatorId;
                    await _context.SaveChangesAsync();
                }
            }
        }

        // ====================== MEMBERSHIP ======================

        public async Task<bool> JoinGroup(int userId, int groupId)
        {
            var existing = await _context.GroupDetails
                .FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);

            if (existing != null)
                return false;

            _context.GroupDetails.Add(new GroupDetail
            {
                UserId = userId,
                GroupId = groupId,
                Role = GroupRole.Member,
                MemberStatus = MemberStatus.Active,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LeaveGroup(int userId, int groupId)
        {
            var member = await _context.GroupDetails
                .FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);

            if (member == null)
                return false;

            member.MemberStatus = MemberStatus.Leaved;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ====================== REPORT ======================

        public async Task<bool> ReportPost(int userId, int postId, string reason)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == postId && p.GroupId.HasValue);

            if (post == null)
                return false;

            _context.GroupReports.Add(new GroupReport
            {
                GroupPostId = postId,
                ReporterId = userId,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
