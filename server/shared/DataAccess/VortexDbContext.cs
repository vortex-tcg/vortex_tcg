using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.DataAccess
{
    public class VortexDbContext : DbContext
    {
        public VortexDbContext(DbContextOptions<VortexDbContext> options) : base(options)
        {
            
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
            
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Label = "Admin" },
                new Role { Id = 2, Label = "Player" }
            );
        }
    }
}