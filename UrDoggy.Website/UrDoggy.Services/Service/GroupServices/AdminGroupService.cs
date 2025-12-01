using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Data;
using UrDoggy.Data.Repositories.Group_Repository;
using UrDoggy.Services.Interfaces.GroupServices;

namespace UrDoggy.Services.Service.GroupServices
{
    public class AdminGroupService : ModeratorService, IAdminGroupService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserGroupRepositrory _userGroupRepository;
        private readonly GroupRepository _groupRepository;

        public AdminGroupService(ApplicationDbContext context) : base(context)
        {
            _context = context;
            _userGroupRepository = new UserGroupRepositrory(_context);
            _groupRepository = new GroupRepository(_context);
        }

        public async Task<bool> AddModerator(int userId, int groupId)
        {
            return await _userGroupRepository
                .CreateMod(userId, groupId);
        }

        public async Task<Group> CreateGroup(Group group, int ownerId)
        {
            return await _groupRepository
                .CreateGroup(group, ownerId);
        }

        public async Task DeleteGroup(int groupId)
        {
            await _groupRepository
                .DeleteGroup(groupId);
        }

        public async Task<bool> RemoveModerator(int userId, int groupId)
        {
            return await _userGroupRepository
                .RemoveMod(userId, groupId);
        }

        public async Task<Group> UpdateGroup(Group group)
        {
            return await _groupRepository
                .UpdateGroup(group);
        }
    }
}
