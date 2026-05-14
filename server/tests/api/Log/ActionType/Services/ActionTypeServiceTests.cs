using System.Linq;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Logs.ActionType.DTOs;
using VortexTCG.Api.Logs.ActionType.Providers;
using VortexTCG.Api.Logs.ActionType.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;
using GamelogModel = VortexTCG.DataAccess.Models.Gamelog;
using Xunit;

namespace VortexTCG.Tests.Api.Log.ActionType.Services
{
    public class ActionTypeServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static ActionTypeService CreateService(VortexDbContext db)
        {
            ActionTypeProvider provider = new ActionTypeProvider(db);
            return new ActionTypeService(provider);
        }

        [Fact]
        public async Task GetAll_ReturnsData()
        {
            using VortexDbContext db = CreateDb();
            db.Actions.Add(new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "A1" });
            db.Actions.Add(new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "A2" });
            await db.SaveChangesAsync();
            ActionTypeService service = CreateService(db);

            ResultDTO<ActionTypeDTO[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Length);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeService service = CreateService(db);

            ActionTypeDTO? dto = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(dto);
        }

        [Fact]
        public async Task GetById_ReturnsDto_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            Guid id = Guid.NewGuid();
            Guid gamelogId = Guid.NewGuid();
            db.Actions.Add(new ActionTypeModel { Id = id, actionDescription = "TestDesc", GameLogId = gamelogId, ParentId = Guid.Empty });
            await db.SaveChangesAsync();
            ActionTypeService service = CreateService(db);

            ActionTypeDTO? dto = await service.GetByIdAsync(id);

            Assert.NotNull(dto);
            Assert.Equal(id, dto!.Id);
            Assert.Equal("TestDesc", dto.ActionDescription);
            Assert.Equal(gamelogId, dto.GameLogId);
            Assert.Null(dto.ParentId);
        }

        [Fact]
        public async Task GetById_ReturnsDto_WithParentId_WhenParentIsSet()
        {
            using VortexDbContext db = CreateDb();
            Guid parentId = Guid.NewGuid();
            ActionTypeModel parent = new ActionTypeModel { Id = parentId, actionDescription = "Parent", ParentId = parentId };
            Guid childId = Guid.NewGuid();
            ActionTypeModel child = new ActionTypeModel { Id = childId, actionDescription = "Child", ParentId = parentId };
            db.Actions.AddRange(parent, child);
            await db.SaveChangesAsync();
            ActionTypeService service = CreateService(db);

            ActionTypeDTO? dto = await service.GetByIdAsync(childId);

            Assert.NotNull(dto);
            Assert.Equal(parentId, dto!.ParentId);
        }

        [Fact]
        public async Task GetById_ReturnsChildIds_WhenParentHasChildren()
        {
            using VortexDbContext db = CreateDb();
            Guid parentId = Guid.NewGuid();
            Guid childId = Guid.NewGuid();
            ActionTypeModel parent = new ActionTypeModel { Id = parentId, actionDescription = "Parent", ParentId = parentId };
            ActionTypeModel child = new ActionTypeModel { Id = childId, actionDescription = "Child", ParentId = parentId };
            db.Actions.AddRange(parent, child);
            await db.SaveChangesAsync();
            ActionTypeService service = CreateService(db);

            ActionTypeDTO? dto = await service.GetByIdAsync(parentId);

            Assert.NotNull(dto);
            Assert.NotNull(dto!.ChildIds);
            Assert.Contains(childId, dto.ChildIds!);
        }

        [Fact]
        public async Task Create_ReturnsCreatedDto()
        {
            using VortexDbContext db = CreateDb();
            Guid gamelogId = Guid.NewGuid();
            db.Gamelogs.Add(new GamelogModel { Id = gamelogId, Label = "log", TurnNumber = 1 });
            await db.SaveChangesAsync();
            ActionTypeService service = CreateService(db);
            ActionTypeCreateDTO input = new ActionTypeCreateDTO { ActionDescription = "New", GameLogId = gamelogId, ParentId = null };

            ResultDTO<ActionTypeDTO> result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("New", result.data!.ActionDescription);
            Assert.NotEqual(Guid.Empty, result.data.Id);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeService service = CreateService(db);
            ActionTypeCreateDTO input = new ActionTypeCreateDTO { ActionDescription = "Upd", GameLogId = Guid.NewGuid(), ParentId = null };

            ResultDTO<ActionTypeDTO> result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            Guid gamelogId = Guid.NewGuid();
            db.Gamelogs.Add(new GamelogModel { Id = gamelogId, Label = "log", TurnNumber = 1 });
            Guid parentId = Guid.NewGuid();
            ActionTypeModel parent = new ActionTypeModel { Id = parentId, actionDescription = "Parent", GameLogId = gamelogId, ParentId = parentId };
            Guid childId = Guid.NewGuid();
            ActionTypeModel child = new ActionTypeModel { Id = childId, actionDescription = "Old", GameLogId = gamelogId, ParentId = parentId };
            db.Actions.AddRange(parent, child);
            await db.SaveChangesAsync();
            ActionTypeService service = CreateService(db);
            ActionTypeCreateDTO input = new ActionTypeCreateDTO { ActionDescription = "New", GameLogId = gamelogId, ParentId = null };

            ResultDTO<ActionTypeDTO> result = await service.UpdateAsync(childId, input);

            Assert.Equal(200, result.statusCode);
            Assert.True(result.success);
            Assert.Equal("New", result.data!.ActionDescription);
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            ActionTypeService service = CreateService(db);

            ResultDTO<object> result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            Guid gamelogId = Guid.NewGuid();
            db.Gamelogs.Add(new GamelogModel { Id = gamelogId, Label = "log", TurnNumber = 1 });
            Guid parentId = Guid.NewGuid();
            ActionTypeModel parent = new ActionTypeModel { Id = parentId, actionDescription = "Parent", GameLogId = gamelogId, ParentId = parentId };
            Guid childId = Guid.NewGuid();
            ActionTypeModel child = new ActionTypeModel { Id = childId, actionDescription = "Del", GameLogId = gamelogId, ParentId = parentId };
            db.Actions.AddRange(parent, child);
            await db.SaveChangesAsync();
            ActionTypeService service = CreateService(db);

            ResultDTO<object> result = await service.DeleteAsync(childId);

            Assert.Equal(200, result.statusCode);
            Assert.True(result.success);
        }
    }
}
