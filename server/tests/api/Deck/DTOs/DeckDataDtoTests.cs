using VortexTCG.Api.Champion.DTOs;
using VortexTCG.Api.Deck.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Deck.DTOs
{
    public class DeckDataDtoTests
    {
        [Fact]
        public void DeckDataDto_CanSetAndGetAllProperties()
        {
            DeckCardDto card = new DeckCardDto { CardId = Guid.NewGuid(), Name = "TestCard" };
            DeckChampionDto champion = new DeckChampionDto { ChampionID = Guid.NewGuid(), Name = "Hero" };

            DeckDataDto dto = new DeckDataDto
            {
                Cards = new List<DeckCardDto> { card },
                Champion = champion
            };

            Assert.Single(dto.Cards);
            Assert.Equal("TestCard", dto.Cards[0].Name);
            Assert.Equal("Hero", dto.Champion.Name);
        }

        [Fact]
        public void DeckDataDto_DefaultCards_IsEmpty()
        {
            DeckDataDto dto = new DeckDataDto();

            Assert.NotNull(dto.Cards);
            Assert.Empty(dto.Cards);
        }
    }
}