using System;
using VortexTCG.Game.DTO;
using Xunit;

namespace game.Tests
{
    public class PhaseDtoTests
    {
        [Fact]
        public void PhaseChangeResultDTO_ShouldHoldValues()
        {
            PhaseChangeResultDTO dto = new PhaseChangeResultDTO
            {
                CurrentPhase = GamePhase.ATTACK,
                ActivePlayerId = Guid.NewGuid(),
                TurnNumber = 4,
                AutoChanged = true,
                AutoChangeReason = "Timer",
                CanAct = false
            };

            Assert.Equal(GamePhase.ATTACK, dto.CurrentPhase);
            Assert.Equal(4, dto.TurnNumber);
            Assert.True(dto.AutoChanged);
            Assert.Equal("Timer", dto.AutoChangeReason);
            Assert.False(dto.CanAct);
            Assert.NotEqual(Guid.Empty, dto.ActivePlayerId);
        }

        [Fact]
        public void PhaseChangeForOpponentDTO_ShouldHoldValues()
        {
            PhaseChangeForOpponentDTO dto = new PhaseChangeForOpponentDTO
            {
                CurrentPhase = GamePhase.DEFENSE,
                ActivePlayerId = Guid.NewGuid(),
                TurnNumber = 7,
                IsYourTurnToAct = true
            };

            Assert.Equal(GamePhase.DEFENSE, dto.CurrentPhase);
            Assert.Equal(7, dto.TurnNumber);
            Assert.True(dto.IsYourTurnToAct);
            Assert.NotEqual(Guid.Empty, dto.ActivePlayerId);
        }

        [Fact]
        public void ChangePhaseResultDTO_ShouldInitializeDefaults()
        {
            ChangePhaseResultDTO dto = new ChangePhaseResultDTO
            {
                TurnChanged = true
            };

            Assert.NotNull(dto.ActivePlayerResult);
            Assert.NotNull(dto.OpponentResult);
            Assert.True(dto.TurnChanged);
        }
    }
}
