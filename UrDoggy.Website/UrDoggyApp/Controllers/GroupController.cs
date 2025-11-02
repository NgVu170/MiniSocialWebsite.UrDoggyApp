using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Data;
using UrDoggy.Services.Interfaces.GroupServices;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class GroupController : Controller
    {
        #region Attributes & Constructor, Helper function
        private readonly ApplicationDbContext _context;
        private readonly IGroupUserService _groupUserService;
        private readonly IAdminGroupService _adminGroupService;
        private readonly IModeratorService _moderatorService;
        private int? userId;

        public GroupController(
            IGroupUserService groupUserService,
            IAdminGroupService adminGroupService,
            IModeratorService moderatorService,
            ApplicationDbContext context)
        {
            _context = context;
            _groupUserService = groupUserService;
            _adminGroupService = adminGroupService;
            _moderatorService = moderatorService;
        }

        private bool CheckLogin()
        {
            userId = HttpContext.Session.GetInt32("UserId");
            return userId != null;
        }
        #endregion
        // ============= USER ZONE ==============
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            var modGroups = await _context.GroupDetails
                .Where(gd => gd.UserId == userId &&
                        (gd.Role == GroupRole.Admin || gd.Role == GroupRole.Moderator))
                .Select(gd => gd.GroupId)
                .ToHashSetAsync();

            var userInGroups = await _context.GroupDetails
                .Where(gd => gd.UserId == userId)
                .ToListAsync();

            ViewBag.UserInGroups = userInGroups;
            ViewBag.ModGroups = modGroups;
            ViewBag.UserId = userId;
            var groups = await _groupUserService.GetAllGroup();
            return View(groups);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(string GroupName, string Description, string Avatar, string CoverImage)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            var newGroup = new Group
            {
                GroupName = GroupName,
                Description = Description,
                Avatar = Avatar,
                CoverImage = CoverImage,
                OwnerId = userId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                GroupStatus = Status.Active
            };
            await _adminGroupService.CreateGroup(newGroup, userId.Value);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _groupUserService.LeaveGroup(userId.Value, groupId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> JoinGroup(int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _groupUserService.JoinGroup(userId.Value, groupId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> PostsInGroup(int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            var posts = await _groupUserService.GetAllPost(groupId);
            var groupInfo = await _context.Groups.Include(g => g.Owner).FirstOrDefaultAsync(g => g.Id == groupId);
            var modList = await _context.GroupDetails.Include(gd => gd.User)
                .Where(gd => gd.GroupId == groupId && gd.Role == GroupRole.Moderator)
                .ToListAsync();

            var userRole = await _groupUserService.getRole(userId.Value, groupId);

            ViewBag.GroupInformation = groupInfo;
            ViewBag.ModList = modList;
            ViewBag.Role = userRole;
            ViewBag.UserId = userId;

            return View(posts);
        }

        // ============= GROUP MANAGEMENT ZONE ==============
        private bool CheckPermission(int userId, int groupId)
        {
            var userInGroup = _context.GroupDetails
                .FirstOrDefault(gd => gd.GroupId == groupId && gd.UserId == userId);

            return userInGroup != null && (userInGroup.Role == GroupRole.Admin || userInGroup.Role == GroupRole.Moderator);
        }

        [HttpGet]
        public async Task<IActionResult> GroupManagement(int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            if (!CheckPermission(userId.Value, groupId))
                return Forbid();

            var group = await _groupUserService.getGroupById(groupId);
            var pendingPostList = await _adminGroupService.GetAllPendingPost(groupId);
            var memberList = await _groupUserService.GetAllMemberInGroup(groupId);
            var reportList = await _moderatorService.GetAllReportPost(groupId);
            var role = await _groupUserService.getRole(userId.Value, groupId);

            ViewBag.PendingPostList = pendingPostList;
            ViewBag.MemberList = memberList;
            ViewBag.ReportList = reportList;
            ViewBag.Role = role;

            return View(group);
        }

        // ============= POST MANAGEMENT ==============
        [HttpPost]
        public async Task<IActionResult> ApprovePost(int postId, int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _moderatorService.ApprovePost(postId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> RejectPost(int postId, int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _moderatorService.RejectPost(postId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        // ============= MEMBER MANAGEMENT ==============
        [HttpPost]
        public async Task<IActionResult> PromoteToModerator(int targetUserId, int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _adminGroupService.AddModerator(targetUserId, groupId);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> DemoteToMember(int targetUserId, int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _adminGroupService.RemoveModerator(targetUserId, groupId);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> BanMember(int targetUserId, int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _moderatorService.BanUser(targetUserId, groupId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> KickMember(int targetUserId, int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _moderatorService.KickUser(targetUserId, groupId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        // ============= ADMIN GROUP ==============
        [HttpPost]
        public async Task<IActionResult> ReleaseGroup(int groupId)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _adminGroupService.DeleteGroup(groupId);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> EditGroupInformation(int groupId)
        {
            CheckLogin();
            var group = await _context.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return NotFound();

            // Chỉ cho phép Admin chỉnh sửa group
            var detail = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(gd => gd.GroupId == groupId && gd.UserId == userId);

            if (detail == null || detail.Role != GroupRole.Admin)
                return Forbid();

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroupInformation(int groupId, string groupName, string description, IFormFile? avatar, IFormFile? coverImage)
        {
            CheckLogin();
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
                return NotFound();
            var detail = await _context.GroupDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(gd => gd.GroupId == groupId && gd.UserId == userId);
            if (detail == null || detail.Role != GroupRole.Admin)
                return Forbid();

            group.GroupName = groupName;
            group.Description = description;
            group.UpdatedAt = DateTime.UtcNow;

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "group_uploads", $"group_{groupId}");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            if (avatar != null)
            {
                var avatarFileName = $"avatar_{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
                var avatarPath = Path.Combine(uploadPath, avatarFileName);
                using (var stream = new FileStream(avatarPath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }
                group.Avatar = $"/group_uploads/group_{groupId}/{avatarFileName}";
            }

            if (coverImage != null)
            {
                var coverFileName = $"cover_{Guid.NewGuid()}{Path.GetExtension(coverImage.FileName)}";
                var coverPath = Path.Combine(uploadPath, coverFileName);
                using (var stream = new FileStream(coverPath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(stream);
                }
                group.CoverImage = $"/group_uploads/group_{groupId}/{coverFileName}";
            }

            await _adminGroupService.UpdateGroup(group);
            return RedirectToAction("GroupManagement", new { groupId });
        }
    }
}
