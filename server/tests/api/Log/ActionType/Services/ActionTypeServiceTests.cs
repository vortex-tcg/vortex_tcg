using System.Linq;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Logs.ActionType.DTOs;
using VortexTCG.Api.Logs.ActionType.Providers;
using VortexTCG.Api.Logs.ActionType.Services;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;
using GamelogModel = VortexTCG.DataAccess.Models.Gamelog;
using Xunit;

namespace Tests.Logs.ActionType.Services
{
    public class ActionTypeServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static ActionTypeService CreateService(VortexDbContext db)
        {
            var provider = new ActionTypeProvider(db);
            return new ActionTypeService(provider);
        }

        [Fact]
        public async Task GetAll_ReturnsData()
        {
            using var db = CreateDb();
            db.Actions.Add(new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "A1" });
            db.Actions.Add(new ActionTypeModel { Id = Guid.NewGuid(), actionDescription = "A2" });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            ResultDTO<ActionTypeDTO[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Length);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var dto = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(dto);
        }

        [Fact]
        public async Task Create_ReturnsCreatedDto()
        {
            using var db = CreateDb();
            var gamelogId = Guid.NewGuid();
            db.Gamelogs.Add(new GamelogModel { Id = gamelogId, Label = "log", TurnNumber = 1 });
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new ActionTypeCreateDTO { ActionDescription = "New", GameLogId = gamelogId, ParentId = null };

            var result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("New", result.data!.ActionDescription);
            Assert.NotEqual(Guid.Empty, result.data.Id);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new ActionTypeCreateDTO { ActionDescription = "Upd", GameLogId = Guid.NewGuid(), ParentId = null };

            var result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using var db = CreateDb();
            var gamelogId = Guid.NewGuid();
            db.Gamelogs.Add(new GamelogModel { Id = gamelogId, Label = "log", TurnNumber = 1 });
            var parentId = Guid.NewGuid();
            var parent = new ActionTypeModel { Id = parentId, actionDescription = "Parent", GameLogId = gamelogId, ParentId = parentId };
            var childId = Guid.NewGuid();
            var child = new ActionTypeModel { Id = childId, actionDescription = "Old", GameLogId = gamelogId, ParentId = parentId };
            db.Actions.AddRange(parent, child);
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new ActionTypeCreateDTO { ActionDescription = "New", GameLogId = gamelogId, ParentId = null };

            var result = await service.UpdateAsync(childId, input);

            Assert.Equal(200, result.statusCode);
            Assert.True(result.success);
            Assert.Equal("New", result.data!.ActionDescription);
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using var db = CreateDb();
            var gamelogId = Guid.NewGuid();
            db.Gamelogs.Add(new GamelogModel { Id = gamelogId, Label = "log", TurnNumber = 1 });
            var parentId = Guid.NewGuid();
            var parent = new ActionTypeModel { Id = parentId, actionDescription = "Parent", GameLogId = gamelogId, ParentId = parentId };
            var childId = Guid.NewGuid();
            var child = new ActionTypeModel { Id = childId, actionDescription = "Del", GameLogId = gamelogId, ParentId = parentId };
            db.Actions.AddRange(parent, child);
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.DeleteAsync(childId);

            Assert.Equal(200, result.statusCode);
            Assert.True(result.success);
        }
    }
}
