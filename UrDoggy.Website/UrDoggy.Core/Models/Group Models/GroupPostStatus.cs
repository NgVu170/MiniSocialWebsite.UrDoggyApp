using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models.GroupModels
{
    public class GroupPostStatus
    {
        public int Id { get; set; }
        public int? PostId { get; set; }
        public int GroupId { get; set; }
        public int AuthorId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime UploaddAt { get; set; } = DateTime.UtcNow;
        public StateOfPost Status { get; set; } = StateOfPost.Pending; // default status is Pending
        public DateTime StatusUpdate { get; set; } = DateTime.UtcNow; // default status update is now
        public int? ModId { get; set; } // Moderator who changed the status

        // Navigation
        [ForeignKey(nameof(PostId))]
        public Post? Post { get; set; } = null!;
        [ForeignKey(nameof(GroupId))]
        public Group Group { get; set; } = null!;
        [ForeignKey(nameof(AuthorId))]
        [InverseProperty(nameof(User.GroupPostsCreated))]
        public User Author { get; set; } = null!;
        [ForeignKey(nameof(ModId))]
        [InverseProperty(nameof(User.GroupPostsModerated))]
        public User? Mod { get; set; } // Moderator who changed the status
        public ICollection<Media> MediaItems { get; set; } = new List<Media>();
    }

    public enum StateOfPost
    {
        Pending,
        Approved,
        Rejected,
        Removed,
        Reported
    }
}
