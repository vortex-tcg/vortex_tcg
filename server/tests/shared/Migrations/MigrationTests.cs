using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;
using FactionModel = VortexTCG.DataAccess.Models.Faction;

namespace VortexTCG.Tests.Migrations
{
    public class MigrationTests
    {
        private static VortexDbContext CreateDbContext()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        [Fact]
        public void DatabaseCanConnect()
        {
            using VortexDbContext db = CreateDbContext();

            bool canConnect = db.Database.CanConnect();

            Assert.True(canConnect);
        }

        [Fact]
        public void DatabaseEnsureCreated()
        {
            using VortexDbContext db = CreateDbContext();

            bool created = db.Database.EnsureCreated();

            Assert.True(created);
        }

        [Fact]
        public void DbContextCanPerformBasicOperations()
        {
            using VortexDbContext db = CreateDbContext();
            db.Database.EnsureCreated();

            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Shadow",
                Currency = "shadow_essence",
                Condition = "darker_the_better",
                Cards = new List<FactionCard>()
            };

            db.Factions.Add(faction);
            db.SaveChanges();

            FactionModel retrieved = db.Factions.FirstOrDefault(f => f.Label == "Shadow");
            Assert.NotNull(retrieved);
            Assert.Equal("Shadow", retrieved.Label);
        }

        [Fact]
        public void UsersTableCanStoreMultipleRecords()
        {
            using VortexDbContext db = CreateDbContext();
            db.Database.EnsureCreated();

            Rank rank = new Rank
            {
                Id = Guid.NewGuid(),
                Label = "Bronze",
                nbVictory = 10,
                Users = new List<User>()
            };

            db.Ranks.Add(rank);
            db.SaveChanges();

            User user1 = new User
            {
                Id = Guid.NewGuid(),
                Email = "user1@test.com",
                Username = "User1",
                FirstName = "John",
                LastName = "Doe",
                Password = "hashed_password",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.DISCONNECTED,
                RankId = rank.Id,
                Rank = rank
            };

            User user2 = new User
            {
                Id = Guid.NewGuid(),
                Email = "user2@test.com",
                Username = "User2",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "hashed_password",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.DISCONNECTED,
                RankId = rank.Id,
                Rank = rank
            };

            db.Users.AddRange(user1, user2);
            db.SaveChanges();

            List<User> users = db.Users.Where(u => u.RankId == rank.Id).ToList();
            Assert.Equal(2, users.Count);
        }

        [Fact]
        public void ForeignKeyRelationshipsWork()
        {
            using VortexDbContext db = CreateDbContext();
            db.Database.EnsureCreated();

            Rank rank = new Rank
            {
                Id = Guid.NewGuid(),
                Label = "Silver",
                nbVictory = 20,
                Users = new List<User>()
            };

            db.Ranks.Add(rank);
            db.SaveChanges();

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = "fk@test.com",
                Username = "FKUser",
                FirstName = "Test",
                LastName = "User",
                Password = "hashed",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.DISCONNECTED,
                RankId = rank.Id,
                Rank = rank
            };

            db.Users.Add(user);
            db.SaveChanges();

            User retrieved = db.Users.Include(u => u.Rank).FirstOrDefault(u => u.Id == user.Id);
            Assert.NotNull(retrieved);
            Assert.NotNull(retrieved.Rank);
            Assert.Equal("Silver", retrieved.Rank.Label);
        }

        [Fact]
        public void CardTableCanStoreRecords()
        {
            using VortexDbContext db = CreateDbContext();
            db.Database.EnsureCreated();

            Card card = new Card
            {
                Id = Guid.NewGuid(),
                Name = "Fireball",
                Price = 100,
                Hp = null,
                Attack = null,
                Cost = 5,
                Description = "Deal 3 damage to all enemies",
                Picture = "fireball.png",
                Extension = Extension.BASIC,
                CardType = CardType.SPELL,
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            db.Cards.Add(card);
            db.SaveChanges();

            Card retrieved = db.Cards.FirstOrDefault(c => c.Name == "Fireball");
            Assert.NotNull(retrieved);
            Assert.Equal(CardType.SPELL, retrieved.CardType);
        }

        [Fact]
        public void FactionWithMultipleCards()
        {
            using VortexDbContext db = CreateDbContext();
            db.Database.EnsureCreated();

            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Light",
                Currency = "holy_light",
                Condition = "enlightened",
                Cards = new List<FactionCard>()
            };

            db.Factions.Add(faction);
            db.SaveChanges();

            Card card1 = new Card
            {
                Id = Guid.NewGuid(),
                Name = "HolyBolt",
                Price = 80,
                Hp = null,
                Attack = null,
                Cost = 3,
                Description = "Heal 2 allies",
                Picture = "holy_bolt.png",
                Extension = Extension.BASIC,
                CardType = CardType.SPELL,
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            Card card2 = new Card
            {
                Id = Guid.NewGuid(),
                Name = "Paladin",
                Price = 120,
                Hp = 4,
                Attack = 3,
                Cost = 4,
                Description = "Holy defender",
                Picture = "paladin.png",
                Extension = Extension.BASIC,
                CardType = CardType.GUARD,
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            db.Cards.AddRange(card1, card2);
            db.SaveChanges();

            FactionCard factionCard1 = new FactionCard
            {
                Id = Guid.NewGuid(),
                CardId = card1.Id,
                Card = card1,
                FactionId = faction.Id,
                Faction = faction
            };

            FactionCard factionCard2 = new FactionCard
            {
                Id = Guid.NewGuid(),
                CardId = card2.Id,
                Card = card2,
                FactionId = faction.Id,
                Faction = faction
            };

            db.FactionCards.AddRange(factionCard1, factionCard2);
            db.SaveChanges();

            FactionModel retrieved = db.Factions.Include(f => f.Cards).FirstOrDefault(f => f.Id == faction.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(2, retrieved.Cards.Count);
        }

        [Fact]
        public void ChampionBelongsToFaction()
        {
            using VortexDbContext db = CreateDbContext();
            db.Database.EnsureCreated();

            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Darkness",
                Currency = "shadow_essence",
                Condition = "embrace_darkness",
                Cards = new List<FactionCard>()
            };

            db.Factions.Add(faction);
            db.SaveChanges();

            Champion champion = new Champion
            {
                Id = Guid.NewGuid(),
                Name = "Dark Knight",
                Description = "A dark warrior",
                HP = 8,
                Picture = "dark_knight.png",
                FactionId = faction.Id,
                Faction = faction,
                Collections = new List<CollectionChampion>()
            };

            db.Champions.Add(champion);
            db.SaveChanges();

            Champion retrieved = db.Champions.Include(c => c.Faction).FirstOrDefault(c => c.Id == champion.Id);
            Assert.NotNull(retrieved);
            Assert.NotNull(retrieved.Faction);
            Assert.Equal("Darkness", retrieved.Faction.Label);
        }

        [Fact]
        public void DeckContainsMultipleCards()
        {
            using VortexDbContext db = CreateDbContext();
            db.Database.EnsureCreated();

            Rank rank = new Rank
            {
                Id = Guid.NewGuid(),
                Label = "Gold",
                nbVictory = 50,
                Users = new List<User>()
            };

            db.Ranks.Add(rank);
            db.SaveChanges();

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = "deck@test.com",
                Username = "DeckUser",
                FirstName = "Deck",
                LastName = "User",
                Password = "hashed",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.DISCONNECTED,
                RankId = rank.Id,
                Rank = rank
            };

            db.Users.Add(user);
            db.SaveChanges();

            Champion champion = new Champion
            {
                Id = Guid.NewGuid(),
                Name = "Legendary",
                Description = "A powerful champion",
                HP = 10,
                Picture = "legend.png",
                Collections = new List<CollectionChampion>()
            };

            db.Champions.Add(champion);
            db.SaveChanges();

            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Neutral",
                Currency = "neutral_mana",
                Condition = "balanced",
                Cards = new List<FactionCard>()
            };

            db.Factions.Add(faction);
            db.SaveChanges();

            Deck deck = new Deck
            {
                Id = Guid.NewGuid(),
                Label = "Fire Deck",
                UserId = user.Id,
                Users = user,
                ChampionId = champion.Id,
                Champion = champion,
                FactionId = faction.Id,
                Faction = faction,
                DeckCard = new List<DeckCard>()
            };

            db.Decks.Add(deck);
            db.SaveChanges();

            Card card1 = new Card
            {
                Id = Guid.NewGuid(),
                Name = "Fireball",
                Price = 100,
                Hp = null,
                Attack = null,
                Cost = 5,
                Description = "Deal damage",
                Picture = "fireball.png",
                Extension = Extension.BASIC,
                CardType = CardType.SPELL,
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            Card card2 = new Card
            {
                Id = Guid.NewGuid(),
                Name = "Burn",
                Price = 80,
                Hp = null,
                Attack = null,
                Cost = 3,
                Description = "Deal more damage",
                Picture = "burn.png",
                Extension = Extension.BASIC,
                CardType = CardType.GUARD,
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            db.Cards.AddRange(card1, card2);
            db.SaveChanges();

            Collection collection = new Collection
            {
                Id = Guid.NewGuid(),
                User = user,
                Cards = new List<CollectionCard>(),
                Champions = new List<CollectionChampion>()
            };

            db.Collections.Add(collection);
            db.SaveChanges();

            CollectionCard collCard1 = new CollectionCard
            {
                Id = Guid.NewGuid(),
                CardId = card1.Id,
                Card = card1,
                Quantity = 2,
                Rarity = Rarity.NORMAL,
                CollectionId = collection.Id,
                Collection = collection,
                DeckCards = new List<DeckCard>()
            };

            CollectionCard collCard2 = new CollectionCard
            {
                Id = Guid.NewGuid(),
                CardId = card2.Id,
                Card = card2,
                Quantity = 3,
                Rarity = Rarity.RARE,
                CollectionId = collection.Id,
                Collection = collection,
                DeckCards = new List<DeckCard>()
            };

            db.CollectionCards.AddRange(collCard1, collCard2);
            db.SaveChanges();

            DeckCard deckCard1 = new DeckCard
            {
                Id = Guid.NewGuid(),
                CardId = collCard1.Id,
                Card = collCard1,
                DeckId = deck.Id,
                Deck = deck,
                Quantity = 2
            };

            DeckCard deckCard2 = new DeckCard
            {
                Id = Guid.NewGuid(),
                CardId = collCard2.Id,
                Card = collCard2,
                DeckId = deck.Id,
                Deck = deck,
                Quantity = 3
            };

            db.DeckCards.AddRange(deckCard1, deckCard2);
            db.SaveChanges();

            Deck retrievedDeck = db.Decks.Include(d => d.DeckCard).FirstOrDefault(d => d.Id == deck.Id);
            Assert.NotNull(retrievedDeck);
            Assert.Equal(2, retrievedDeck.DeckCard.Count);
        }
    }
}
