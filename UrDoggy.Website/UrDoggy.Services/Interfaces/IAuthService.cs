using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> Register(string username, string email, string password,
                                    string profilePicture, string displayName, string bio);
        Task<bool> Login(string username, string password);
        Task<string> HashPassword(User user, string password);
        Task<bool> VerifyPassword(User user, string password);
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserById(int userId);
        Task<bool> IsEmailConfirmed(User user);
        Task<string> GenerateEmailConfirmationToken(User user);
        Task<IdentityResult> ConfirmEmail(User user, string token);
        Task<IdentityResult> ChangePassword(User user, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetToken(User user);
        Task<IdentityResult> ResetPassword(User user, string token, string newPassword);
    }
}
