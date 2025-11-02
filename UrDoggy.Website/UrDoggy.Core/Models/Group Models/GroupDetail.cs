using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models.GroupModels
{
    [Table("GroupDetails")]
    public class GroupDetail
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public GroupRole Role { get; set; } = GroupRole.Member; // default role is Member
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public MemberStatus MemberStatus { get; set; } = MemberStatus.Active; // default status is Active
        public int ActivtyScore { get; set; } = 0; // default activity score is 0
        //navigation
        public Group Group { get; set; } = null!;
        public User User { get; set; } = null!;
    }

    public enum GroupRole
    {
        Admin,
        Moderator,
        Member
    }
    public enum MemberStatus
    {
        Active,
        Banned,
        Deleted,
        Leaved
    }
}
