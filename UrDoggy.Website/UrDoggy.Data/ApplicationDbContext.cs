using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UrDoggy.Core.Models;     // Comment, Message, Notification, Media, PostVote, Tag, PostTag, Report, Conversation(ignored), ConversationDto(ignored)

namespace UrDoggy.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostVote> PostVotes => Set<PostVote>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<PostTag> PostTags => Set<PostTag>();
        public DbSet<Media> Media => Set<Media>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Friend> Friends => Set<Friend>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<GroupDetail> GroupDetails => Set<GroupDetail>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model); // cần cho Identity

            // ======================
            // USER (Identity extension fields)
            // ======================
            model.Entity<User>(e =>
            {
                e.Property(x => x.DisplayName).HasMaxLength(128).IsRequired(false);
                e.Property(x => x.ProfilePicture).HasMaxLength(512).IsRequired(false);
                e.Property(x => x.Bio).HasMaxLength(2000).IsRequired(false);
            });

            // ======================
            // POST
            // ======================
            model.Entity<Post>(e =>
            {
                e.ToTable("Posts");
                e.HasKey(x => x.Id);
                e.Property(x => x.Content).HasMaxLength(5000);

                // FK tới User: RESTRICT để tránh multiple cascade paths
                e.HasOne(p => p.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(g => g.Group)
                    .WithMany(g => g.Posts)
                    .HasForeignKey(g => g.GroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(x => new { x.UserId, x.CreatedAt });
                e.HasIndex(x => x.GroupId);
            });

            // ======================
            // COMMENT
            // ======================
            model.Entity<Comment>(e =>
            {
                e.ToTable("Comments");
                e.HasKey(x => x.Id);

                e.Property(x => x.Content).HasMaxLength(2000).IsRequired();

                // Xoá Post thì xoá Comment (cascade trong phạm vi Post)
                e.HasOne(x => x.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(x => x.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Với User đặt RESTRICT
                e.HasOne(x => x.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.PostId, x.CreatedAt });
            });

            // ======================
            // MESSAGE
            // ======================
            model.Entity<Message>(e =>
            {
                e.ToTable("Messages");
                e.HasKey(x => x.Id);

                e.Property(x => x.Content).HasMaxLength(5000).IsRequired();

                e.HasOne(x => x.Sender)
                    .WithMany(u => u.SentMessages)
                    .HasForeignKey(x => x.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Receiver)
                    .WithMany(u => u.ReceivedMessages)
                    .HasForeignKey(x => x.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.SenderId, x.ReceiverId, x.SentAt });
            });

            // ======================
            // NOTIFICATION
            // ======================
            model.Entity<Notification>(e =>
            {
                e.ToTable("Notifications");
                e.HasKey(x => x.Id);

                e.Property(x => x.Message).HasMaxLength(2000).IsRequired();
                e.Property(x => x.Type).IsRequired();

                e.HasOne(x => x.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // tránh multiple cascade

                e.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
            });

            // ======================
            // REPORT
            // ======================
            model.Entity<Report>(e =>
            {
                e.ToTable("Reports");
                e.HasKey(x => x.Id);

                e.Property(x => x.Reason).HasMaxLength(1000).IsRequired();

                // Bạn đang dùng FK scalar (PostId, ReporterId) không có nav -> để nguyên
                e.HasIndex(x => new { x.PostId, x.CreatedAt });
            });

            // ======================
            // FRIEND (2 FK tới Users → NO ACTION cho cả hai)
            // ======================
            model.Entity<Friend>(e =>
            {
                e.ToTable("Friends");
                e.HasKey(x => x.Id);

                e.Property(x => x.Status).HasMaxLength(32).IsRequired();

                e.HasOne(x => x.User)
                    .WithMany(u => u.Friends)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(x => x.FriendUser)
                    .WithMany()
                    .HasForeignKey(x => x.FriendId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasIndex(x => new { x.UserId, x.FriendId }).IsUnique();
            });

            // ======================
            // POST VOTE
            // ======================
            model.Entity<PostVote>(e =>
            {
                e.ToTable("PostVotes");
                e.HasKey(x => x.Id);

                e.HasIndex(v => new { v.PostId, v.UserId }).IsUnique();

                e.HasOne(x => x.Post)
                    .WithMany(p => p.PostVotes)
                    .HasForeignKey(x => x.PostId)
                    .OnDelete(DeleteBehavior.Cascade);   // xoá Post xoá luôn Vote

                e.HasOne(x => x.User)
                    .WithMany(u => u.PostVotes)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);  // tránh multiple cascade
            });

            // ======================
            // TAG
            // ======================
            model.Entity<Tag>(e =>
            {
                e.ToTable("Tags");
                e.HasKey(x => x.Id);

                e.Property(x => x.Name).HasMaxLength(64).IsRequired();
                e.HasIndex(x => x.Name).IsUnique();
            });

            // ======================
            // POSTTAG (N-N)
            // ======================
            model.Entity<PostTag>(e =>
            {
                e.ToTable("PostTags");
                e.HasKey(pt => new { pt.PostId, pt.TagId });

                e.HasOne(pt => pt.Post)
                    .WithMany(p => p.PostTags)
                    .HasForeignKey(pt => pt.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(pt => pt.Tag)
                    .WithMany(t => t.PostTags)
                    .HasForeignKey(pt => pt.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================
            // MEDIA
            // ======================
            model.Entity<Media>(e =>
            {
                e.ToTable("Media");
                e.HasKey(x => x.Id);

                e.Property(x => x.Path).HasMaxLength(512).IsRequired();
                e.Property(x => x.MediaType).HasMaxLength(32).IsRequired();

                e.HasOne(x => x.Post)
                    .WithMany(p => p.MediaItems)
                    .HasForeignKey(x => x.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => new { x.PostId, x.CreatedAt });
            });

            // ======================
            // GROUP
            // ======================
            model.Entity<Group>(e =>
            {
                e.ToTable("Groups");
                e.HasKey(x => x.Id);

                e.Property(x => x.GroupName).HasMaxLength(128).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired(false);

                e.HasOne(g => g.Owner)
                .WithMany(u => u.OwnedGroups)
                .HasForeignKey(g => g.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================
            // GROUP DETAIL
            // ======================
            model.Entity<GroupDetail>(e =>
            {
                e.ToTable("GroupDetails");
                e.HasKey(x => x.Id); // hoặc composite if you want

                e.HasOne(gd => gd.Group)
                 .WithMany(g => g.Members)
                 .HasForeignKey(gd => gd.GroupId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(gd => gd.User)
                 .WithMany(u => u.GroupDetails)
                 .HasForeignKey(gd => gd.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.GroupId, x.UserId }).IsUnique();
            });

            // ======================
            // IGNORE các model không phải entity DB
            // ======================
            model.Ignore<Conversation>();
            model.Ignore<ConversationDto>();
        }
    }
}
