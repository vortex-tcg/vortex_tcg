using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Logs.GameLog.DTOs;
using VortexTCG.Api.Logs.GameLog.Providers;
using VortexTCG.Api.Logs.GameLog.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using GameLogModel = VortexTCG.DataAccess.Models.Gamelog;
using Xunit;

namespace VortexTCG.Tests.Api.Log.GameLog.Services
{
    public class GameLogServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static GameLogService CreateService(VortexDbContext db)
        {
            GameLogProvider provider = new GameLogProvider(db);
            return new GameLogService(provider);
        }

        [Fact]
        public async Task GetAll_ReturnsData()
        {
            using VortexDbContext db = CreateDb();
            db.Gamelogs.Add(new GameLogModel { Id = Guid.NewGuid(), Label = "A", TurnNumber = 1 });
            db.Gamelogs.Add(new GameLogModel { Id = Guid.NewGuid(), Label = "B", TurnNumber = 2 });
            await db.SaveChangesAsync();
            GameLogService service = CreateService(db);

            ResultDTO<GameLogDTO[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Length);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            GameLogService service = CreateService(db);

            GameLogDTO? dto = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(dto);
        }

        [Fact]
        public async Task Create_ReturnsCreatedDto()
        {
            using VortexDbContext db = CreateDb();
            GameLogService service = CreateService(db);
            GameLogCreateDTO input = new GameLogCreateDTO { Label = "New", TurnNumber = 3, UserId = Guid.NewGuid() };

            ResultDTO<GameLogDTO> result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("New", result.data!.Label);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            GameLogService service = CreateService(db);
            GameLogCreateDTO input = new GameLogCreateDTO { Label = "Upd", TurnNumber = 2, UserId = Guid.NewGuid() };

            ResultDTO<GameLogDTO> result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            GameLogModel entity = new GameLogModel { Id = Guid.NewGuid(), Label = "Old", TurnNumber = 1 };
            db.Gamelogs.Add(entity);
            await db.SaveChangesAsync();
            GameLogService service = CreateService(db);
            GameLogCreateDTO input = new GameLogCreateDTO { Label = "New", TurnNumber = 5, UserId = Guid.NewGuid() };

            ResultDTO<GameLogDTO> result = await service.UpdateAsync(entity.Id, input);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("New", result.data!.Label);
            Assert.Equal(5, result.data.TurnNumber);
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            GameLogService service = CreateService(db);

            ResultDTO<object> result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            GameLogModel entity = new GameLogModel { Id = Guid.NewGuid(), Label = "Del", TurnNumber = 1 };
            db.Gamelogs.Add(entity);
            await db.SaveChangesAsync();
            GameLogService service = CreateService(db);

            ResultDTO<object> result = await service.DeleteAsync(entity.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }
    }
}
