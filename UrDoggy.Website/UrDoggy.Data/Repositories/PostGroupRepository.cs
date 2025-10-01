using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using UrDoggy.Core.Models;

namespace UrDoggy.Data.Repositories
{
    public class PostGroupRepository
    {
        private readonly ApplicationDbContext _context;
        public PostGroupRepository(ApplicationDbContext context)
        {
            this._context = context;
        }

        // READ: feed
        public async Task<List<Post>> GetAllPostGroups(int groupId)
        {
            return await _context.Posts
                .OrderBy(pg => pg.Id)
                .Where(pg => pg.GroupId == groupId)
                .AsNoTracking()
                .ToListAsync();
        }

    }
}
