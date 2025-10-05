using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Data.Repositories.Group_Repository
{
    public class GroupRepository
    {
        private readonly ApplicationDbContext _context;
        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //======================= ADMIN =============================
        public async Task<Group> CreateGroup(Group group, int creator)
        {
            if (group.CreatedAt == default) group.CreatedAt = DateTime.UtcNow;
            group.OwnerId = creator;
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }
        public async Task<Group> UpdateGroup(Group group)
        {
            var existingGroup = await _context.Groups.FindAsync(group.Id);
            if (existingGroup == null)
            {
                throw new ArgumentException("Group not found.");
            }
            existingGroup.GroupName = group.GroupName;
            existingGroup.Description = group.Description;
            existingGroup.Avatar = group.Avatar;
            existingGroup.CoverImage = group.CoverImage;
            existingGroup.UpdatedAt = DateTime.UtcNow;
            _context.Groups.Update(existingGroup);
            await _context.SaveChangesAsync();
            return existingGroup;
        }
        public async Task DeleteGroup(int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                throw new ArgumentException("Group not found.");
            }
            group.GroupStatus = Status.Deleted;
            await _context.SaveChangesAsync();
        }

        //======================= MODERATOR =============================
        public async Task<List<Group>> GetGroupById(int groupId)
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.Id == groupId)
                .ToListAsync();
        }
        public async Task<List<Group>> GetAllGroups()
        {
            return await _context.Groups
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<Group>> GetGroupsByOwner(int ownerId)
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.OwnerId == ownerId)
                .ToListAsync();
        }
        public async Task<List<Group>> GetGroupsByStatus(Status status)
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.GroupStatus == status)
                .ToListAsync();
        }
        public async Task<List<GroupDetail>> GetGroupMembers(int groupId)
        {
            return await _context.GroupDetails
                .AsNoTracking()
                .Where(gd => gd.GroupId == groupId)
                .Include(gd => gd.User)
                .ToListAsync();
        }
    }
}
