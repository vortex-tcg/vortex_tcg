using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Faction.DTOs;
using VortexTCG.Faction.Providers;
using Xunit;

using CardModel = VortexTCG.DataAccess.Models.Card;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using FactionCardModel = VortexTCG.DataAccess.Models.FactionCard;

namespace VortexTCG.Tests.Api.Faction.Providers
{
    public class FactionProviderTests
    {
        private static VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
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
            using var db = CreateDb();
            var provider = new FactionProvider(db);

            var (isValid, invalid) = await provider.ValidateCardIds(new List<Guid>());

            Assert.True(isValid);
            Assert.Empty(invalid);
        }

        [Fact]
        public async Task ValidateCardIds_InvalidIds_ReturnsFalse()
        {
            using var db = CreateDb();
            var provider = new FactionProvider(db);
            var missingId = Guid.NewGuid();

            var (isValid, invalid) = await provider.ValidateCardIds(new List<Guid> { missingId });

            Assert.False(isValid);
            Assert.Contains(missingId, invalid);
        }

        [Fact]
        public async Task ValidateCardIds_ValidIds_ReturnsTrue()
        {
            using var db = CreateDb();
            var card = CreateCard();
            db.Cards.Add(card);
            await db.SaveChangesAsync();
            var provider = new FactionProvider(db);

            var (isValid, invalid) = await provider.ValidateCardIds(new List<Guid> { card.Id });

            Assert.True(isValid);
            Assert.Empty(invalid);
        }

        [Fact]
        public async Task ValidateChampionId_Empty_ReturnsTrue()
        {
            using var db = CreateDb();
            var provider = new FactionProvider(db);

            var result = await provider.ValidateChampionId(Guid.Empty);

            Assert.True(result);
        }

        [Fact]
        public async Task ValidateChampionId_NotFound_ReturnsFalse()
        {
            using var db = CreateDb();
            var provider = new FactionProvider(db);

            var result = await provider.ValidateChampionId(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task CreateFaction_InvalidCardIds_ReturnsError()
        {
            using var db = CreateDb();
            var provider = new FactionProvider(db);
            var missingId = Guid.NewGuid();
            var dto = new CreateFactionDto
            {
                Label = "New Faction",
                Currency = "Gold",
                Condition = "None",
                CardIds = new List<Guid> { missingId }
            };

            var (success, result, error) = await provider.CreateFaction(dto);

            Assert.False(success);
            Assert.Null(result);
            Assert.Contains(missingId.ToString(), error);
        }

        [Fact]
        public async Task CreateFaction_WithCards_PersistsFactionAndLinks()
        {
            using var db = CreateDb();
            var card = CreateCard();
            db.Cards.Add(card);
            await db.SaveChangesAsync();
            var provider = new FactionProvider(db);
            var dto = new CreateFactionDto
            {
                Label = "New Faction",
                Currency = "Gold",
                Condition = "None",
                CardIds = new List<Guid> { card.Id }
            };

            var (success, result, error) = await provider.CreateFaction(dto);

            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal("New Faction", result!.Label);
            Assert.Equal(1, db.Factions.Count());
            Assert.Equal(1, db.FactionCards.Count());
            Assert.True(string.IsNullOrEmpty(error));
        }

        [Fact]
        public async Task UpdateFaction_InvalidCardIds_ReturnsError()
        {
            using var db = CreateDb();
            var faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            var provider = new FactionProvider(db);
            var update = new UpdateFactionDto
            {
                CardIds = new List<Guid> { Guid.NewGuid() }
            };

            var (success, result, error) = await provider.UpdateFaction(faction.Id, update);

            Assert.False(success);
            Assert.Null(result);
            Assert.Contains("IDs de cartes", error);
        }

        [Fact]
        public async Task UpdateFaction_UpdatesFieldsAndCards()
        {
            using var db = CreateDb();
            var faction = CreateFaction();
            var oldCard = CreateCard();
            var newCard = CreateCard();
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
            var provider = new FactionProvider(db);
            var update = new UpdateFactionDto
            {
                Label = "Updated",
                Currency = "Silver",
                Condition = "UpdatedCond",
                CardIds = new List<Guid> { newCard.Id }
            };

            var (success, result, error) = await provider.UpdateFaction(faction.Id, update);

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
        public async Task DeleteFaction_NotFound_ReturnsFalse()
        {
            using var db = CreateDb();
            var provider = new FactionProvider(db);

            var result = await provider.DeleteFaction(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteFaction_RemovesFaction()
        {
            using var db = CreateDb();
            var faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            var provider = new FactionProvider(db);

            var result = await provider.DeleteFaction(faction.Id);

            Assert.True(result);
            Assert.Empty(db.Factions);
        }

        [Fact]
        public async Task FactionExists_ReturnsExpectedValue()
        {
            using var db = CreateDb();
            var faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            var provider = new FactionProvider(db);

            Assert.True(await provider.FactionExists(faction.Id));
            Assert.False(await provider.FactionExists(Guid.NewGuid()));
        }

        [Fact]
        public async Task LabelExists_HonorsExclusion()
        {
            using var db = CreateDb();
            var faction = CreateFaction();
            db.Factions.Add(faction);
            await db.SaveChangesAsync();
            var provider = new FactionProvider(db);

            Assert.True(await provider.LabelExists(faction.Label));
            Assert.False(await provider.LabelExists(faction.Label, faction.Id));
        }
    }
}
