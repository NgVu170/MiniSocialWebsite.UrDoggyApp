using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Services.Interfaces.GroupServices;

namespace UrDoggy.Website.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class GroupController : Controller
    {
        private readonly IGroupUserService _groupUserService;
        private readonly IAdminGroupService _adminGroupService;
        private readonly IModeratorService _moderatorService;

        public GroupController(
            IGroupUserService groupUserService,
            IAdminGroupService adminGroupService,
            IModeratorService moderatorService)
        {
            _groupUserService = groupUserService;
            _adminGroupService = adminGroupService;
            _moderatorService = moderatorService;
        }

        private bool CheckLogin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return userId != null;
        }

        // ====================== USER ZONE ======================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");
            var modGroups = await _groupUserService.GetModeratorGroupIds(userId.Value);
            var userInGroups = await _groupUserService.GetUserGroupDetails(userId.Value);

            ViewBag.UserInGroups = userInGroups;
            ViewBag.ModGroups = modGroups;
            ViewBag.UserId = userId;

            var groups = await _groupUserService.GetAllGroup();
            return View(groups);
        }

        [HttpGet]
        public IActionResult CreateGroup()
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(string GroupName, string Description, string Avatar, string CoverImage)
        {
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            Avatar = string.IsNullOrWhiteSpace(Avatar) ? "/images/default-avatar.png" : Avatar.Trim();
            CoverImage = string.IsNullOrWhiteSpace(CoverImage) ? "/images/default-cover.png" : CoverImage.Trim();
            var userId = HttpContext.Session.GetInt32("UserId");
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
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _groupUserService.LeaveGroup(userId.Value, groupId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> JoinGroup(int groupId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
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
            var userId = HttpContext.Session.GetInt32("UserId");
            var posts = await _groupUserService.GetAllPost(groupId);
            var groupInfo = await _groupUserService.GetGroupByIdWithOwner(groupId);
            var modList = await _groupUserService.GetModerators(groupId);
            var userRole = await _groupUserService.getRole(userId.Value, groupId);

            ViewBag.GroupInformation = groupInfo;
            ViewBag.ModList = modList;
            ViewBag.Role = userRole;
            ViewBag.UserId = userId;

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePost(int groupId)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "Auth");

            var userId = HttpContext.Session.GetInt32("UserId");
            var isActiveMember = await _groupUserService.IsActiveMember(userId.Value, groupId);
            if (!isActiveMember)
            {
                TempData["Error"] = "Bạn chưa là thành viên đang hoạt động.";
                return RedirectToAction(nameof(PostsInGroup), new { groupId });
            }

            var group = await _groupUserService.getGroupById(groupId);
            if (group == null) return NotFound();

            ViewBag.GroupInformation = group;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(int groupId, string content, List<IFormFile>? media)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "Auth");
            var userId = HttpContext.Session.GetInt32("UserId");
            var isActiveMember = await _groupUserService.IsActiveMember(userId.Value, groupId);
            if (!isActiveMember)
            {
                TempData["Error"] = "Bạn chưa là thành viên đang hoạt động.";
                return RedirectToAction(nameof(PostsInGroup), new { groupId });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Nội dung bài viết không được để trống.";
                return RedirectToAction(nameof(CreatePost), new { groupId });
            }

            var status = new GroupPostStatus
            {
                GroupId = groupId,
                AuthorId = userId.Value,
                Content = content.Trim(),
                Status = StateOfPost.Pending,
                UploaddAt = DateTime.UtcNow,
                MediaItems = new List<Media>()
            };

            if (media != null && media.Count > 0)
            {
                var uploadBase = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "group_uploads",
                    $"group_{groupId}",
                    "posts"
                );

                if (!Directory.Exists(uploadBase))
                    Directory.CreateDirectory(uploadBase);

                foreach (var file in media)
                {
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif" }.Contains(ext)) continue;

                    var fname = $"{Guid.NewGuid()}{ext}";
                    var fpath = Path.Combine(uploadBase, fname);

                    using var stream = new FileStream(fpath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    status.MediaItems.Add(new Media
                    {
                        Path = $"/group_uploads/group_{groupId}/posts/{fname}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _groupUserService.CreatePendingPostAsync(status);

            TempData["Success"] = "Đã gửi bài để kiểm duyệt.";
            return RedirectToAction(nameof(PostsInGroup), new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> ReportPost(int groupId, int postId, string reason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!CheckLogin()) return RedirectToAction("Login", "Auth");

            var ok = await _groupUserService.ReportPost(userId.Value, postId, reason?.Trim() ?? "");
            TempData[ok ? "Success" : "Error"] = ok ? "Đã gửi báo cáo." : "Không thể gửi báo cáo.";

            return RedirectToAction(nameof(PostsInGroup), new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePostByOwner(int groupId, int postId)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "Auth");
            var userId = HttpContext.Session.GetInt32("UserId");
            var post = await _groupUserService.GetPostById(postId);
            if (post == null)
            {
                TempData["Error"] = "Bài viết không tồn tại.";
                return RedirectToAction(nameof(PostsInGroup), new { groupId });
            }

            await _groupUserService.DeletePost(postId, null);
            TempData["Success"] = "Đã xóa bài của bạn.";

            return RedirectToAction(nameof(PostsInGroup), new { groupId });
        }

        // ====================== GROUP MANAGEMENT ======================

        private async Task<bool> CheckPermissionAsync(int userId, int groupId)
        {
            return await _groupUserService.IsModeratorOrAdmin(userId, groupId);
        }

        [HttpGet]
        public async Task<IActionResult> GroupManagement(int groupId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            if (!await CheckPermissionAsync(userId.Value, groupId))
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

        // ====================== POST MODERATION ======================

        [HttpPost]
        public async Task<IActionResult> ApprovePost(int postId, int groupId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _moderatorService.ApprovePost(postId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> RejectPost(int postId, int groupId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!CheckLogin())
                return RedirectToAction("Login", "Auth");

            await _moderatorService.RejectPost(postId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        // ====================== MEMBER MANAGEMENT ======================

        [HttpPost]
        public async Task<IActionResult> PromoteToModerator(int targetUserId, int groupId)
        {
            await _adminGroupService.AddModerator(targetUserId, groupId);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> DemoteToMember(int targetUserId, int groupId)
        {
            await _adminGroupService.RemoveModerator(targetUserId, groupId);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> BanMember(int targetUserId, int groupId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            await _moderatorService.BanUser(targetUserId, groupId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> KickMember(int targetUserId, int groupId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            await _moderatorService.KickUser(targetUserId, groupId, userId.Value);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        // ====================== ADMIN ======================

        [HttpPost]
        public async Task<IActionResult> ReleaseGroup(int groupId)
        {
            await _adminGroupService.DeleteGroup(groupId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditGroupInformation(int groupId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var group = await _groupUserService.getGroupById(groupId);
            var isAdmin = await _groupUserService.IsAdmin(userId.Value, groupId);

            if (!isAdmin) return Forbid();
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroupInformation(
            int groupId,
            string groupName,
            string description,
            IFormFile? avatar,
            IFormFile? coverImage)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var group = await _groupUserService.getGroupById(groupId);
            var isAdmin = await _groupUserService.IsAdmin(userId.Value, groupId);

            if (!isAdmin) return Forbid();

            group.GroupName = groupName;
            group.Description = description;
            group.UpdatedAt = DateTime.UtcNow;
            group.Avatar = avatar != null
                ? $"/images/group_{groupId}_avatar{Path.GetExtension(avatar.FileName)}"
                : group.Avatar;
            group.CoverImage = coverImage != null
                ? $"/images/group_{groupId}_avatar{Path.GetExtension(coverImage.FileName)}"
                : group.CoverImage;

            await _adminGroupService.UpdateGroup(group);
            return RedirectToAction("GroupManagement", new { groupId });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePostByModerator(int groupId, int postId)
        {
            if (!CheckLogin()) return RedirectToAction("Login", "Auth");
            var userId = HttpContext.Session.GetInt32("UserId");
            await _moderatorService.DeletePost(postId, userId.Value);
            TempData["Success"] = "Moderator đã xóa bài.";

            return RedirectToAction(nameof(PostsInGroup), new { groupId });
        }
    }
}
