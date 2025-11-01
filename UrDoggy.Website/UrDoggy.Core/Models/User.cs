using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UrDoggy.Core.Models.GroupModels;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Core.Models
{
    public class User : IdentityUser<int>
    {
        public string ProfilePicture { get; set; } = "./UrDoggy.Webtisite/wwwroot/images/default-avatar.png";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; } = false;
        public string DisplayName { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;

        // Navigation
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostVote> PostVotes { get; set; } = new List<PostVote>();
        public ICollection<Friend> Friends { get; set; } = new List<Friend>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
        public ICollection<GroupDetail> GroupDetails { get; set; } = new List<GroupDetail>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<GroupReport> GroupReports { get; set; } = new List<GroupReport>();

        [InverseProperty(nameof(GroupPostStatus.Author))]
        public ICollection<GroupPostStatus> GroupPostsCreated { get; set; } = new List<GroupPostStatus>();

        [InverseProperty(nameof(GroupPostStatus.Mod))]
        public ICollection<GroupPostStatus> GroupPostsModerated { get; set; } = new List<GroupPostStatus>();
    }
}

