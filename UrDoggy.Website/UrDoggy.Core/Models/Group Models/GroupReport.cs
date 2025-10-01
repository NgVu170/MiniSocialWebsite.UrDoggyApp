using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models.Group_Models
{
    public class GroupReport
    {
        public int Id { get; set; }
        public int GroupPostId { get; set; }
        public int ReporterId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Navigation
        public Post GroupPost { get; set; } = null!;
        public User Reporter { get; set; } = null!;

    }
}
