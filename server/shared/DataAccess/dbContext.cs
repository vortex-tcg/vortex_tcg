using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class VortexDbContext : DbContext
    {
        public VortexDbContext(DbContextOptions<VortexDbContext> options) : base(options)
        {
            
        }
        
        public DbSet<Person> Persons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}