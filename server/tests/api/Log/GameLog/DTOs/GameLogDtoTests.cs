using VortexTCG.Api.Logs.GameLog.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Log.GameLog.DTOs
{
    public class GameLogDtoTests
    {
        [Fact]
        public void GameLogDTO_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            List<Guid> actionIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            GameLogDTO dto = new GameLogDTO
            {
                Id = id,
                Label = "Turn 5 log",
                TurnNumber = 5,
                UserId = userId,
                ActionIds = actionIds
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Turn 5 log", dto.Label);
            Assert.Equal(5, dto.TurnNumber);
            Assert.Equal(userId, dto.UserId);
            Assert.Equal(2, dto.ActionIds!.Count);
        }

        [Fact]
        public void GameLogDTO_UserId_CanBeNull()
        {
            GameLogDTO dto = new GameLogDTO { UserId = null };

            Assert.Null(dto.UserId);
        }

        [Fact]
        public void GameLogDTO_ActionIds_CanBeNull()
        {
            GameLogDTO dto = new GameLogDTO { ActionIds = null };

            Assert.Null(dto.ActionIds);
        }

        [Fact]
        public void GameLogDTO_ActionIds_CanBeEmpty()
        {
            GameLogDTO dto = new GameLogDTO { ActionIds = new List<Guid>() };

            Assert.NotNull(dto.ActionIds);
            Assert.Empty(dto.ActionIds);
        }

        [Fact]
        public void GameLogCreateDTO_CanSetAndGetAllProperties()
        {
            Guid userId = Guid.NewGuid();
            List<Guid> actionIds = new List<Guid> { Guid.NewGuid() };

            GameLogCreateDTO dto = new GameLogCreateDTO
            {
                Label = "Game round 3",
                TurnNumber = 3,
                UserId = userId,
                ActionIds = actionIds
            };

            Assert.Equal("Game round 3", dto.Label);
            Assert.Equal(3, dto.TurnNumber);
            Assert.Equal(userId, dto.UserId);
            Assert.Single(dto.ActionIds!);
        }

        [Fact]
        public void GameLogCreateDTO_UserId_CanBeNull()
        {
            GameLogCreateDTO dto = new GameLogCreateDTO
            {
                Label = "Log",
                TurnNumber = 1,
                UserId = null,
                ActionIds = null
            };

            Assert.Null(dto.UserId);
            Assert.Null(dto.ActionIds);
        }

        [Fact]
        public void GameLogDTO_DefaultTurnNumber_IsZero()
        {
            GameLogDTO dto = new GameLogDTO();

            Assert.Equal(0, dto.TurnNumber);
        }
    }
}
