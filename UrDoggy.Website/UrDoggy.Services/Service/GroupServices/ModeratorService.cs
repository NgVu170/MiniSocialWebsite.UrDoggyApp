using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models.Group_Models;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Data;
using UrDoggy.Data.Repositories.Group_Repository;
using UrDoggy.Services.Interfaces.GroupServices;

namespace UrDoggy.Services.Service.GroupServices
{
    public class ModeratorService : GroupUserService, IModeratorService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserGroupRepositrory _userGroupRepositrory;
        private readonly GroupPostRepository _groupPostRepository;

        public ModeratorService(ApplicationDbContext context)
        : base(context)
        {
            _context = context;
            _userGroupRepositrory = new UserGroupRepositrory(_context);
            _groupPostRepository = new GroupPostRepository(_context);
        }

        public async Task<bool> ApprovePost(int groupPostStatusId, int modId)
        {
            return await _groupPostRepository
                .ApprovePost(groupPostStatusId, modId);
        }

        public async Task<bool> BanUser(int userId, int groupId, int modId, string reason)
        {
            return await _userGroupRepositrory
                .BanUser(userId, groupId, modId);
        }

        public async Task DeletePost(int postId, int modId)
        {
            await _groupPostRepository
                .DeletePost(postId, modId);
        }

        public async Task<List<GroupPostStatus>> GetAllPendingPost(int groupId)
        {
            return await _groupPostRepository
                .GetAllPendingPost(groupId);
        }

        public async Task<List<GroupReport>> GetAllReportPost(int groupId)
        {
            return await _groupPostRepository
                .GetAllReportPost(groupId);
        }

        public async Task<bool> KickUser(int userId, int groupId, int modId)
        {
            return await _userGroupRepositrory
                .KickUser(userId, groupId, modId);
        }

        public async Task<bool> RejectPost(int grouPostStatusId, int modId)
        {
            return await _groupPostRepository
                .DeniedPost(grouPostStatusId, modId);
        }

        public async Task<bool> UnbanUser(int userId, int groupId, int modId)
        {
            return await _userGroupRepositrory
                .UnbanUser(userId, groupId, modId);
        }
    }
}
