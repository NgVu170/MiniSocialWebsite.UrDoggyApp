using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Data;
using UrDoggy.Services.Interfaces;
using UrDoggy.Services.Interfaces.GroupServices;
using UrDoggy.Services.Service;
using UrDoggy.Services.Service.GroupServices;


namespace UrDoggy.Website.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IGroupUserService _groupUserService;
        private readonly IAdminGroupService _adminGroupService;
        private readonly IModeratorService _moderatorService;

        public GroupController(
            IGroupUserService groupUserService, 
            IAdminGroupService adminGroupService,
            ApplicationDbContext context,
            IModeratorService moderatorService)
        {
            _context = context;
            _groupUserService = groupUserService;
            _moderatorService = moderatorService;
            _adminGroupService = adminGroupService;
        }

        #region User in Group
        public async Task<IActionResult> AllGroups()
        {
            var groups = await _groupUserService.GetAllGroup();
            return View(groups);
        }
        public async Task<IActionResult> GetAllMemberIngroupd(int groupId)
        {
            var members = await _groupUserService.GetAllMemberInGroup(groupId);
            return View(members);
        }
        public async Task<IActionResult> GroupPosts(int groupId)
        {
            var posts = await _groupUserService.GetAllPost(groupId);
            return View(posts);
        }
        public async Task<IActionResult> GroupDetail(int groupId)
        {
            var group = await _groupUserService.getGroupById(groupId);
            return View(group);
        }
        #endregion

        #region Modereator in Group
        private bool isModCheck(int userId, int groupId)
        {
            var findUser = _context.GroupDetails
                .FirstOrDefault(g => g.Id == groupId && g.UserId == userId);
            if (findUser.Role == Core.Models.GroupModels.GroupRole.Admin ||
                findUser.Role == Core.Models.GroupModels.GroupRole.Moderator)
                return true;
            else
                return false;
        }

        public async Task<IActionResult> ModeratorGroupManagement(int groupId, int userId)
        {
            if (isModCheck(userId, groupId) == false)
                return Forbid();
            var group = await _moderatorService.GetAllPendingPost(groupId);
            return View(group);
        }

        #endregion

        #region Admin in group
        private bool isAdminCheck(int userId, int groupId)
        {
            var findUser = _context.GroupDetails
                .FirstOrDefault(g => g.Id == groupId && g.UserId == userId);
            if (findUser.Role == Core.Models.GroupModels.GroupRole.Admin)
                return true;
            else
                return false;
        }
        public async Task<IActionResult> AdminGroupManagement(int groupId, int userId)
        {
            if (isAdminCheck(userId, groupId) == false)
                return Forbid();
            var group = await _groupUserService.GetAllPost(groupId);
            return View(group);
        }
        #endregion

    }
}
