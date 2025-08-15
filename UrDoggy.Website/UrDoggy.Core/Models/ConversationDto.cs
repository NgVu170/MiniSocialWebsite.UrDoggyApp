using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrDoggy.Core.Models;

namespace UrDoggy.Core.Models
{
    public class ConversationDto
    {
        public int OtherId { get; set; }
        public string OtherUsername { get; set; }
        public string OtherPicture { get; set; }
        public string Preview { get; set; }
        public int PreviewSenderId { get; set; }
        public DateTime Time { get; set; }
    }
}
