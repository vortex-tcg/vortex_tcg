using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Booster> Boosters { get; set; }
    public DbSet<FriendsList> FriendsLists { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relations personnalisées
        modelBuilder.Entity<Game>()
            .HasOne(g => g.CurrentPlayer)
            .WithMany(u => u.GamesAsCurrentPlayer)
            .HasForeignKey(g => g.CurrentPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasMany(g => g.Players)
            .WithMany(u => u.GamesAsPlayer);
    }
}
