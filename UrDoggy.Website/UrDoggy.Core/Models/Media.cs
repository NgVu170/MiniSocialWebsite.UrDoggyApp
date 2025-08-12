using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models
{
    public class Media
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }

        public string Path { get; set; }
        public string MediaType { get; set; } // image/video
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
