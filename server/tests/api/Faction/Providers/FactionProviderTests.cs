using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Faction.DTOs;
using VortexTCG.Faction.Providers;
using Xunit;

using CardModel = VortexTCG.DataAccess.Models.Card;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using FactionCardModel = VortexTCG.DataAccess.Models.FactionCard;
using ChampionModel = VortexTCG.DataAccess.Models.Champion;

namespace VortexTCG.Tests.Api.Faction.Providers
{
    public class FactionProviderTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static CardModel CreateCard(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
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

        private static FactionModel CreateFaction(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Label = "Faction",
            Currency = "Gold",
            Condition = "None",
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "test"
        };

        [Fact]
        public async Task ValidateCardIds_EmptyList_IsValid()
        {
            using VortexDbContext db = CreateDb();
            FactionProvider provider = new FactionProvider(db);

            (bool isValid, List<Guid> invalid) = await provider.ValidateCardIds(new List<Guid>());

            Assert.True(isValid);
            Assert.Empty(invalid);
        }

        [Fact]
        public async Task ValidateCardIds_InvalidIds_ReturnsFalse()
        {
            using VortexDbContext db = CreateDb();
            FactionProvider provider = new FactionProvider(db);
            Guid missingId = Guid.NewGuid();

            (bool isValid, List<Guid> invalid) = await provider.ValidateCardIds(new List<Guid> { missingId });

            Assert.False(isValid);
            Assert.Contains(missingId, invalid);
        }

        [Fact]
        public async Task ValidateCardIds_ValidIds_ReturnsTrue()
        {
            using VortexDbContext db = CreateDb();
            CardModel card = CreateCard();
            db.Cards.Add(card);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);

            (bool isValid, List<Guid> invalid) = await provider.ValidateCardIds(new List<Guid> { card.Id });

            Assert.True(isValid);
            Assert.Empty(invalid);
        }

        [Fact]
        public async Task ValidateChampionId_Empty_ReturnsTrue()
        {
            using VortexDbContext db = CreateDb();
            FactionProvider provider = new FactionProvider(db);

            bool result = await provider.ValidateChampionId(Guid.Empty);

            Assert.True(result);
        }

        [Fact]
        public async Task ValidateChampionId_NotFound_ReturnsFalse()
        {
            using VortexDbContext db = CreateDb();
            FactionProvider provider = new FactionProvider(db);

            bool result = await provider.ValidateChampionId(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task CreateFaction_InvalidCardIds_ReturnsError()
        {
            using VortexDbContext db = CreateDb();
            FactionProvider provider = new FactionProvider(db);
            Guid missingId = Guid.NewGuid();
            CreateFactionDto dto = new CreateFactionDto
            {
                Label = "New Faction",
                Currency = "Gold",
                Condition = "None",
                CardIds = new List<Guid> { missingId }
            };

            (bool success, FactionDto? result, string error) = await provider.CreateFaction(dto);

            Assert.False(success);
            Assert.Null(result);
            Assert.Contains(missingId.ToString(), error);
        }

        [Fact]
        public async Task CreateFaction_InvalidChampion_ReturnsError()
        {
            using VortexDbContext db = CreateDb();
            FactionProvider provider = new FactionProvider(db);
            Guid missingChampion = Guid.NewGuid();
            CreateFactionDto dto = new CreateFactionDto
            {
                Label = "FactionWithChampion",
                Currency = "Gold",
                Condition = "None",
                ChampionId = missingChampion
            };

            (bool success, FactionDto? result, string error) = await provider.CreateFaction(dto);

            Assert.False(success);
            Assert.Null(result);
            Assert.Contains(missingChampion.ToString(), error);
        }

        [Fact]
        public async Task CreateFaction_WithCards_PersistsFactionAndLinks()
        {
            using VortexDbContext db = CreateDb();
            CardModel card = CreateCard();
            db.Cards.Add(card);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);
            CreateFactionDto dto = new CreateFactionDto
            {
                Label = "New Faction",
                Currency = "Gold",
                Condition = "None",
                CardIds = new List<Guid> { card.Id }
            };

            (bool success, FactionDto? result, string error) = await provider.CreateFaction(dto);

            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal("New Faction", result!.Label);
            Assert.Equal(1, db.Factions.Count());
            Assert.Equal(1, db.FactionCards.Count());
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public async Task CreateFaction_WithChampion_AssignsChampion()
        {
            using VortexDbContext db = CreateDb();
            ChampionModel champion = new ChampionModel
            {
                Id = Guid.NewGuid(),
                Name = "ChampionA",
                Description = "desc",
                HP = 10,
                Picture = "pic",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            db.Champions.Add(champion);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);
            CreateFactionDto dto = new CreateFactionDto
            {
                Label = "New Faction",
                Currency = "Gold",
                Condition = "None",
                ChampionId = champion.Id
            };

            (bool success, FactionDto? result, string error) = await provider.CreateFaction(dto);

            Assert.True(success);
            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(error));
            ChampionModel? updated = await db.Champions.SingleAsync(ch => ch.Id == champion.Id);
            Assert.Equal(result!.Id, updated.FactionId);
        }

        [Fact]
        public async Task UpdateFaction_InvalidCardIds_ReturnsError()
        {
            using VortexDbContext db = CreateDb();
            FactionModel faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);
            UpdateFactionDto update = new UpdateFactionDto
            {
                CardIds = new List<Guid> { Guid.NewGuid() }
            };

            (bool success, FactionDto? result, string error) = await provider.UpdateFaction(faction.Id, update);

            Assert.False(success);
            Assert.Null(result);
            Assert.Contains("IDs de cartes", error);
        }

        [Fact]
        public async Task UpdateFaction_InvalidChampion_ReturnsError()
        {
            using VortexDbContext db = CreateDb();
            FactionModel faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);
            UpdateFactionDto update = new UpdateFactionDto
            {
                ChampionId = Guid.NewGuid()
            };

            (bool success, FactionDto? result, string error) = await provider.UpdateFaction(faction.Id, update);

            Assert.False(success);
            Assert.Null(result);
            Assert.Contains("champion", error, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateFaction_UpdatesFieldsAndCards()
        {
            using VortexDbContext db = CreateDb();
            FactionModel faction = CreateFaction();
            CardModel oldCard = CreateCard();
            CardModel newCard = CreateCard();
            db.Factions.Add(faction);
            db.Cards.AddRange(oldCard, newCard);
            db.FactionCards.Add(new FactionCardModel
            {
                Id = Guid.NewGuid(),
                FactionId = faction.Id,
                CardId = oldCard.Id,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            });
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);
            UpdateFactionDto update = new UpdateFactionDto
            {
                Label = "Updated",
                Currency = "Silver",
                Condition = "UpdatedCond",
                CardIds = new List<Guid> { newCard.Id }
            };

            (bool success, FactionDto? result, string error) = await provider.UpdateFaction(faction.Id, update);

            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal("Updated", result!.Label);
            Assert.Equal("Silver", result.Currency);
            Assert.Equal("UpdatedCond", result.Condition);
            Assert.Equal(1, db.FactionCards.Count());
            Assert.Equal(newCard.Id, db.FactionCards.Single().CardId);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public async Task UpdateFaction_ChangesChampionAssignments()
        {
            using VortexDbContext db = CreateDb();
            FactionModel faction = CreateFaction();
            ChampionModel oldChampion = new ChampionModel
            {
                Id = Guid.NewGuid(),
                Name = "Old",
                Description = "desc",
                HP = 10,
                Picture = "pic",
                FactionId = faction.Id,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            ChampionModel newChampion = new ChampionModel
            {
                Id = Guid.NewGuid(),
                Name = "New",
                Description = "desc",
                HP = 12,
                Picture = "pic",
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            };
            db.Factions.Add(faction);
            db.Champions.AddRange(oldChampion, newChampion);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);
            UpdateFactionDto update = new UpdateFactionDto
            {
                ChampionId = newChampion.Id
            };

            (bool success, FactionDto? result, string error) = await provider.UpdateFaction(faction.Id, update);

            Assert.True(success);
            Assert.NotNull(result);
            ChampionModel? refreshedOld = await db.Champions.SingleAsync(ch => ch.Id == oldChampion.Id);
            ChampionModel? refreshedNew = await db.Champions.SingleAsync(ch => ch.Id == newChampion.Id);
            Assert.Equal(Guid.Empty, refreshedOld.FactionId);
            Assert.Equal(faction.Id, refreshedNew.FactionId);
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public async Task DeleteFaction_NotFound_ReturnsFalse()
        {
            using VortexDbContext db = CreateDb();
            FactionProvider provider = new FactionProvider(db);

            bool result = await provider.DeleteFaction(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFaction_RemovesFaction()
        {
            using VortexDbContext db = CreateDb();
            FactionModel faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);

            bool result = await provider.DeleteFaction(faction.Id);

            Assert.True(result);
            Assert.Empty(db.Factions);
        }

        [Fact]
        public async Task FactionExists_ReturnsExpectedValue()
        {
            using VortexDbContext db = CreateDb();
            FactionModel faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);

            Assert.True(await provider.FactionExists(faction.Id));
            Assert.False(await provider.FactionExists(Guid.NewGuid()));
        }

        [Fact]
        public async Task LabelExists_HonorsExclusion()
        {
            using VortexDbContext db = CreateDb();
            FactionModel faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            FactionProvider provider = new FactionProvider(db);

            Assert.True(await provider.LabelExists(faction.Label));
            Assert.False(await provider.LabelExists(faction.Label, faction.Id));
        }
    }
}
