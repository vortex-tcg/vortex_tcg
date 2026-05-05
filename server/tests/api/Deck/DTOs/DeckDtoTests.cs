using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Champion.DTOs;
using VortexTCG.Api.Deck.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Deck.DTOs
{
    public class DeckDtoTests
    {
        [Fact]
        public void DeckDTO_CanSetAndGetAllProperties()
        {
            Guid champId = Guid.NewGuid();
            List<CardDto> cards = new List<CardDto> { new CardDto { Id = Guid.NewGuid(), Name = "Card1" } };
            DeckChampionDto champion = new DeckChampionDto { ChampionID = champId, Name = "Champ" };

            DeckDTO dto = new DeckDTO
            {
                Id = "deck-001",
                Name = "My Deck",
                Cards = cards,
                Champion = champion
            };

            Assert.Equal("deck-001", dto.Id);
            Assert.Equal("My Deck", dto.Name);
            Assert.Single(dto.Cards);
            Assert.Equal("Card1", dto.Cards[0].Name);
            Assert.Equal(champId, dto.Champion.ChampionID);
        }
    }
}