using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models
{
    [Table ("Groups")]
    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int OwnerId { get; set; }
        public Status GroupStatus { get; set; } = Status.Active;

        //Navigation
        public User Owner { get; set; } = null!;
        public ICollection<GroupDetail> Members { get; set; } = new List<GroupDetail>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }

    public enum Status
    {
        Active,
        Warning,
        Banned,
        Deleted
    }
}
