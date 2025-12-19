using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Deck.Controllers;
using Deck.DTOs;
using Deck.Services;
using Xunit;

namespace VortexTCG.Tests.Api.Deck.Controllers
{
    public class DeckControllerTest
    {
        private DeckController CreateController()
        {
            return new DeckController();
        }

        [Fact]
        public async Task GetDeckById_ReturnsOk_WithMockDeck()
        {
            // Arrange
            DeckController controller = CreateController();
            string testDeckId = "deck42";

            // Act
            ActionResult<DeckDTO> result = controller.GetDeckById(testDeckId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            DeckDTO deck = Assert.IsType<DeckDTO>(okResult.Value);
            Assert.Equal(testDeckId, deck.Id);
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
