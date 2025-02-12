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
    }
}
