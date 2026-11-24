using EntityModels;
using Microsoft.EntityFrameworkCore;

namespace EntityContext
{
    public class ForumContext : DbContext
    {
        public ForumContext(DbContextOptions<ForumContext> options) : base(options){}

        public DbSet<Topic> Topics => Set<Topic>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Friendship> Friendships => Set<Friendship>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
        public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Topic)
                .WithMany()
                .HasForeignKey(p => p.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Like>(entity =>
            {
                entity.ToTable("Likes", table =>
                    table.HasCheckConstraint(
                        "CK_Likes_ExactlyOneTarget",
                        """
                        ([PostId] IS NOT NULL AND [CommentId] IS NULL)
                        OR
                        ([PostId] IS NULL AND [CommentId] IS NOT NULL)
                        """
                        ));

                entity.HasKey(l => l.Id);

                entity.Property(l => l.UserId)
                    .IsRequired();

                entity.Property(l => l.CreatedAt)
                    .IsRequired();

                entity.HasOne(l => l.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(l => l.PostId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Comment)
                    .WithMany(c => c.Likes)
                    .HasForeignKey(l => l.CommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(l => new { l.UserId, l.PostId })
                    .IsUnique()
                    .HasFilter("[PostId] IS NOT NULL");

                entity.HasIndex(l => new { l.UserId, l.CommentId })
                    .IsUnique()
                    .HasFilter("[CommentId] IS NOT NULL");
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.UserId)
                    .IsRequired();

                entity.HasIndex(p => p.UserId)
                    .IsUnique();

                entity.Property(p => p.FirstName)
                    .HasMaxLength(100);

                entity.Property(p => p.LastName)
                    .HasMaxLength(100);

                entity.Property(p => p.Address)
                    .HasMaxLength(200);

                entity.Property(p => p.PostalCode)
                    .HasMaxLength(20);

                entity.Property(p => p.City)
                    .HasMaxLength(100);

                entity.Property(p => p.ProfileText)
                    .HasMaxLength(5000);
            });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.ToTable("Friendships", table =>
                    table.HasCheckConstraint(
                        "CK_Friendships_DifferentUsers",
                        "[UserIdLow] <> [UserIdHigh]"
                    ));

                entity.Property(f => f.RequesterUserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(f => f.AddresseeUserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(f => f.UserIdLow)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(f => f.UserIdHigh)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasIndex(f => new
                {
                    f.UserIdLow,
                    f.UserIdHigh
                }).IsUnique();
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.SenderUserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(m => m.RecipientUserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(m => m.Subject)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(m => m.Body)
                    .IsRequired()
                    .HasMaxLength(5000);

                entity.Property(m => m.SentAt)
                    .IsRequired();

                entity.Property(m => m.IsRead)
                    .IsRequired();

                // Helps retrieve inbox messages and unread messages.
                entity.HasIndex(m => new
                {
                    m.RecipientUserId,
                    m.IsRead,
                    m.SentAt
                });

                // Helps retrieve sent messages.
                entity.HasIndex(m => new
                {
                    m.SenderUserId,
                    m.SentAt
                });
            });

            // Chat rooms
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(r => r.RoomType)
                    .IsRequired();

                entity.Property(r => r.CreatedByUserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(r => r.OwnerUserId)
                    .HasMaxLength(450);

                entity.Property(r => r.CreatedAt)
                    .IsRequired();

                // A topic can only have one chatroom.
                entity.HasIndex(r => r.TopicId)
                    .IsUnique()
                    .HasFilter("[TopicId] IS NOT NULL");

                // Topic relationship.
                entity.HasOne(r => r.Topic)
                    .WithOne()
                    .HasForeignKey<ChatRoom>(r => r.TopicId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Helpful when retrieving rooms by type.
                entity.HasIndex(r => r.RoomType);
            });

            // Chat room members
            modelBuilder.Entity<ChatRoomMember>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(m => m.JoinedAt)
                    .IsRequired();

                entity.HasOne(m => m.ChatRoom)
                    .WithMany(r => r.Members)
                    .HasForeignKey(m => m.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                // A user can only belong to a room once.
                entity.HasIndex(m => new
                {
                    m.ChatRoomId,
                    m.UserId
                }).IsUnique();

                // Find all rooms belonging to a user.
                entity.HasIndex(m => m.UserId);
            });

            // Chat messages
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.SenderUserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(m => m.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(m => m.SentAt)
                    .IsRequired();

                entity.HasOne(m => m.ChatRoom)
                    .WithMany(r => r.Messages)
                    .HasForeignKey(m => m.ChatRoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Supports retrieving the latest messages efficiently.
                entity.HasIndex(m => new
                {
                    m.ChatRoomId,
                    m.Id
                });
            });

            SeedData(modelBuilder);
        }
        private static void SeedData(ModelBuilder modelBuilder)
        {
            int topicId = 1;
            int postId = 1;
            int commentId = 1;

            modelBuilder.Entity<Topic>().HasData(
                new Topic
                {
                    Id = topicId,
                    Title = "Welcome to the forum",
                    CreatedByUserId = "seed-user",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = postId,
                    TopicId = topicId,
                    Content = "This is the first post",
                    CreatedByUserId = "seed-user",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    Id = commentId,
                    PostId = postId,
                    Content = "First comment!",
                    CreatedByUserId = "seed-user",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<ChatRoom>().HasData(
                new ChatRoom
                {
                    Id = 1,
                    Name = "General",
                    RoomType = ChatRoomType.Miscellaneous,
                    CreatedByUserId = "seed-user",
                    OwnerUserId = null,
                    TopicId = null,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ChatRoom
                {
                    Id = 2,
                    Name = "Introductions",
                    RoomType = ChatRoomType.Miscellaneous,
                    CreatedByUserId = "seed-user",
                    OwnerUserId = null,
                    TopicId = null,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ChatRoom
                {
                    Id = 3,
                    Name = "Off-topic",
                    RoomType = ChatRoomType.Miscellaneous,
                    CreatedByUserId = "seed-user",
                    OwnerUserId = null,
                    TopicId = null,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
             );
        }
    }
}
