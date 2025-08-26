using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VortexTCG.DataAccess
{
    public class VortexDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public VortexDbContext(DbContextOptions<VortexDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<ActionType> ActionTypes { get; set; }
        public DbSet<Booster> Boosters { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<Champion> Champions { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<ConditionType> ConditionTypes { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<DeckCard> DeckCards { get; set; }
        public DbSet<EffectCard> EffectCards { get; set; }
        public DbSet<EffectChampion> EffectChampions { get; set; }
        public DbSet<EffectDescription> EffectDescriptions { get; set; }
        public DbSet<EffectType> EffectTypes { get; set; }
        public DbSet<Extension> Extensions { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<FriendsList> FriendsLists { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Gamelog> Gamelogs { get; set; }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<Rarity> Rarities { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Roles
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Label = "Admin",
                    CreatedBy = "System",
                    CreatedAtUtc = DateTime.UtcNow
                },
                new Role
                {
                    Id = 2,
                    Label = "User",
                    CreatedBy = "System",
                    CreatedAtUtc = DateTime.UtcNow
                }
            );

            //Ranks
            modelBuilder.Entity<Rank>().HasData(
                new Rank {
                    Id = 1,
                    Label = "Wood",
                    nbVictory = 0,
                    CreatedBy = "System",
                    CreatedAtUtc = DateTime.UtcNow
                }
            );

            //Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Username = "johndoe",
                    Email = "johndoe@gmail.com",
                    Password = "Mdp",
                    Language = "fr",
                    CurrencyQuantity = 1000,
                    RoleId = 2, // User
                    RankId = 1, // Wood
                    CreatedBy = "System",
                    CreatedAtUtc = DateTime.UtcNow
                }
            );


            // Configuration des propriétés d'audit pour les entités implémentant AuditableEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>("CreatedAt")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime?>("UpdatedAt");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<string>("CreatedBy");

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<string>("UpdatedBy");
                }
            }
        }

        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditFields()
        {
            var entries = ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in entries)
            {
                var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("CreatedBy").CurrentValue = currentUser;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdatedBy").CurrentValue = currentUser;
                }
            }
        }
    }
}
