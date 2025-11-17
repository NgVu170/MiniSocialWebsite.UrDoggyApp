using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetProfile(int userId);
        Task UpdateProfile(int userId, string displayName, string bio, string profilePicture);
        Task<List<User>> Search(string searchTerm);
        Task<List<User>> TagUser(string userPart);
        Task<User> GetById(int userId);
        Task<User> GetByUsername(string username);
        Task<List<User>> GetAllUsers();
        Task DeleteUser(int userId);
        Task<int> GetFriendCount(int userId);
        Task<bool> CheckUsernameExists(string username);
        Task<bool> CheckEmailExists(string email);
        Task<int> GetCurrentUserId(ClaimsPrincipal user);
        Task<User> GetCurrentUser(ClaimsPrincipal user);
    }
}
