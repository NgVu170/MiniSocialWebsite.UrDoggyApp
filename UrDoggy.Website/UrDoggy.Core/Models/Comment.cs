using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models
{
    public class Comment
    {
        public int Id { get; set; }
        //Post
        public int PostId { get; set; }
        public Post Post { get; set; }
        //User
        public int UserId { get; set; }
        public User User { get; set; }
        //Group
        public int? GroupId { get; set; } // Nullable for comments not in a group
        public Group? Group { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
