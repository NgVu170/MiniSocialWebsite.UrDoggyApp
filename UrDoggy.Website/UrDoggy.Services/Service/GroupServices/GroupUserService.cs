using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Data;
using UrDoggy.Data.Repositories;
using UrDoggy.Data.Repositories.Group_Repository;
using UrDoggy.Services.Interfaces.GroupServices;

namespace UrDoggy.Services.Service.GroupServices
{
    public class GroupUserService : IGroupUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserGroupRepositrory _userGroupRepositrory;
        private readonly PostRepository _postRepository;
        private readonly GroupPostRepository _groupPostRepository;
        public GroupUserService(ApplicationDbContext context)
        {
            _groupPostRepository = new GroupPostRepository(context);
            _postRepository = new PostRepository(context);
            _userGroupRepositrory = new UserGroupRepositrory(context);
            _context = context;
        }
        public async Task<List<Group>> GetAllGroup()
        {
            return await _context.Groups.AsNoTracking().ToListAsync();
        }
        public async Task<List<Post>> GetAllPost(int groupId)
        {
            var querry = _context.Posts.Where(g => g.GroupId == groupId).AsQueryable();
            return await querry
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<GroupDetail>> GetAllMemberInGroup(int groupId)
        {
            return await _context.GroupDetails
                .Where(g => g.GroupId == groupId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<GroupDetail>> GetAllGroupOfUser(int userId)
        {
            return await _context.GroupDetails
                .Where(g => g.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<bool> JoinGroup(int userId, int groupId)
        {
            var existingMembership = await _context.GroupDetails
                .FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);
            if (existingMembership != null)
            {
                return false; // User is already a member of the group
            }
            else
            {
                return await _userGroupRepositrory.JoinGroup(userId, groupId);
            }
        }
        public async Task<bool> LeaveGroup(int userId, int groupId)
        {
            var existingMembership = await _context.GroupDetails
                .FirstOrDefaultAsync(g => g.UserId == userId && g.GroupId == groupId);
            if (existingMembership == null)
            {
                return false; // User is not a member of the group
            }
            else
            {
                return await _userGroupRepositrory.LeaveGroup(userId, groupId);
            }
        }
        public async Task<Post> CreatePost(Post post, IEnumerable<(string path, string mediaType)> media)
        {
            var postRepo = new GroupPostRepository(_context);
            await postRepo.CreatePost(post, media);
            return post;
        }
        public async Task DeletePost(int postId, int? modId = null)
        {
            var postRepo = new PostRepository(_context);
            await _postRepository.DeletePost(postId, modId);
        }
        public async Task<bool> ReportPost(int postId, int userId, string reason)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == postId && p.GroupId.HasValue);
            if (post == null)
            {
                throw new ArgumentException("Post not found or is not a group post.");
            }
            else
            {
                await _groupPostRepository.ReportPost(postId, userId, reason);
                return true;
            }
        }
    }
}
