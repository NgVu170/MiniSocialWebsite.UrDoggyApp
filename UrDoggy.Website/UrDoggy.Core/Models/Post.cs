using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Data.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpVotes { get; set; } = 0;
        public int DownVotes { get; set; } = 0;
        public string MediaPath { get; set; }
        public string MediaType { get; set; }

        // Navigation
        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostVote> PostVotes { get; set; }
        public ICollection<PostTag> PostTags { get; set; }
        public ICollection<Media> MediaItems { get; set; }
    }
}
