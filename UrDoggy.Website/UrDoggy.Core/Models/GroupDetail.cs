using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models
{
    [Table("GroupDetails")]
    public class GroupDetail
    {
        public int GroupId { get; set; }
        public int MemberIds { get; set; }
        public GroupRole Roles { get; set; } = GroupRole.Member; // default role is Member
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Status MemberStatus { get; set; } = Status.Active; // default status is Active
        int activtyScore { get; set; } = 0; // default activity score is 0
        //navigation
        [ForeignKey("GroupId")]
        public Group Group { get; set; }
        [ForeignKey("MemberIds")]
        public User Member { get; set; }
    }

    public enum GroupRole
    {
        Owner,
        Admin,
        Member
    }
}
