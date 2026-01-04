using VortexTCG.Api.Champion.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Champion.DTOs
{
    public class DeckChampionDtoTests
    {
        [Fact]
        public void DeckChampionDto_CanSetAndGetAllProperties()
        {
            Guid championId = Guid.NewGuid();
            Guid factionId = Guid.NewGuid();
            
            DeckChampionDto dto = new DeckChampionDto
            {
                ChampionID = championId,
                Name = "Dragon",
                Description = "Powerful dragon",
                HP = 250,
                Picture = "dragon.jpg",
                FactionId = factionId
            };

            Assert.Equal(championId, dto.ChampionID);
            Assert.Equal("Dragon", dto.Name);
            Assert.Equal("Powerful dragon", dto.Description);
            Assert.Equal(250, dto.HP);
            Assert.Equal("dragon.jpg", dto.Picture);
            Assert.Equal(factionId, dto.FactionId);
        }

        [Fact]
        public void DeckChampionDto_MultipleInstances_AreIndependent()
        {
            DeckChampionDto champ1 = new DeckChampionDto
            {
                ChampionID = Guid.NewGuid(),
                Name = "Champion1",
                HP = 100
            };

            DeckChampionDto champ2 = new DeckChampionDto
            {
                ChampionID = Guid.NewGuid(),
                Name = "Champion2",
                HP = 200
            };

            Assert.NotEqual(champ1.ChampionID, champ2.ChampionID);
            Assert.NotEqual(champ1.Name, champ2.Name);
            Assert.NotEqual(champ1.HP, champ2.HP);
        }
    }
}
