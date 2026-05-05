using VortexTCG.Api.Logs.ActionType.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Log.ActionType.DTOs
{
    public class ActionTypeDtoTests
    {
        [Fact]
        public void ActionTypeDTO_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            Guid gameLogId = Guid.NewGuid();
            Guid parentId = Guid.NewGuid();
            List<Guid> childIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            ActionTypeDTO dto = new ActionTypeDTO
            {
                Id = id,
                ActionDescription = "Attack action",
                GameLogId = gameLogId,
                ParentId = parentId,
                ChildIds = childIds
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Attack action", dto.ActionDescription);
            Assert.Equal(gameLogId, dto.GameLogId);
            Assert.Equal(parentId, dto.ParentId);
            Assert.Equal(2, dto.ChildIds!.Count);
        }

        [Fact]
        public void ActionTypeDTO_ParentId_CanBeNull()
        {
            ActionTypeDTO dto = new ActionTypeDTO { ParentId = null };

            Assert.Null(dto.ParentId);
        }

        [Fact]
        public void ActionTypeDTO_ChildIds_CanBeNull()
        {
            ActionTypeDTO dto = new ActionTypeDTO { ChildIds = null };

            Assert.Null(dto.ChildIds);
        }

        [Fact]
        public void ActionTypeDTO_ChildIds_CanBeEmpty()
        {
            ActionTypeDTO dto = new ActionTypeDTO { ChildIds = new List<Guid>() };

            Assert.NotNull(dto.ChildIds);
            Assert.Empty(dto.ChildIds);
        }

        [Fact]
        public void ActionTypeCreateDTO_CanSetAndGetAllProperties()
        {
            Guid gameLogId = Guid.NewGuid();
            Guid parentId = Guid.NewGuid();

            ActionTypeCreateDTO dto = new ActionTypeCreateDTO
            {
                ActionDescription = "Defense action",
                GameLogId = gameLogId,
                ParentId = parentId
            };

            Assert.Equal("Defense action", dto.ActionDescription);
            Assert.Equal(gameLogId, dto.GameLogId);
            Assert.Equal(parentId, dto.ParentId);
        }

        [Fact]
        public void ActionTypeCreateDTO_ParentId_CanBeNull()
        {
            ActionTypeCreateDTO dto = new ActionTypeCreateDTO
            {
                ActionDescription = "Root action",
                GameLogId = Guid.NewGuid(),
                ParentId = null
            };

            Assert.Null(dto.ParentId);
        }
    }
}
