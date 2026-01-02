using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Faction.DTOs;
using VortexTCG.Faction.Services;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using CardModel = VortexTCG.DataAccess.Models.Card;
using Xunit;

namespace VortexTCG.Tests.Api.Faction.Services
{
    public class FactionServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
        }

        [Fact]
        public async Task GetAllFactions_ReturnsSuccess()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var result = await service.GetAllFactions();

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
        }

        [Fact]
        public async Task GetAllFactions_ReturnsFactions()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Test Faction",
                Currency = "Gold",
                Condition = "None",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "Test",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "Test"
            };
            db.Factions.Add(faction);
            await db.SaveChangesAsync();

            var result = await service.GetAllFactions();

            Assert.True(result.success);
            Assert.Single(result.data!);
            Assert.Equal("Test Faction", result.data![0].Label);
        }

        [Fact]
        public async Task GetFactionById_ReturnsSuccess()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var factionId = Guid.NewGuid();
            var faction = new FactionModel
            {
                Id = factionId,
                Label = "Test",
                Currency = "Gold",
                Condition = "None",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "Test",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "Test"
            };
            db.Factions.Add(faction);
            await db.SaveChangesAsync();

            var result = await service.GetFactionById(factionId);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Test", result.data!.Label);
        }

        [Fact]
        public async Task GetFactionById_ReturnsNotFoundWhenMissing()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var result = await service.GetFactionById(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task GetFactionWithCardsById_ReturnsSuccess()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var factionId = Guid.NewGuid();
            var faction = new FactionModel
            {
                Id = factionId,
                Label = "Test",
                Currency = "Gold",
                Condition = "None",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "Test",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "Test"
            };
            db.Factions.Add(faction);
            await db.SaveChangesAsync();

            var result = await service.GetFactionWithCardsById(factionId);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
        }

        [Fact]
        public async Task GetFactionWithCardsById_ReturnsNotFoundWhenMissing()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var result = await service.GetFactionWithCardsById(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task GetFactionWithChampionById_ReturnsSuccess()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var factionId = Guid.NewGuid();
            var faction = new FactionModel
            {
                Id = factionId,
                Label = "Test",
                Currency = "Gold",
                Condition = "None",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "Test",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "Test"
            };
            db.Factions.Add(faction);
            await db.SaveChangesAsync();

            var result = await service.GetFactionWithChampionById(factionId);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
        }

        [Fact]
        public async Task GetFactionWithChampionById_ReturnsNotFoundWhenMissing()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var result = await service.GetFactionWithChampionById(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task CreateFaction_ReturnsConflictWhenLabelExists()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            db.Factions.Add(new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Dup",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            });
            await db.SaveChangesAsync();
            var service = new FactionService(db, configuration);

            var dto = new CreateFactionDto { Label = "Dup", Currency = "Gold", Condition = "Cond" };

            var result = await service.CreateFaction(dto);

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task CreateFaction_InvalidIds_ReturnsBadRequest()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);
            var missingId = Guid.NewGuid();

            var dto = new CreateFactionDto
            {
                Label = "New",
                Currency = "Gold",
                Condition = "Cond",
                CardIds = new List<Guid> { missingId }
            };

            var result = await service.CreateFaction(dto);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains(missingId.ToString(), result.message);
        }

        [Fact]
        public async Task CreateFaction_SucceedsAndReturnsCreated()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var cardId = Guid.NewGuid();
            db.Cards.Add(new CardModel
            {
                Id = cardId,
                Name = "Card",
                Price = 1,
                Hp = 1,
                Attack = 1,
                Cost = 1,
                Description = "Desc",
                Picture = "pic",
                Extension = Extension.BASIC,
                CardType = CardType.GUARD,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            });
            await db.SaveChangesAsync();
            var service = new FactionService(db, configuration);

            var dto = new CreateFactionDto
            {
                Label = "Created",
                Currency = "Gold",
                Condition = "Cond",
                CardIds = new List<Guid> { cardId }
            };

            var result = await service.CreateFaction(dto);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Created", result.data!.Label);
        }

        [Fact]
        public async Task UpdateFaction_ReturnsNotFoundWhenMissing()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var result = await service.UpdateFaction(Guid.NewGuid(), new UpdateFactionDto { Label = "X" });

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task UpdateFaction_ReturnsConflictWhenLabelExists()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var first = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Existing",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            var target = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Target",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            db.Factions.AddRange(first, target);
            await db.SaveChangesAsync();
            var service = new FactionService(db, configuration);

            var result = await service.UpdateFaction(target.Id, new UpdateFactionDto { Label = "Existing" });

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task UpdateFaction_InvalidIds_ReturnsBadRequest()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Target",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            var service = new FactionService(db, configuration);

            var dto = new UpdateFactionDto { CardIds = new List<Guid> { Guid.NewGuid() } };

            var result = await service.UpdateFaction(faction.Id, dto);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task UpdateFaction_Succeeds()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Target",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            var card = new CardModel
            {
                Id = Guid.NewGuid(),
                Name = "Card",
                Price = 1,
                Hp = 1,
                Attack = 1,
                Cost = 1,
                Description = "Desc",
                Picture = "pic",
                Extension = Extension.BASIC,
                CardType = CardType.GUARD,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            db.Factions.Add(faction);
            db.Cards.Add(card);
            await db.SaveChangesAsync();
            var service = new FactionService(db, configuration);

            var dto = new UpdateFactionDto
            {
                Label = "Updated",
                Currency = "Silver",
                Condition = "NewCond",
                CardIds = new List<Guid> { card.Id }
            };

            var result = await service.UpdateFaction(faction.Id, dto);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("Updated", result.data!.Label);
        }

        [Fact]
        public async Task DeleteFaction_ReturnsNotFound()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var service = new FactionService(db, configuration);

            var result = await service.DeleteFaction(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task DeleteFaction_Succeeds()
        {
            using var db = CreateDb();
            var configuration = CreateConfiguration();
            var faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Target",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            var service = new FactionService(db, configuration);

            var result = await service.DeleteFaction(faction.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }
    }
}

