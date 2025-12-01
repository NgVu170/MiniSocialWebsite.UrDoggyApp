using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Core.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? GroupId { get; set; } // Nullable for posts not in a group
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int UpVotes { get; set; } = 0;
        public int DownVotes { get; set; } = 0;
        // Navigation
        public User User { get; set; } = null!;
        public Group? Group { get; set; }
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostVote> PostVotes { get; set; } = new List<PostVote>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public ICollection<Media>? MediaItems { get; set; } = new List<Media>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<User>? TaggedUsers { get; set; } = null;
    }
}
