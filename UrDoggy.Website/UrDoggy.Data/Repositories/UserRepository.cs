using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UrDoggy.Data;
using UrDoggy.Core.Models;


namespace UrDoggy.Data.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserRepository(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IdentityResult> Register(string username, string email, string password, string? profilePicture, string? displayName, string? bio)
        {
            if (string.IsNullOrEmpty(profilePicture))
            {
                profilePicture = "/Upload/Images/default_image.jpg";
            }
            var user = new User
            {
                UserName = username,
                Email = email,
                ProfilePicture = profilePicture ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? username : displayName,
                Bio = bio ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false,
                LockoutEnabled = true,
            };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception($"User registration failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return result;
        }

        public async Task<User?> GetByI(int id)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task UpdateProfile(int id, string? displayName, string? bio, string? picPath)
        {
            var u = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return;

            u.DisplayName = displayName ?? u.DisplayName;
            u.Bio = bio ?? u.Bio;
            u.ProfilePicture = picPath ?? u.ProfilePicture;

            await _context.SaveChangesAsync();
        }

        public Task<List<User>> GetAllUsers()
        {
            return _context.Users.AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteUser(int id)
        {
            var u = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return;
            // dùng Identity, xóa qua UserManager để dọn claims/roles/logins
            await _userManager.DeleteAsync(u);
        }

        public Task<List<User>> Search(string q)
        {
            q ??= string.Empty;
            return _context.Users.AsNoTracking()
                .Where(u =>
                    EF.Functions.Like(u.UserName!, $"%{q}%") ||
                    EF.Functions.Like(u.DisplayName!, $"%{q}%"))
                .Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    ProfilePicture = u.ProfilePicture
                })
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }
    }
}
