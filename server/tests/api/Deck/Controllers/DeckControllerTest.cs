using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using VortexTCG.Api.Deck.Controllers;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Interface;
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
        public async Task CreateDeck_Returns400_WhenLabelEmpty()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.CreateDeck(new CreateDeckDto { Label = "", UserId = Guid.NewGuid(), ChampionId = Guid.NewGuid() });

            ObjectResult response = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckResponseDto> payload = Assert.IsType<ResultDTO<DeckResponseDto>>(response.Value);
            Assert.False(payload.success);
            Assert.Equal(400, payload.statusCode);
        }

        [Fact]
        public async Task CreateDeck_Returns400_WhenUserIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.CreateDeck(new CreateDeckDto { Label = "Deck", UserId = Guid.Empty, ChampionId = Guid.NewGuid() });

            ObjectResult response = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckResponseDto> payload = Assert.IsType<ResultDTO<DeckResponseDto>>(response.Value);
            Assert.False(payload.success);
            Assert.Equal(400, payload.statusCode);
        }

        [Fact]
        public async Task CreateDeck_Returns201_WhenValid()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            Guid userId = Guid.NewGuid();
            Guid championId = Guid.NewGuid();

            IActionResult result = await controller.CreateDeck(new CreateDeckDto { Label = "My Deck", UserId = userId, ChampionId = championId });

            ObjectResult response = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckResponseDto> payload = Assert.IsType<ResultDTO<DeckResponseDto>>(response.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.NotNull(payload.data);
            Assert.Equal("My Deck", payload.data!.Label);
            Assert.Equal(userId, payload.data.UserId);
            Assert.NotEqual(Guid.Empty, payload.data.Id);
        }

        [Fact]
        public async Task DeleteDeck_Returns404_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.DeleteDeck(Guid.NewGuid());

            ObjectResult response = Assert.IsType<ObjectResult>(result);
            ResultDTO<bool> payload = Assert.IsType<ResultDTO<bool>>(response.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task DeleteDeck_Returns204_WhenFound()
        {
            using VortexDbContext db = CreateDb();

            DeckModel deck = new DeckModel { Id = Guid.NewGuid(), Label = "Deletable", ChampionId = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow, CreatedBy = "test" };
            db.Decks.Add(deck);
            await db.SaveChangesAsync();

            DeckController controller = CreateController(db);

            IActionResult result = await controller.DeleteDeck(deck.Id);

            ObjectResult response = Assert.IsType<ObjectResult>(result);
            ResultDTO<bool> payload = Assert.IsType<ResultDTO<bool>>(response.Value);
            Assert.True(payload.success);
            Assert.Equal(204, payload.statusCode);

            bool stillExists = await db.Decks.AnyAsync(d => d.Id == deck.Id);
            Assert.False(stillExists);
        }

        [Fact]
        public async Task CreateDeck_Returns400_WhenChampionIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.CreateDeck(new CreateDeckDto { Label = "Deck", UserId = Guid.NewGuid(), ChampionId = Guid.Empty });

            ObjectResult response = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckResponseDto> payload = Assert.IsType<ResultDTO<DeckResponseDto>>(response.Value);
            Assert.False(payload.success);
            Assert.Equal(400, payload.statusCode);
        }

        [Fact]
        public async Task GetDecksByUserId_Returns200_WithEmptyList()
        {
            Mock<IDeckService> mockService = new Mock<IDeckService>();
            mockService.Setup(s => s.GetDecksByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new ResultDTO<List<DeckDTO>> { success = true, statusCode = 200, data = new List<DeckDTO>() });
            DeckController controller = new DeckController(mockService.Object);

            IActionResult result = await controller.GetDecksByUserId(Guid.NewGuid());

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<List<DeckDTO>> payload = Assert.IsType<ResultDTO<List<DeckDTO>>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);
            Assert.Empty(payload.data!);
        }

        [Fact]
        public async Task UpdateDeck_Returns404_WhenNotFound()
        {
            Mock<IDeckService> mockService = new Mock<IDeckService>();
            mockService.Setup(s => s.UpdateDeckAsync(It.IsAny<Guid>(), It.IsAny<UpdateDeckDto>()))
                .ReturnsAsync(new ResultDTO<bool> { success = false, statusCode = 404, message = "Deck not found" });
            DeckController controller = new DeckController(mockService.Object);

            IActionResult result = await controller.UpdateDeck(Guid.NewGuid(), new UpdateDeckDto());

            ObjectResult response = Assert.IsType<ObjectResult>(result);
            ResultDTO<bool> payload = Assert.IsType<ResultDTO<bool>>(response.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task UpdateDeck_Returns200_WhenSuccess()
        {
            Mock<IDeckService> mockService = new Mock<IDeckService>();
            mockService.Setup(s => s.UpdateDeckAsync(It.IsAny<Guid>(), It.IsAny<UpdateDeckDto>()))
                .ReturnsAsync(new ResultDTO<bool> { success = true, statusCode = 200, data = true });
            DeckController controller = new DeckController(mockService.Object);

            IActionResult result = await controller.UpdateDeck(Guid.NewGuid(), new UpdateDeckDto());

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<bool> payload = Assert.IsType<ResultDTO<bool>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);
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