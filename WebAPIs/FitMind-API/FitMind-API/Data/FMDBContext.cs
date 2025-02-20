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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserRT>()
                .HasOne(ut => ut.AppUser)
                .WithMany(u => u.UserTokens)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
