using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Providers;
using VortexTCG.Api.Deck.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;
using DeckModel = VortexTCG.DataAccess.Models.Deck;
using CardModel = VortexTCG.DataAccess.Models.Card;
using CollectionCardModel = VortexTCG.DataAccess.Models.CollectionCard;
using DeckCardModel = VortexTCG.DataAccess.Models.DeckCard;
using ChampionModel = VortexTCG.DataAccess.Models.Champion;
using FactionModel = VortexTCG.DataAccess.Models.Faction;

namespace VortexTCG.Tests.Api.Deck.Services
{
    public class DeckServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static VortexDbContext CreateSqliteDb()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using (SqliteCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA foreign_keys = OFF;";
                cmd.ExecuteNonQuery();
            }
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseSqlite(connection)
                .Options;
            VortexDbContext db = new VortexDbContext(options);
            db.Database.EnsureCreated();
            return db;
        }

        private static DeckService CreateService(VortexDbContext db)
        {
            DeckProvider provider = new DeckProvider(db);
            return new DeckService(provider);
        }

        [Fact]
        public async Task GetDeckDataAsync_Returns404_WhenDeckNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<DeckDataDto> result = await service.GetDeckDataAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Equal("Deck not found", result.message);
        }

        [Fact]
        public async Task GetDeckDataAsync_Returns200_WithData_WhenFound()
        {
            using VortexDbContext db = CreateDb();

            ChampionModel champion = new ChampionModel { Id = Guid.NewGuid(), Name = "Hero", Description = "d", HP = 30, Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CardModel card = new CardModel { Id = Guid.NewGuid(), Name = "Sword", Price = 5, Description = "d", Picture = "p", Cost = 2, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CollectionCardModel collCard = new CollectionCardModel { Id = Guid.NewGuid(), CardId = card.Id, Quantity = 1, Rarity = Rarity.RARE, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "Hero Deck", ChampionId = champion.Id, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckCardModel deckCard = new DeckCardModel { Id = Guid.NewGuid(), DeckId = deck.Id, CardId = collCard.Id, Quantity = 2, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            db.Champions.Add(champion);
            db.Cards.Add(card);
            db.CollectionCards.Add(collCard);
            db.Decks.Add(deck);
            db.DeckCards.Add(deckCard);
            await db.SaveChangesAsync();

            DeckService service = CreateService(db);

            ResultDTO<DeckDataDto> result = await service.GetDeckDataAsync(deck.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Single(result.data!.Cards);
            Assert.Equal("Sword", result.data.Cards[0].Name);
            Assert.Equal("Hero", result.data.Champion.Name);
        }

        [Fact]
        public async Task CreateAsync_Returns400_WhenLabelEmpty()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<DeckResponseDto> result = await service.CreateAsync(new CreateDeckDto { Label = "", UserId = Guid.NewGuid(), ChampionId = Guid.NewGuid() });

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Equal("Label requis", result.message);
        }

        [Fact]
        public async Task CreateAsync_Returns400_WhenUserIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<DeckResponseDto> result = await service.CreateAsync(new CreateDeckDto { Label = "My Deck", UserId = Guid.Empty, ChampionId = Guid.NewGuid() });

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Equal("UserId requis", result.message);
        }

        [Fact]
        public async Task CreateAsync_Returns400_WhenChampionIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<DeckResponseDto> result = await service.CreateAsync(new CreateDeckDto { Label = "My Deck", UserId = Guid.NewGuid(), ChampionId = Guid.Empty });

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Equal("ChampionId requis", result.message);
        }

        [Fact]
        public async Task CreateAsync_Returns201_AndPersistsDeck_WhenValid()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            Guid userId = Guid.NewGuid();
            Guid championId = Guid.NewGuid();
            Guid factionId = Guid.NewGuid();

            ResultDTO<DeckResponseDto> result = await service.CreateAsync(new CreateDeckDto
            {
                Label = "Hero Deck",
                UserId = userId,
                ChampionId = championId,
                FactionId = factionId
            });

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Hero Deck", result.data!.Label);
            Assert.Equal(userId, result.data.UserId);
            Assert.Equal(championId, result.data.ChampionId);
            Assert.Equal(factionId, result.data.FactionId);
            Assert.NotEqual(Guid.Empty, result.data.Id);

            bool persisted = await db.Decks.AnyAsync(d => d.Id == result.data.Id);
            Assert.True(persisted);
        }

        [Fact]
        public async Task DeleteAsync_Returns404_WhenDeckNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<bool> result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Equal("Deck non trouvé", result.message);
        }

        [Fact]
        public async Task DeleteAsync_Returns204_AndRemovesDeck_WhenFound()
        {
            using VortexDbContext db = CreateDb();

            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "To Delete", ChampionId = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            DeckService service = CreateService(db);

            ResultDTO<bool> result = await service.DeleteAsync(deck.Id);

            Assert.True(result.success);
            Assert.Equal(204, result.statusCode);
            Assert.True(result.data);

            bool stillExists = await db.Decks.AnyAsync(d => d.Id == deck.Id);
            Assert.False(stillExists);
        }

        [Fact]
        public async Task UpdateDeckAsync_Returns404_WhenDeckNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<bool> result = await service.UpdateDeckAsync(Guid.NewGuid(), new UpdateDeckDto());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task UpdateDeckAsync_UpdatesFields_AndReturns200()
        {
            using VortexDbContext db = CreateSqliteDb();

            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "Old", ChampionId = Guid.NewGuid(), FactionId = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            DeckService service = CreateService(db);

            ResultDTO<bool> result = await service.UpdateDeckAsync(deck.Id, new UpdateDeckDto
            {
                Name = "New Name",
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid(),
                Cards = new List<UpdateDeckCardDto>()
            });

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }

        [Fact]
        public async Task UpdateDeckAsync_SkipsEmptyFields_WhenNotProvided()
        {
            using VortexDbContext db = CreateSqliteDb();

            Guid originalChampion = Guid.NewGuid();
            Guid originalFaction = Guid.NewGuid();
            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "Original", ChampionId = originalChampion, FactionId = originalFaction, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            DeckService service = CreateService(db);

            ResultDTO<bool> result = await service.UpdateDeckAsync(deck.Id, new UpdateDeckDto
            {
                Name = "   ",
                ChampionId = Guid.Empty,
                FactionId = Guid.Empty,
                Cards = new List<UpdateDeckCardDto>()
            });

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            DeckModel? updated = await db.Decks.FindAsync(deck.Id);
            Assert.Equal("Original", updated!.Label);
            Assert.Equal(originalChampion, updated.ChampionId);
            Assert.Equal(originalFaction, updated.FactionId);
        }

        [Fact]
        public async Task GetDecksByUserIdAsync_ReturnsEmpty_WhenNoDecks()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<List<DeckDTO>> result = await service.GetDecksByUserIdAsync(Guid.NewGuid());

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Empty(result.data!);
        }

        [Fact]
        public async Task GetDecksByUserIdAsync_ReturnsMappedDecks_WithChampion()
        {
            using VortexDbContext db = CreateDb();

            Guid userId = Guid.NewGuid();
            ChampionModel champion = new ChampionModel { Id = Guid.NewGuid(), Name = "Hero", Description = "d", HP = 30, Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            FactionModel faction = new FactionModel { Id = Guid.NewGuid(), Label = "F", Currency = "G", Condition = "C", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "MyDeck", UserId = userId, ChampionId = champion.Id, FactionId = faction.Id, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            db.Champions.Add(champion);
            db.Factions.Add(faction);
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            DeckService service = CreateService(db);
            ResultDTO<List<DeckDTO>> result = await service.GetDecksByUserIdAsync(userId);

            Assert.True(result.success);
            Assert.Single(result.data!);
            Assert.Equal("MyDeck", result.data![0].Name);
            Assert.NotNull(result.data[0].Champion);
            Assert.Equal("Hero", result.data[0].Champion!.Name);
        }

        [Fact]
        public async Task GetDeckDataAsync_MapsClassesCorrectly_WhenClassExists()
        {
            using VortexDbContext db = CreateDb();

            ChampionModel champion = new ChampionModel { Id = Guid.NewGuid(), Name = "Hero", Description = "d", HP = 30, Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CardModel card = new CardModel { Id = Guid.NewGuid(), Name = "Axe", Price = 3, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            Class cls = new Class { Id = Guid.NewGuid(), Label = "Warrior", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            ClassCard classCard = new ClassCard { Id = Guid.NewGuid(), CardId = card.Id, ClassId = cls.Id, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CollectionCardModel collCard = new CollectionCardModel { Id = Guid.NewGuid(), CardId = card.Id, Quantity = 1, Rarity = Rarity.NORMAL, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "Warrior Deck", ChampionId = champion.Id, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckCardModel deckCard = new DeckCardModel { Id = Guid.NewGuid(), DeckId = deck.Id, CardId = collCard.Id, Quantity = 1, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            db.Champions.Add(champion);
            db.Cards.Add(card);
            db.Class.Add(cls);
            db.Set<ClassCard>().Add(classCard);
            db.CollectionCards.Add(collCard);
            db.Decks.Add(deck);
            db.DeckCards.Add(deckCard);
            await db.SaveChangesAsync();

            DeckService service = CreateService(db);

            ResultDTO<DeckDataDto> result = await service.GetDeckDataAsync(deck.Id);

            Assert.True(result.success);
            Assert.Single(result.data!.Cards[0].Classes);
            Assert.Equal("Warrior", result.data.Cards[0].Classes[0]);
        }
    }
}