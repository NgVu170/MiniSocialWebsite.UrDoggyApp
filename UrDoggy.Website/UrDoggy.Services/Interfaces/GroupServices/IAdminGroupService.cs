using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Services.Interfaces.GroupServices
{
    public interface IAdminGroupService : IModeratorService
    {
        Task<bool> CreateGroup(Core.Models.GroupModels.Group group, int adminId, IEnumerable<(string path, string mediaType)> media);
        Task<bool> DeleteGroup(int groupId, int adminId);
        Task<bool> AddModerator(int userId, int groupId, int adminId);
        Task<bool> RemoveModerator(int userId, int groupId, int adminId);
        Task<bool> UpdateGroup(Core.Models.GroupModels.Group group, int adminId, IEnumerable<(string path, string mediaType)> media);
    }
}
