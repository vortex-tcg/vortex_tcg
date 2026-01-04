using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Api.Collection.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using CollectionModel = VortexTCG.DataAccess.Models.Collection;
using Xunit;

namespace VortexTCG.Tests.Api.Collection.Services
{
    public class CollectionServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static CollectionService CreateService(VortexDbContext db)
        {
            CollectionProvider provider = new CollectionProvider(db);
            return new CollectionService(provider);
        }

        [Fact]
        public async Task Create_Returns400_WhenUserIdMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.Empty };

            ResultDTO<CollectionDto> result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.NewGuid() };

            ResultDTO<CollectionDto> result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.NotEqual(Guid.Empty, result.data!.Id);
        }

        [Fact]
        public async Task GetById_ReturnsNull_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);

            ResultDTO<CollectionDto> result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task GetById_ReturnsData()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);

            ResultDTO<CollectionDto> dto = await service.GetByIdAsync(entity.Id);

            Assert.True(dto.success);
            Assert.Equal(entity.Id, dto.data!.Id);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.NewGuid() };

            ResultDTO<CollectionDto> result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionService service = CreateService(db);
            CollectionCreateDto input = new CollectionCreateDto { UserId = Guid.NewGuid() };

            ResultDTO<CollectionDto> result = await service.UpdateAsync(entity.Id, input);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal(entity.Id, result.data!.Id);
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            CollectionService service = CreateService(db);

            ResultDTO<bool> result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }
    }
}
