using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Deck.Controllers;
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

namespace VortexTCG.Tests.Api.Deck.Controllers
{
    public class DeckControllerTest
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static DeckController CreateController(VortexDbContext db)
        {
            DeckProvider provider = new DeckProvider(db);
            DeckService service = new DeckService(provider);
            return new DeckController(service);
        }

        [Fact]
        public async Task GetDeckById_Returns200_WithMockDeck()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.GetDeckById("my-deck");

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDTO> payload = Assert.IsType<ResultDTO<DeckDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);
            Assert.NotNull(payload.data);
            Assert.Equal("my-deck", payload.data!.Id);
        }

        [Fact]
        public async Task GetDeckData_Returns404_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.GetDeckData(Guid.NewGuid());

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDataDto> payload = Assert.IsType<ResultDTO<DeckDataDto>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task GetDeckData_Returns200_WhenFound()
        {
            using VortexDbContext db = CreateDb();

            ChampionModel champion = new ChampionModel { Id = Guid.NewGuid(), Name = "King", Description = "d", HP = 40, Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CardModel card = new CardModel { Id = Guid.NewGuid(), Name = "Shield", Price = 2, Description = "d", Picture = "p", CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            CollectionCardModel collCard = new CollectionCardModel { Id = Guid.NewGuid(), CardId = card.Id, Quantity = 1, Rarity = Rarity.NORMAL, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "King Deck", ChampionId = champion.Id, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            DeckCardModel deckCard = new DeckCardModel { Id = Guid.NewGuid(), DeckId = deck.Id, CardId = collCard.Id, Quantity = 1, CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };

            db.Champions.Add(champion);
            db.Cards.Add(card);
            db.CollectionCards.Add(collCard);
            db.Decks.Add(deck);
            db.DeckCards.Add(deckCard);
            await db.SaveChangesAsync();

            DeckController controller = CreateController(db);

            IActionResult result = await controller.GetDeckData(deck.Id);

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDataDto> payload = Assert.IsType<ResultDTO<DeckDataDto>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);
            Assert.NotNull(payload.data);
        }
    }
}