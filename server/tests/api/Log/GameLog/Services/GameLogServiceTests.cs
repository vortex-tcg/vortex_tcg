using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Logs.GameLog.DTOs;
using VortexTCG.Api.Logs.GameLog.Providers;
using VortexTCG.Api.Logs.GameLog.Services;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using GameLogModel = VortexTCG.DataAccess.Models.Gamelog;
using Xunit;

namespace Tests.Logs.GameLog.Services
{
    public class GameLogServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static GameLogService CreateService(VortexDbContext db)
        {
            var provider = new GameLogProvider(db);
            return new GameLogService(provider);
        }

        [Fact]
        public async Task GetAll_ReturnsData()
        {
            using var db = CreateDb();
            db.Gamelogs.Add(new GameLogModel { Id = Guid.NewGuid(), Label = "A", TurnNumber = 1 });
            db.Gamelogs.Add(new GameLogModel { Id = Guid.NewGuid(), Label = "B", TurnNumber = 2 });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            ResultDTO<GameLogDTO[]> result = await service.GetAllAsync();

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
            var service = CreateService(db);
            var input = new GameLogCreateDTO { Label = "New", TurnNumber = 3, UserId = Guid.NewGuid() };

            var result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("New", result.data!.Label);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new GameLogCreateDTO { Label = "Upd", TurnNumber = 2, UserId = Guid.NewGuid() };

            var result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using var db = CreateDb();
            var entity = new GameLogModel { Id = Guid.NewGuid(), Label = "Old", TurnNumber = 1 };
            db.Gamelogs.Add(entity);
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new GameLogCreateDTO { Label = "New", TurnNumber = 5, UserId = Guid.NewGuid() };

            var result = await service.UpdateAsync(entity.Id, input);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("New", result.data!.Label);
            Assert.Equal(5, result.data.TurnNumber);
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
            var entity = new GameLogModel { Id = Guid.NewGuid(), Label = "Del", TurnNumber = 1 };
            db.Gamelogs.Add(entity);
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.DeleteAsync(entity.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }
    }
}
