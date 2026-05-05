using VortexTCG.Api.Deck.Providers;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;
using DeckModel = VortexTCG.DataAccess.Models.Deck;
using CardModel = VortexTCG.DataAccess.Models.Card;
using CollectionCardModel = VortexTCG.DataAccess.Models.CollectionCard;
using DeckCardModel = VortexTCG.DataAccess.Models.DeckCard;
using ChampionModel = VortexTCG.DataAccess.Models.Champion;
using ClassModel = VortexTCG.DataAccess.Models.Class;
using ClassCardModel = VortexTCG.DataAccess.Models.ClassCard;

namespace VortexTCG.Tests.Api.Deck.Providers
{
    public class DeckProviderTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        [Fact]
        public void GetMockDeck_Returns_DeckWith30Cards()
        {
            using VortexDbContext db = CreateDb();
            DeckProvider provider = new DeckProvider(db);

            var result = provider.GetMockDeck("test-deck-id");

            Assert.NotNull(result);
            Assert.Equal("test-deck-id", result.Id);
            Assert.Equal("Mock Deck test-deck-id", result.Name);
            Assert.Equal(30, result.Cards.Count);
            Assert.NotNull(result.Champion);
            Assert.Equal("Emporio PingChilling", result.Champion.Name);
        }

        [Fact]
        public async Task GetDeckWithCardsAndChampionAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckProvider provider = new DeckProvider(db);

            DeckModel? result = await provider.GetDeckWithCardsAndChampionAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDeckWithCardsAndChampionAsync_ReturnsDeck_WithRelations()
        {
            using VortexDbContext db = CreateDb();

            ChampionModel champion = new ChampionModel { Id = Guid.NewGuid(), Name = "Champ", Description = "d", HP = 30, Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CardModel card = new CardModel { Id = Guid.NewGuid(), Name = "Card", Price = 1, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CollectionCardModel collCard = new CollectionCardModel { Id = Guid.NewGuid(), CardId = card.Id, Quantity = 1, Rarity = Rarity.NORMAL, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "TestDeck", ChampionId = champion.Id, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckCardModel deckCard = new DeckCardModel { Id = Guid.NewGuid(), DeckId = deck.Id, CardId = collCard.Id, Quantity = 1, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            db.Champions.Add(champion);
            db.Cards.Add(card);
            db.CollectionCards.Add(collCard);
            db.Decks.Add(deck);
            db.DeckCards.Add(deckCard);
            await db.SaveChangesAsync();

            DeckProvider provider = new DeckProvider(db);
            DeckModel? result = await provider.GetDeckWithCardsAndChampionAsync(deck.Id);

            Assert.NotNull(result);
            Assert.Equal(deck.Id, result!.Id);
            Assert.NotNull(result.Champion);
            Assert.Equal("Champ", result.Champion.Name);
            Assert.Single(result.DeckCard);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckProvider provider = new DeckProvider(db);

            DeckModel? result = await provider.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsDeck_WhenFound()
        {
            using VortexDbContext db = CreateDb();

            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "Test", ChampionId = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            DeckProvider provider = new DeckProvider(db);
            DeckModel? result = await provider.GetByIdAsync(deck.Id);

            Assert.NotNull(result);
            Assert.Equal(deck.Id, result!.Id);
            Assert.Equal("Test", result.Label);
        }

        [Fact]
        public async Task AddAsync_PersistsDeck_AndReturnsIt()
        {
            using VortexDbContext db = CreateDb();
            DeckProvider provider = new DeckProvider(db);

            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "New Deck", UserId = Guid.NewGuid(), ChampionId = Guid.NewGuid(), FactionId = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            DeckModel result = await provider.AddAsync(deck);

            Assert.Equal(deck.Id, result.Id);
            Assert.NotNull(await db.Decks.FindAsync(deck.Id));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckProvider provider = new DeckProvider(db);

            bool result = await provider.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_AndRemovesDeck()
        {
            using VortexDbContext db = CreateDb();

            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "ToDelete", ChampionId = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            DeckProvider provider = new DeckProvider(db);
            bool result = await provider.DeleteAsync(deck.Id);

            Assert.True(result);
            Assert.Null(await db.Decks.FindAsync(deck.Id));
        }

        [Fact]
        public async Task GetClassRowsByCardIdsAsync_ReturnsEmpty_WhenNoMatch()
        {
            using VortexDbContext db = CreateDb();
            DeckProvider provider = new DeckProvider(db);

            List<(Guid CardId, string Label)> result = await provider.GetClassRowsByCardIdsAsync(new List<Guid> { Guid.NewGuid() });

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetClassRowsByCardIdsAsync_ReturnsRows_ForMatchingCards()
        {
            using VortexDbContext db = CreateDb();

            CardModel card = new CardModel { Id = Guid.NewGuid(), Name = "Card", Price = 1, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            ClassModel cls = new ClassModel { Id = Guid.NewGuid(), Label = "Warrior", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            ClassCardModel classCard = new ClassCardModel { Id = Guid.NewGuid(), CardId = card.Id, ClassId = cls.Id, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            db.Cards.Add(card);
            db.Class.Add(cls);
            db.Set<ClassCardModel>().Add(classCard);
            await db.SaveChangesAsync();

            DeckProvider provider = new DeckProvider(db);
            List<(Guid CardId, string Label)> result = await provider.GetClassRowsByCardIdsAsync(new List<Guid> { card.Id });

            Assert.Single(result);
            Assert.Equal(card.Id, result[0].CardId);
            Assert.Equal("Warrior", result[0].Label);
        }
    }
}