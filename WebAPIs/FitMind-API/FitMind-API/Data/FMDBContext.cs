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

        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<HiddenPost> HiddenPosts { get; set; }

        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Report> Reports { get; set; }

        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }

        public DbSet<AppNotification> AppNotifications { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }

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

            // Saved Post relations
            modelBuilder.Entity<SavedPost>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavedPost>()
                .HasOne(sp => sp.Post)
                .WithMany()
                .HasForeignKey(sp => sp.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Hidden Post relations
            modelBuilder.Entity<HiddenPost>()
                .HasOne(hp => hp.User)
                .WithMany()
                .HasForeignKey(hp => hp.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HiddenPost>()
                .HasOne(hp => hp.Post)
                .WithMany()
                .HasForeignKey(hp => hp.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Poll relations
            modelBuilder.Entity<Poll>()
                .HasOne(p => p.Post)
                .WithOne(ap => ap.Poll)
                .HasForeignKey<Poll>(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollOption>()
                .HasOne(po => po.Poll)
                .WithMany(p => p.Options)
                .HasForeignKey(po => po.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollVote>()
                .HasOne(pv => pv.Poll)
                .WithMany(p => p.Votes)
                .HasForeignKey(pv => pv.PollId)
                .OnDelete(DeleteBehavior.NoAction); // NO CASCADE from Poll -> Votes to prevent multiple cascade paths, or keep cascade and remove from User

            modelBuilder.Entity<PollVote>()
                .HasOne(pv => pv.Option)
                .WithMany(po => po.Votes)
                .HasForeignKey(pv => pv.OptionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PollVote>()
                .HasOne(pv => pv.User)
                .WithMany()
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PollVote>()
                .HasIndex(pv => new { pv.PollId, pv.UserId, pv.OptionId })
                .IsUnique();

            // Notification relations
            modelBuilder.Entity<AppNotification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.TargetUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserFollower relations
            modelBuilder.Entity<UserFollower>()
                .HasKey(uf => new { uf.FollowerId, uf.FollowingId });

            modelBuilder.Entity<UserFollower>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollower>()
                .HasOne(uf => uf.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Report relations
            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReporterUser)
                .WithMany()
                .HasForeignKey(r => r.ReporterUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // Trainer Specialization Category relation
            modelBuilder.Entity<AppUsers>()
                .HasOne(u => u.SpecializationCategory)
                .WithMany()
                .HasForeignKey(u => u.SpecializationCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}