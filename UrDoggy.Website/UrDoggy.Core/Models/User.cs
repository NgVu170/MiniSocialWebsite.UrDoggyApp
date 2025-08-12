using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace UrDoggy.Core.Models
{
    public class User : IdentityUser<int>
    {
        public string ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsAdmin { get; set; } = false;
        public string DisplayName { get; set; }
        public string Bio { get; set; }

        // Navigation
        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<PostVote> PostVotes { get; set; }
        public ICollection<Friend> Friends { get; set; }
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<Message> ReceivedMessages { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
