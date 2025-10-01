using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models.Group_Models;
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
            if (existingMember != null)
                return false;
            var newMember = new GroupDetail
            {
                UserId = userId,
                GroupId = groupId,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
            _context.GroupDetails.Remove(existingMember);
            await _context.SaveChangesAsync();
            return true;
        }
        //======================= MODERATOR =========================
        public async Task<bool> BanUser(int userId)
        {
            throw new Exception("Not implemented yet.");
        }
        public async Task<bool> KickUser(int userId)
        {
            throw new Exception("Not implemented yet.");
        }
        //===================== ADMINISTRATOR =======================
        public async Task<bool> CreateMod (int userId, int groupId)
        {
            var findGroup = await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.OwnerId == groupId);
            if (findGroup == null)
            {
                throw new ArgumentException("Group not found.");
            }

            var existingMod = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId && m.GroupId == groupId);
            if (existingMod != null)
                return false;

            existingMod.Role = GroupRole.Moderator;
            existingMod.UpdatedAt = DateTime.UtcNow;
            _context.GroupDetails.Update(existingMod);
            return true;
        }
        public async Task<bool> RemoveMod(int userId, int groupId)
        {
            var findGroup = await _context.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.OwnerId == groupId);
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
