using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Services.Interfaces.GroupServices
{
    public interface IAdminGroupService : IModeratorService
    {
        Task<Group> CreateGroup(Core.Models.GroupModels.Group group, int ownerId);
        Task DeleteGroup(int groupId);
        Task<bool> AddModerator(int userId, int groupId);
        Task<bool> RemoveModerator(int userId, int groupId);
        Task<Group> UpdateGroup(Core.Models.GroupModels.Group group);
    }
}
