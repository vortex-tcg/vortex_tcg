using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Faction.DTOs;
using VortexTCG.Faction.Services;
using VortexTCG.Common.DTO;
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
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            ResultDTO<List<FactionDto>> result = await service.GetAllFactions();

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
        }

        [Fact]
        public async Task GetAllFactions_ReturnsFactions()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            FactionModel faction = new FactionModel
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

            ResultDTO<List<FactionDto>> result = await service.GetAllFactions();

            Assert.True(result.success);
            Assert.Single(result.data!);
            Assert.Equal("Test Faction", result.data![0].Label);
        }

        [Fact]
        public async Task GetFactionById_ReturnsSuccess()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            Guid factionId = Guid.NewGuid();
            FactionModel faction = new FactionModel
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

            ResultDTO<FactionDto> result = await service.GetFactionById(factionId);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Test", result.data!.Label);
        }

        [Fact]
        public async Task GetFactionById_ReturnsNotFoundWhenMissing()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            ResultDTO<FactionDto> result = await service.GetFactionById(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task GetFactionWithCardsById_ReturnsSuccess()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            Guid factionId = Guid.NewGuid();
            FactionModel faction = new FactionModel
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

            ResultDTO<FactionWithCardsDto> result = await service.GetFactionWithCardsById(factionId);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
        }

        [Fact]
        public async Task GetFactionWithCardsById_ReturnsNotFoundWhenMissing()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            ResultDTO<FactionWithCardsDto> result = await service.GetFactionWithCardsById(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task GetFactionWithChampionById_ReturnsSuccess()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            Guid factionId = Guid.NewGuid();
            FactionModel faction = new FactionModel
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

            ResultDTO<FactionWithChampionDto> result = await service.GetFactionWithChampionById(factionId);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
        }

        [Fact]
        public async Task GetFactionWithChampionById_ReturnsNotFoundWhenMissing()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            ResultDTO<FactionWithChampionDto> result = await service.GetFactionWithChampionById(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
            Assert.Null(result.data);
        }

        [Fact]
        public async Task CreateFaction_ReturnsConflictWhenLabelExists()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
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
            FactionService service = new FactionService(db, configuration);

            CreateFactionDto dto = new CreateFactionDto { Label = "Dup", Currency = "Gold", Condition = "Cond" };

            ResultDTO<FactionDto> result = await service.CreateFaction(dto);

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task CreateFaction_InvalidIds_ReturnsBadRequest()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);
            Guid missingId = Guid.NewGuid();

            CreateFactionDto dto = new CreateFactionDto
            {
                Label = "New",
                Currency = "Gold",
                Condition = "Cond",
                CardIds = new List<Guid> { missingId }
            };

            ResultDTO<FactionDto> result = await service.CreateFaction(dto);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains(missingId.ToString(), result.message);
        }

        [Fact]
        public async Task CreateFaction_SucceedsAndReturnsCreated()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            Guid cardId = Guid.NewGuid();
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
            FactionService service = new FactionService(db, configuration);

            CreateFactionDto dto = new CreateFactionDto
            {
                Label = "Created",
                Currency = "Gold",
                Condition = "Cond",
                CardIds = new List<Guid> { cardId }
            };

            ResultDTO<FactionDto> result = await service.CreateFaction(dto);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Created", result.data!.Label);
        }

        [Fact]
        public async Task UpdateFaction_ReturnsNotFoundWhenMissing()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            ResultDTO<FactionDto> result = await service.UpdateFaction(Guid.NewGuid(), new UpdateFactionDto { Label = "X" });

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task UpdateFaction_ReturnsConflictWhenLabelExists()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionModel first = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Existing",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            FactionModel target = new FactionModel
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
            FactionService service = new FactionService(db, configuration);

            ResultDTO<FactionDto> result = await service.UpdateFaction(target.Id, new UpdateFactionDto { Label = "Existing" });

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task UpdateFaction_InvalidIds_ReturnsBadRequest()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionModel faction = new FactionModel
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
            FactionService service = new FactionService(db, configuration);

            UpdateFactionDto dto = new UpdateFactionDto { CardIds = new List<Guid> { Guid.NewGuid() } };

            ResultDTO<FactionDto> result = await service.UpdateFaction(faction.Id, dto);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task UpdateFaction_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Target",
                Currency = "Gold",
                Condition = "Cond",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            CardModel card = new CardModel
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
            FactionService service = new FactionService(db, configuration);

            UpdateFactionDto dto = new UpdateFactionDto
            {
                Label = "Updated",
                Currency = "Silver",
                Condition = "NewCond",
                CardIds = new List<Guid> { card.Id }
            };

            ResultDTO<FactionDto> result = await service.UpdateFaction(faction.Id, dto);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("Updated", result.data!.Label);
        }

        [Fact]
        public async Task DeleteFaction_ReturnsNotFound()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionService service = new FactionService(db, configuration);

            ResultDTO<object> result = await service.DeleteFaction(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task DeleteFaction_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration configuration = CreateConfiguration();
            FactionModel faction = new FactionModel
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
            FactionService service = new FactionService(db, configuration);

            ResultDTO<object> result = await service.DeleteFaction(faction.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }
    }
}

