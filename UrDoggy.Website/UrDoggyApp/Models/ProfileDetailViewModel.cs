using UrDoggy.Core.Models;

namespace UrDoggy.Website.Models
{
    public class ProfileDetailViewModel
    {
        public User User { get; set; }
        public List<Post> Posts { get; set; }
        public Dictionary<int, List<Comment>> CommentsMap { get; set; }

        public bool IsOwnProfile { get; set; }
        public bool IsFriend { get; set; }
        public bool HasSentRequest { get; set; }
        public bool HasReceivedRequest { get; set; }
        public bool CanSendRequest { get; set; }
    }
}
