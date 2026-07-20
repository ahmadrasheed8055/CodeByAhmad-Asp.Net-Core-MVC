using FitMind_API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Data
{
    public class FMDBContext : DbContext
    {
        public FMDBContext(DbContextOptions options) : base(options)
        {
        }

        //tables
        public DbSet<Categories> Categories { get; set; }
        public DbSet<UserRT> UserRegistrationTokens { get; set; }
        public DbSet<AppUsers> AppUsers { get; set; }

        public DbSet<AddPost> AddPosts { get; set; }
        public DbSet<PostComments> PostComments { get; set; }

        public DbSet<PostReactions> PostReactions { get; set; }

        // NEW: Comment reactions
        public DbSet<CommentReactions> CommentReactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRT>()
                .HasOne(ut => ut.AppUser)
                .WithMany(u => u.UserTokens)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Still fine

            modelBuilder.Entity<PostComments>()
                .HasOne(pc => pc.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(pc => pc.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade Post -> Comments

            modelBuilder.Entity<PostComments>()
                .HasOne(pc => pc.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.Restrict); // IMPORTANT: NO CASCADE from User -> Comments

            modelBuilder.Entity<PostReactions>()
                .HasOne(pr => pr.Post)
                .WithMany(p => p.postReactions)
                .HasForeignKey(pr => pr.PostId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade Post -> Likes

            modelBuilder.Entity<PostReactions>()
                .HasOne(pl => pl.User)
                .WithMany(u => u.postReactions)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Restrict); // IMPORTANT: NO CASCADE from User -> Likes

            // NEW: CommentReactions relationships (no navigation collections required on other entities)
            modelBuilder.Entity<PostComments>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict); // Important: Prevent multiple cascade paths in SQL Server. We will handle recursive delete in code.

            modelBuilder.Entity<CommentReactions>()
                .HasOne(cr => cr.Comment)
                .WithMany(c => c.CommentReactions)
                .HasForeignKey(cr => cr.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommentReactions>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}