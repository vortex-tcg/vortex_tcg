using System;
using api.Effect.DTOs;
using api.Effect.Providers;
using api.Effect.Services;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.Api.Effect.Services
{
    public class EffectTypeServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        [Fact]
        public async Task List_Returns_Success_With_Mapped_Entities()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectType
            {
                Id = id,
                Label = "Burn"
            });

            var result = await service.listAsync();
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Single(result.data);
            Assert.Equal("Burn", result.data[0].Label);
            Assert.Equal(id, result.data[0].Id);
        }

        [Fact]
        public async Task Get_Returns_Success_When_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectType
            {
                Id = id,
                Label = "Poison"
            });

            var result = await service.getAsync(id);
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Poison", result.data.Label);
        }

        [Fact]
        public async Task Get_Returns_404_When_Not_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var result = await service.getAsync(Guid.NewGuid());
            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Create_Returns_Success_With_New_Entity()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var dto = new EffectTypeCreateDto { Label = "  Freeze  " };

            var result = await service.createAsync(dto);
            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Freeze", result.data.Label);
        }

        [Fact]
        public async Task Create_Returns_400_When_Label_Missing()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var dto = new EffectTypeCreateDto { Label = "   " };

            var result = await service.createAsync(dto);
            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("Label", result.message!);
        }

        [Fact]
        public async Task Create_Returns_409_When_Label_Already_Exists()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            await provider.addAsync(new EffectType
            {
                Id = Guid.NewGuid(),
                Label = "Duplicate"
            });

            var dto = new EffectTypeCreateDto { Label = "Duplicate" };

            var result = await service.createAsync(dto);
            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
            Assert.Contains("label existe déjà", result.message!);
        }

        [Fact]
        public async Task Update_Returns_Success_With_Modified_Entity()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var id = Guid.NewGuid();
            var entity = new EffectType
            {
                Id = id,
                Label = "OldLabel"
            };
            db.EffectTypes.Add(entity);
            await db.SaveChangesAsync();

            // Clear tracking to simulate fresh provider call
            db.ChangeTracker.Clear();

            var dto = new EffectTypeUpdateDto { Label = "NewLabel" };

            var result = await service.updateAsync(id, dto);
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("NewLabel", result.data!.Label);
        }

        [Fact]
        public async Task Update_Returns_404_When_Not_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var dto = new EffectTypeUpdateDto { Label = "SomeName" };

            var result = await service.updateAsync(Guid.NewGuid(), dto);
            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Returns_400_When_Label_Invalid()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectType
            {
                Id = id,
                Label = "Original"
            });

            var dto = new EffectTypeUpdateDto { Label = "   " };

            var result = await service.updateAsync(id, dto);
            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Update_Returns_409_When_New_Label_Already_Exists()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await provider.addAsync(new EffectType
            {
                Id = id1,
                Label = "First"
            });

            await provider.addAsync(new EffectType
            {
                Id = id2,
                Label = "Second"
            });

            var dto = new EffectTypeUpdateDto { Label = "First" };

            var result = await service.updateAsync(id2, dto);
            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task Update_Allows_Same_Label_When_Updating_Same_Record()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var id = Guid.NewGuid();
            var entity = new EffectType
            {
                Id = id,
                Label = "SameName"
            };
            db.EffectTypes.Add(entity);
            await db.SaveChangesAsync();

            // Clear tracking to simulate fresh provider call
            db.ChangeTracker.Clear();

            var dto = new EffectTypeUpdateDto { Label = "SameName" };

            var result = await service.updateAsync(id, dto);
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }

        [Fact]
        public async Task Delete_Returns_Success_When_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectType
            {
                Id = id,
                Label = "ToDelete"
            });

            var result = await service.deleteAsync(id);
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.True(result.data);
        }

        [Fact]
        public async Task Delete_Returns_404_When_Not_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectTypeProvider(db);
            var service = new EffectTypeService(provider);

            var result = await service.deleteAsync(Guid.NewGuid());
            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }
    }
}
