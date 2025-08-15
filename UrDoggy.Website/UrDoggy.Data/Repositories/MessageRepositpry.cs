using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models; // Message

namespace UrDoggy.Data.Repositories
{
    
    public class MessageRepositpry
    {
        private readonly ApplicationDbContext _context;
        public MessageRepositpry(ApplicationDbContext context)
        {
            _context = context;
        }

       

    }
}
