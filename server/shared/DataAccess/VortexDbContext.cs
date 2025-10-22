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

        public DbSet<Action> Actions { get; set; }
        public DbSet<Gamelog> Gamelogs { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set;  }
        public DbSet<Rank> Ranks { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<CollectionChampion> CollectionChampions { get; set; }
        public DbSet<Champion> Champions { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<CollectionCard> CollectionCards { get; set; }
        public DbSet<DeckCard> DeckCards { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Class> Class { get; set; }
        public DbSet<BoosterCard> BoosterCards { get; set; }
        public DbSet<Booster> Boosters { get; set; }
        public DbSet<EffectCard> EffectCards { get; set; }
        public DbSet<Effect> Effects { get; set; }
        public DbSet<EffectDescription> EffectDescriptions { get; set; }
        public DbSet<EffectType> EffectTypes { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<ConditionType> ConditionTypes { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Effect
            modelBuilder.Entity<Effect>()
                        .HasOne(c => c.StartCondition)
                        .WithMany(ct => ct.StartEffects)
                        .HasForeignKey(c => c.StartCondition);
            
            modelBuilder.Entity<Effect>()
                        .HasOne(c => c.EndCondition)
                        .WithMany(ct => ct.EndEffects)
                        .HasForeignKey(c => c.EndCondition);
            
            // Card
            modelBuilder.Entity<Card>()
                        .Property(p => p.Extension)
                        .HasConversion<string>()
                        .HasColumnType("varchar(256)");

            modelBuilder.Entity<Card>()
                        .Property(p => p.CardType)
                        .HasConversion<string>()
                        .HasColumnType("varchar(256)");

            // CollectionCard
            modelBuilder.Entity<CollectionCard>()
                        .Property(p => p.Rarity)
                        .HasConversion<string>()
                        .HasColumnType("varchar(256)");

            // Friend
            modelBuilder.Entity<Friend>()
                        .HasOne(c => c.FriendUser)
                        .WithMany(ct => ct.OtherFriends)
                        .HasForeignKey(c => c.FriendUserId);

            modelBuilder.Entity<Friend>()
                        .HasOne(c => c.User)
                        .WithMany(ct => ct.Friends)
                        .HasForeignKey(c => c.UserId);

            // User
            modelBuilder.Entity<User>()
                        .Property(p => p.Status)
                        .HasConversion<string>()
                        .HasColumnType("varchar(256)");
            
            modelBuilder.Entity<User>()
                        .Property(p => p.Role)
                        .HasConversion<string>()
                        .HasColumnType("varchar(256)");

            // Game
            modelBuilder.Entity<Game>()
                        .Property(p => p.Status)
                        .HasConversion<string>()
                        .HasColumnType("varchar(256)");

            // Action
            modelBuilder.Entity<ActionType>()
                        .HasOne(a => a.Parent)
                        .WithMany(a => a.Childs)
                        .HasForeignKey(a => a.ParentId);


            // Configuration des propriétés d'audit pour les entités implémentant AuditableEntity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime?>("CreatedAt")
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
