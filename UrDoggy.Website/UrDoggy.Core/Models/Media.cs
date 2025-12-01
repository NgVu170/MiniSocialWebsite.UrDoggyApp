using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models.GroupModels;

namespace UrDoggy.Core.Models
{
    public class Media
    {
        public int Id { get; set; }
        public int? PostId { get; set; }
        public int? GroupPostStatusId { get; set; }
        public string Path { get; set; }
        public string MediaType { get; set; } // image/video
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Navigation
        [ForeignKey(nameof(PostId))]
        public Post? Post { get; set; }
        [ForeignKey(nameof(GroupPostStatusId))]
        public GroupPostStatus? GroupPostStatus { get; set; }
    }
}
