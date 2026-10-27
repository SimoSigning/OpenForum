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
        }
    }
}
