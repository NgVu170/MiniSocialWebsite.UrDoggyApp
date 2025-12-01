using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Data.Repositories.Group_Repository
{
    public class UserGroupRepositrory
    {
        private readonly ApplicationDbContext _context;
        public UserGroupRepositrory(ApplicationDbContext context)
        {
            _context = context;
        }

        //====================== NORMAL USER ========================
        public async Task<bool> JoinGroup(int userId, int groupId)
        {
            var findGroup = await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId && g.GroupStatus == Status.Active);
            if (findGroup == null)
            {
                throw new ArgumentException("Group not found or inactive.");
            }
            var existingMember = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);
            if ((existingMember != null))
                return false;
            var newMember = new GroupDetail
            {
                UserId = userId,
                GroupId = groupId,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
            };
            _context.GroupDetails.Add(newMember);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> LeaveGroup(int userId, int groupId)
        {
            var existingMember = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);
            if (existingMember == null)
                return false;
            existingMember.MemberStatus = MemberStatus.Leaved;
            existingMember.UpdatedAt = DateTime.UtcNow;
            _context.GroupDetails.Update(existingMember);
            await _context.SaveChangesAsync();
            return true;
        }
        //======================= MODERATOR =========================
        public async Task<bool> BanUser(int userId, int groupdId, int modId)
        {
            var findUser = await _context.GroupDetails
                .Where(g => g.UserId == userId && g.GroupId == groupdId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (findUser == null)
            {
                throw new ArgumentException("User not found in group.");
            } else
            {
                findUser.MemberStatus = MemberStatus.Banned;
                findUser.UpdatedAt = DateTime.UtcNow;
                _context.GroupDetails.Update(findUser);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        public async Task<bool> UnbanUser(int userId, int groupId, int modId)
        {
            var findUser = await _context.GroupDetails
                .Where(g => g.UserId == userId && g.GroupId == groupId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (findUser == null)
            {
                throw new ArgumentException("User not found in group.");
            }

            if (findUser.MemberStatus != MemberStatus.Banned)
            {
                throw new InvalidOperationException("User is not banned.");
            }

            // Cập nhật lại trạng thái
            findUser.MemberStatus = MemberStatus.Active; // hoặc Status.Member tuỳ enum của bạn
            findUser.UpdatedAt = DateTime.UtcNow;

            _context.GroupDetails.Update(findUser);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> KickUser(int userId, int groupId, int modId)
        {
            var findUser = await _context.GroupDetails
                .Where(g => g.UserId == userId && g.GroupId == groupId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (findUser == null)
            {
                throw new ArgumentException("User not found in group.");
            }
            else
            {
                findUser.MemberStatus = MemberStatus.Deleted;
                findUser.UpdatedAt = DateTime.UtcNow;
                _context.GroupDetails.Update(findUser);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        //===================== ADMINISTRATOR =======================
        public async Task<bool> CreateMod (int userId, int groupId)
        {
            var findGroup = await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);
            if (findGroup == null)
            {
                throw new ArgumentException("Group not found.");
            }

            var existingMod = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);
            if (existingMod == null)
            {
                throw new ArgumentException($"User not found in group{groupId}.");
            }

            existingMod.Role = GroupRole.Moderator;
            existingMod.UpdatedAt = DateTime.UtcNow;
            _context.GroupDetails.Update(existingMod);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveMod(int userId, int groupId)
        {
            var findGroup = await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == groupId);
            if (findGroup == null)
            {
                throw new ArgumentException("Group not found.");
            }

            var existingMod = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);
            if (existingMod != null)
                return false;

            existingMod.Role = GroupRole.Member;
            existingMod.UpdatedAt = DateTime.UtcNow;
            _context.GroupDetails.Update(existingMod);
            return true;
        }
    }
}
