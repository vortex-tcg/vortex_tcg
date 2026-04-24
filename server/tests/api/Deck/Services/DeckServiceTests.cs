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

namespace VortexTCG.Tests.Api.Deck.Services
{
    public class DeckServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static DeckService CreateService(VortexDbContext db)
        {
            DeckProvider provider = new DeckProvider(db);
            return new DeckService(provider);
        }

        [Fact]
        public void GetDeckById_Returns200_WithMockDeck()
        {
            using VortexDbContext db = CreateDb();
            DeckService service = CreateService(db);

            ResultDTO<DeckDTO> result = service.GetDeckById("my-deck-id");

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("my-deck-id", result.data!.Id);
            Assert.Equal(30, result.data.Cards.Count);
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