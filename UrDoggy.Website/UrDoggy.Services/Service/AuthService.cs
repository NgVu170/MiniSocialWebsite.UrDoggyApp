using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;
using UrDoggy.Data.Repositories;
using UrDoggy.Services.Interfaces;

namespace UrDoggy.Services.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly UserManager<User> _userManager;

        public AuthService(UserRepository userRepository, UserManager<User> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<IdentityResult> Register(string username, string email, string password,
                                                  string profilePicture, string displayName, string bio)
        {
            return await _userRepository.Register(username, email, password, profilePicture, displayName, bio);
        }

        public async Task<bool> Login(string username, string password)
        {
            var user = await _userRepository.GetByUsername(username);
            if (user == null)
                return false;

            return await VerifyPassword(user, password);
        }

        public async Task<string> HashPassword(User user, string password)
        {
            return _userManager.PasswordHasher.HashPassword(user, password);
        }

        public async Task<bool> VerifyPassword(User user, string password)
        {
            var result = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _userRepository.GetByUsername(username);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _userRepository.GetByEmail(email);
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _userRepository.GetByI(userId);
        }

        public async Task<bool> IsEmailConfirmed(User user)
        {
            return await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationToken(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmail(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<IdentityResult> ChangePassword(User user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<string> GeneratePasswordResetToken(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPassword(User user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }
}
