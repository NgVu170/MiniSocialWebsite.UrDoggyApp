using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Claims;
using UrDoggy.Core.Models;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Data;
using UrDoggy.Services.Interfaces;
using UrDoggy.Services.Interfaces.GroupServices;
using UrDoggy.Services.Service;
using UrDoggy.Services.Service.GroupServices;


namespace UrDoggy.Website.Controllers
{
    [Authorize]
    [Route("[controller]/[action]/{groupId:int}")]
    public class GroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGroupUserService _groupUserService;
        private readonly IAdminGroupService _adminGroupService;
        private int? userId;
        protected int groupId;

        public GroupController(
            IGroupUserService groupUserService,
            IAdminGroupService adminGroupService,
            ApplicationDbContext context)
        {
            _context = context;
            _groupUserService = groupUserService;
            _adminGroupService = adminGroupService;

            //check login
            CheckLogin();
        }

        private IActionResult CheckLogin()
        {
            userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }


        #region User
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var modGroups = await _context.GroupDetails
            .Where(gd => gd.UserId == userId &&
                    (gd.Role == Core.Models.GroupModels.GroupRole.Admin ||
                     gd.Role == Core.Models.GroupModels.GroupRole.Moderator))
            .Select(gd => gd.GroupId)
            .ToHashSetAsync();
            var userInGroups = await _context.GroupDetails.Where(gd => gd.UserId == userId).ToListAsync();

            ViewBag.UserInGroups = userInGroups;
            ViewBag.ModGroups = modGroups;
            ViewBag.UserId = userId;
            var groups = await _groupUserService.GetAllGroup();
            return View(groups);
        }
        [HttpPost]
        private async Task CreateGroup(FormCollection collection)
        {
            var newGroup = new Group
            {
                GroupName = collection["GroupName"].ToString(),
                Description = collection["Description"].ToString(),
                Avatar = collection["Avatar"].ToString(),
                CoverImage = collection["CoverImage"].ToString(),
                OwnerId = userId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                GroupStatus = Status.Active
            };
            await _adminGroupService.CreateGroup(newGroup, userId.Value);
        }
        [HttpPost]
        public async Task LeaveGroup(int groupId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            await _groupUserService.LeaveGroup(userId.Value, groupId);
            RedirectToAction("Index");
        }
        [HttpPost]
        public async Task JoinGroup(int groupId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            await _groupUserService.JoinGroup(userId.Value, groupId);
            RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> PostsInGroup(int groudpId)
        {
            userId = HttpContext.Session.GetInt32("UserId");
            var posts = await _groupUserService.GetAllPost(groudpId);
            var groupInformation = _context.Groups.FirstOrDefault(g => g.Id == groudpId);
            var modList = _context.GroupDetails.Where(gd => gd.Role == GroupRole.Moderator).ToList();
            var temp = _context.GroupDetails
                .FirstOrDefault(gd => gd.GroupId == groudpId && gd.UserId == userId);
            ViewBag.Role = temp.Role;
            ViewBag.UserId = userId;
            ViewBag.ModList = modList;
            ViewBag.GroupInformation = groupInformation;
            return View(posts);
        }

        [HttpGet]
        private async Task<IActionResult> CreatePost()
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            ViewBag.GroupId = groupId;
            ViewBag.GroupName = group.GroupName;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        private async Task<IActionResult> CreatePost(int groupId, string content, List<IFormFile> MediaFiles)
        {
            CheckLogin();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();
            List<Media> mediaItems = new List<Media>();
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var userFolder = Path.Combine(rootPath, $"user_{user.Id}");
            if (!Directory.Exists(userFolder))
                Directory.CreateDirectory(userFolder);

            var newPost = new Post
            {
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                GroupId = groupId,
            };
            if (MediaFiles != null && MediaFiles.Any())
            {
                var postFolder = Path.Combine(userFolder, $"post_{newPost.Id}");
                if (!Directory.Exists(postFolder))
                    Directory.CreateDirectory(postFolder);

                foreach (var file in MediaFiles)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(postFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    var mediaType = file.ContentType.StartsWith("video") ? "video" : "image";

                    mediaItems.Add(new Media
                    {
                        PostId = newPost.Id,
                        Path = $"/uploads/user_{user.Id}/post_{newPost.Id}/{fileName}",
                        MediaType = mediaType,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await _groupUserService.CreatePost(newPost, mediaItems.Select(m => (m.Path, m.MediaType)));
            }
            return RedirectToAction("PostsInGroup", new { groudpId = groupId });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePostByAuthor(int postId, int groupId)
        {
            CheckLogin();
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return NotFound();
            if (post.UserId != userId)
                return Forbid();
            await _groupUserService.DeletePost(postId);
            return RedirectToAction("PostsInGroup", new { groudpId = groupId });
        }
        [HttpPost]
        public async Task<IActionResult> DeletePostByManagement(int postId, int modId)
        {
            CheckLogin();
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return NotFound();
            await _groupUserService.DeletePost(postId, modId);
            return RedirectToAction("PostsInGroup", new { groudpId = groupId });
        }

        [HttpPost]
        public async Task<IActionResult> ReportPost(int postId, int groupId, string reason)
        {
            CheckLogin();
            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return NotFound();
            await _groupUserService.ReportPost(postId, userId.Value, reason);
            return RedirectToAction("PostsInGroup", new { groudpId = groupId });
        } //Bam vao goi form r nhap reason r gui len cho Admin
        #endregion

        #region Group managaement
        private bool CheckPermision(int userId, int groupId)
        {
            var findUser = _context.GroupDetails
                .FirstOrDefault(gd => gd.GroupId == groupId && gd.UserId == userId);
            if (findUser == null) return false;

            if (findUser.Role == Core.Models.GroupModels.GroupRole.Admin ||
                findUser.Role == Core.Models.GroupModels.GroupRole.Moderator)
            {
                return true;
            }
            else
                return false;
        }

        [HttpGet]
        public async Task<IActionResult> GroupManagement(int groupId)
        {
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                if (!CheckPermision(userId, groupId))
                {
                    return Forbid();
                }
                var group = await _groupUserService.getGroupById(groupId);
                return View(group);
            }
            else
            {
                return Unauthorized();
            }
        }
        #endregion
    }
}
