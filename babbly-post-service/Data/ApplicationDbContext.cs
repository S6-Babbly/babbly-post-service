using babbly_post_service.Models;
using Microsoft.EntityFrameworkCore;

namespace babbly_post_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Post entity
            modelBuilder.Entity<Post>()
                .ToTable("posts");

            modelBuilder.Entity<Post>()
                .Property(p => p.Id)
                .HasColumnName("id");

            modelBuilder.Entity<Post>()
                .Property(p => p.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<Post>()
                .Property(p => p.Content)
                .HasColumnName("content");

            modelBuilder.Entity<Post>()
                .Property(p => p.Likes)
                .HasColumnName("likes");

            modelBuilder.Entity<Post>()
                .Property(p => p.CreatedAt)
                .HasColumnName("created_at");
        }
    }
} 