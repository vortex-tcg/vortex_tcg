using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VortexTCG.Common.DTO;
using VortexTCG.Api.Deck.Controllers;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Interface;
using VortexTCG.Api.Deck.Services;
using Xunit;

namespace VortexTCG.Tests.Api.Deck.Controllers
{
    public class DeckControllerTest
    {
        private static DeckController CreateController(out Mock<IDeckService> deckServiceMock)
        {
            deckServiceMock = new Mock<IDeckService>();
            return new DeckController(deckServiceMock.Object);
        }

        [Fact]
        public async Task GetDeckById_ReturnsOk_WithMockDeck()
        {
            // Arrange
            DeckController controller = CreateController(out var deckServiceMock);
            string testDeckId = "deck42";
    
            // Act
            IActionResult result = await controller.GetDeckById(testDeckId);

            // Assert
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDTO>? response = Assert.IsType<ResultDTO<DeckDTO>>(okResult.Value);
            DeckDTO? deck = response?.data;
            Assert.NotNull(deck);
            Assert.Equal(testDeckId, deck!.Id);
            Assert.StartsWith("Mock Deck", deck.Name);
            Assert.NotNull(deck.Cards);
            Assert.Equal(30, deck.Cards.Count);
            foreach (VortexTCG.Api.Card.DTOs.CardDto card in deck.Cards)
            {
                Assert.False(string.IsNullOrWhiteSpace(card.Name));
                Assert.False(string.IsNullOrWhiteSpace(card.Description));
                Assert.True(card.Hp > 0);
                Assert.True(card.Attack > 0);
                Assert.True(card.Cost > 0);
                Assert.False(string.IsNullOrWhiteSpace(card.Picture));
                Assert.False(string.IsNullOrWhiteSpace(card.CardType));
                Assert.NotNull(card.Class);
            }
        }
    }
}
