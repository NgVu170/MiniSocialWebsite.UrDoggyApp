using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrDoggy.Core.Models
{
    public class Conversation
    {
        public int OtherUserId { get; set; }
        public User OtherUser { get; set; }
        public string OtherPicture { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
