using System;
using VortexTCG.Game.DTO;
using Xunit;

namespace game.Tests
{
    public class PlayCardDtoTests
    {
        [Fact]
        public void PlayCardOpponentResultDto_ShouldStoreProperties()
        {
            PlayCardOpponentResultDto dto = new PlayCardOpponentResultDto
            {
                PlayerId = Guid.NewGuid(),
                PlayedCard = new GameCardDto { Id = Guid.NewGuid(), Name = "Spell" },
                Champion = new PlayCardChampionDto { Id = Guid.NewGuid(), Hp = 20, Gold = 3, SecondaryCurrency = 1 },
                location = 2
            };

            Assert.NotNull(dto.PlayedCard);
            Assert.NotNull(dto.Champion);
            Assert.Equal(2, dto.location);
            Assert.NotEqual(Guid.Empty, dto.PlayerId);
        }

        [Fact]
        public void PlayCardPlayerResultDto_ShouldStoreProperties()
        {
            PlayCardPlayerResultDto dto = new PlayCardPlayerResultDto
            {
                PlayerId = Guid.NewGuid(),
                PlayedCard = new GameCardDto { Id = Guid.NewGuid(), Name = "Unit" },
                Champion = new PlayCardChampionDto { Id = Guid.NewGuid(), Hp = 18, Gold = 5, SecondaryCurrency = 0 },
                location = 3,
                canPlayed = true
            };

            Assert.NotNull(dto.PlayedCard);
            Assert.NotNull(dto.Champion);
            Assert.Equal(3, dto.location);
            Assert.True(dto.canPlayed);
        }

        [Fact]
        public void PlayCardResponseDto_ShouldHoldResults()
        {
            PlayCardResponseDto dto = new PlayCardResponseDto
            {
                PlayerResult = new PlayCardPlayerResultDto
                {
                    PlayerId = Guid.NewGuid(),
                    PlayedCard = new GameCardDto { Id = Guid.NewGuid(), Name = "Unit" },
                    Champion = new PlayCardChampionDto { Id = Guid.NewGuid(), Hp = 10, Gold = 1, SecondaryCurrency = 0 },
                    location = 0,
                    canPlayed = false
                },
                OpponentResult = new PlayCardOpponentResultDto
                {
                    PlayerId = Guid.NewGuid(),
                    PlayedCard = new GameCardDto { Id = Guid.NewGuid(), Name = "Spell" },
                    Champion = new PlayCardChampionDto { Id = Guid.NewGuid(), Hp = 9, Gold = 2, SecondaryCurrency = 1 },
                    location = 1
                }
            };

            Assert.NotNull(dto.PlayerResult);
            Assert.NotNull(dto.OpponentResult);
        }
    }
}
