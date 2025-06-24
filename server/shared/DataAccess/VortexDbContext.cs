using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.DataAccess
{
    public class VortexDbContext : DbContext
    {
        public VortexDbContext(DbContextOptions<VortexDbContext> options) : base(options)
        {
            
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<User> Deck { get; set; }
        public DbSet<User> Rank { get; set; }
        public DbSet<User> Role { get; set; }
        public DbSet<User> Game { get; set; }
        public DbSet<User> Collection { get; set; }
        public DbSet<User> Booster { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}