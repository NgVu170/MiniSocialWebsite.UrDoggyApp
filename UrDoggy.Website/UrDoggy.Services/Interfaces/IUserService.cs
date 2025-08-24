using System;
using System.Collections.Generic;
using System.Linq;
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
        Task<User> GetById(int userId);
        Task<List<User>> GetAllUsers();
        Task DeleteUser(int userId);
        Task<int> GetFriendCount(int userId);
        Task<bool> CheckUsernameExists(string username);
        Task<bool> CheckEmailExists(string email);
    }
}
