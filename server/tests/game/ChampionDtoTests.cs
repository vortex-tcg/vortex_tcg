using System;
using VortexTCG.Game.DTO;
using Xunit;

namespace game.Tests
{
    public class ChampionDtoTests
    {
        [Fact]
        public void PlayCardChampionDto_ShouldStoreProperties()
        {
            PlayCardChampionDto dto = new PlayCardChampionDto
            {
                Id = Guid.NewGuid(),
                Hp = 25,
                Gold = 3,
                SecondaryCurrency = 2
            };

            Assert.NotEqual(Guid.Empty, dto.Id);
            Assert.Equal(25, dto.Hp);
            Assert.Equal(3, dto.Gold);
            Assert.Equal(2, dto.SecondaryCurrency);
        }

        [Fact]
        public void BattleChampionDto_ShouldStoreProperties()
        {
            BattleChampionDto dto = new BattleChampionDto
            {
                Hp = 18,
                SecondaryCurrency = 4,
                Gold = 5
            };

            Assert.Equal(18, dto.Hp);
            Assert.Equal(4, dto.SecondaryCurrency);
            Assert.Equal(5, dto.Gold);
        }
    }
}
