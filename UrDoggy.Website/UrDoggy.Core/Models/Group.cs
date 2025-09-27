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
        public string GroupName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int OwnerId { get; set; }
        public Status GroupStatus { get; set; } = Status.Active;

        //Navigation
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        public ICollection<GroupDetail> Members { get; set; }
        public ICollection<Post> Posts { get; set; }
    }

    public enum Status
    {
        Active,
        Warning,
        Banned,
        Deleted
    }
}
