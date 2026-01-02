using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Rank.DTOs;
using VortexTCG.Api.Rank.Providers;
using VortexTCG.Api.Rank.Services;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using RankModel = VortexTCG.DataAccess.Models.Rank;
using Xunit;

namespace Tests.Rank.Services
{
    public class RankServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static RankService CreateService(VortexDbContext db)
        {
            var provider = new RankProvider(db);
            return new RankService(provider);
        }

        [Fact]
        public async Task Create_Returns400_WhenLabelEmpty()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new RankCreateDTO { Label = string.Empty };

            ResultDTO<RankDTO> result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Create_Returns409_WhenDuplicate()
        {
            using var db = CreateDb();
            db.Ranks.Add(new RankModel { Id = Guid.NewGuid(), Label = "Bronze", nbVictory = 1 });
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new RankCreateDTO { Label = "Bronze", nbVictory = 1 };

            var result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new RankCreateDTO { Label = "Silver", nbVictory = 2 };

            var result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("Silver", result.data!.Label);
        }

        [Fact]
        public async Task GetAll_ReturnsItems()
        {
            using var db = CreateDb();
            db.Ranks.Add(new RankModel { Id = Guid.NewGuid(), Label = "A", nbVictory = 1 });
            db.Ranks.Add(new RankModel { Id = Guid.NewGuid(), Label = "B", nbVictory = 2 });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            ResultDTO<RankDTO[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Length);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var dto = await service.GetByIdAsync(Guid.NewGuid());

            Assert.False(dto.success);
            Assert.Equal(404, dto.statusCode);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new RankCreateDTO { Label = "Gold", nbVictory = 3 };

            var result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using var db = CreateDb();
            var entity = new RankModel { Id = Guid.NewGuid(), Label = "Old", nbVictory = 1 };
            db.Ranks.Add(entity);
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new RankCreateDTO { Label = "New", nbVictory = 4 };

            var result = await service.UpdateAsync(entity.Id, input);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("New", result.data!.Label);
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
            var entity = new RankModel { Id = Guid.NewGuid(), Label = "Del", nbVictory = 1 };
            db.Ranks.Add(entity);
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.DeleteAsync(entity.Id);

            Assert.True(result.success);
            Assert.Equal(204, result.statusCode);
        }
    }
}
