using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int ReporterId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; } = null!;
        [ForeignKey(nameof(ReporterId))]
        public User Reporter { get; set; } = null!;
    }
}
