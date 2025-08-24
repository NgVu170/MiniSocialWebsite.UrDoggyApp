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
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly FriendRepository _friendRepository;

        public UserService(UserRepository userRepository, FriendRepository friendRepository)
        {
            _userRepository = userRepository;
            _friendRepository = friendRepository;
        }

        public async Task<User> GetProfile(int userId)
        {
            return await _userRepository.GetByI(userId);
        }

        public async Task UpdateProfile(int userId, string displayName, string bio, string profilePicture)
        {
            await _userRepository.UpdateProfile(userId, displayName, bio, profilePicture);
        }

        public async Task<List<User>> Search(string searchTerm)
        {
            return await _userRepository.Search(searchTerm);
        }

        public async Task<User> GetById(int userId)
        {
            return await _userRepository.GetByI(userId);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _userRepository.GetAllUsers();
        }

        public async Task DeleteUser(int userId)
        {
            await _userRepository.DeleteUser(userId);
        }

        public async Task<int> GetFriendCount(int userId)
        {
            var friends = await _friendRepository.GetFriends(userId);
            return friends.Count;
        }

        public async Task<bool> CheckUsernameExists(string username)
        {
            var user = await _userRepository.GetByUsername(username);
            return user != null;
        }

        public async Task<bool> CheckEmailExists(string email)
        {
            var user = await _userRepository.GetByEmail(email);
            return user != null;
        }
    }
}
