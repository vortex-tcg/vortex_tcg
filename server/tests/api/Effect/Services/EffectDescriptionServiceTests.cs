using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EffectDescriptionServiceTests
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
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectDescription
            {
                Id = id,
                Label = "Dmg5",
                Description = "Deal 5 damage",
                Parameter = "5"
            });

            var result = await service.listAsync();
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Single(result.data);
            Assert.Equal("Dmg5", result.data[0].Label);
            Assert.Equal(id, result.data[0].Id);
        }

        [Fact]
        public async Task Get_Returns_Success_When_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectDescription
            {
                Id = id,
                Label = "Heal10",
                Description = "Restore 10 HP",
                Parameter = "10"
            });

            var result = await service.getAsync(id);
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Heal10", result.data.Label);
        }

        [Fact]
        public async Task Get_Returns_404_When_Not_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var result = await service.getAsync(Guid.NewGuid());
            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Create_Returns_Success_With_New_Entity()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var dto = new EffectDescriptionInputDto
            {
                Label = "  Stun  ",
                Description = "  Stun for 2 turns  ",
                Parameter = "  2  "
            };

            var result = await service.createAsync(dto);
            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.NotNull(result.data);
            Assert.Equal("Stun", result.data.Label);
            Assert.Equal("Stun for 2 turns", result.data.Description);
            Assert.Equal("2", result.data.Parameter);
        }

        [Fact]
        public async Task Create_Returns_400_When_Label_Missing()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var dto = new EffectDescriptionInputDto
            {
                Label = "   ",
                Description = "Some description",
                Parameter = null
            };

            var result = await service.createAsync(dto);
            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("Label", result.message!);
        }

        [Fact]
        public async Task Create_Returns_400_When_Description_Missing()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var dto = new EffectDescriptionInputDto
            {
                Label = "ValidLabel",
                Description = "",
                Parameter = null
            };

            var result = await service.createAsync(dto);
            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
            Assert.Contains("Description", result.message!);
        }

        [Fact]
        public async Task Create_Returns_409_When_Label_Already_Exists()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            await provider.addAsync(new EffectDescription
            {
                Id = Guid.NewGuid(),
                Label = "Duplicate",
                Description = "First",
                Parameter = null
            });

            var dto = new EffectDescriptionInputDto
            {
                Label = "Duplicate",
                Description = "Second",
                Parameter = null
            };

            var result = await service.createAsync(dto);
            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
            Assert.Contains("label existe déjà", result.message!);
        }

        [Fact]
        public async Task Update_Returns_Success_With_Modified_Entity()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var id = Guid.NewGuid();
            var entity = new EffectDescription
            {
                Id = id,
                Label = "Old",
                Description = "Old desc",
                Parameter = "1"
            };
            db.EffectDescriptions.Add(entity);
            await db.SaveChangesAsync();

            // Clear tracking to simulate fresh provider call
            db.ChangeTracker.Clear();

            var dto = new EffectDescriptionInputDto
            {
                Label = "New",
                Description = "New desc",
                Parameter = "2"
            };

            var result = await service.updateAsync(id, dto);
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("New", result.data!.Label);
            Assert.Equal("New desc", result.data.Description);
            Assert.Equal("2", result.data.Parameter);
        }

        [Fact]
        public async Task Update_Returns_404_When_Not_Found()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var dto = new EffectDescriptionInputDto
            {
                Label = "Test",
                Description = "Test",
                Parameter = null
            };

            var result = await service.updateAsync(Guid.NewGuid(), dto);
            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Returns_400_When_Label_Invalid()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectDescription
            {
                Id = id,
                Label = "Original",
                Description = "Desc",
                Parameter = null
            });

            var dto = new EffectDescriptionInputDto
            {
                Label = "   ",
                Description = "New desc",
                Parameter = null
            };

            var result = await service.updateAsync(id, dto);
            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Update_Returns_409_When_New_Label_Already_Exists()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await provider.addAsync(new EffectDescription
            {
                Id = id1,
                Label = "First",
                Description = "Desc1",
                Parameter = null
            });

            await provider.addAsync(new EffectDescription
            {
                Id = id2,
                Label = "Second",
                Description = "Desc2",
                Parameter = null
            });

            var dto = new EffectDescriptionInputDto
            {
                Label = "First",
                Description = "Updated desc",
                Parameter = null
            };

            var result = await service.updateAsync(id2, dto);
            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task Update_Allows_Same_Label_When_Updating_Same_Record()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var id = Guid.NewGuid();
            var entity = new EffectDescription
            {
                Id = id,
                Label = "SameName",
                Description = "Old desc",
                Parameter = "1"
            };
            db.EffectDescriptions.Add(entity);
            await db.SaveChangesAsync();

            // Clear tracking to simulate fresh provider call
            db.ChangeTracker.Clear();

            var dto = new EffectDescriptionInputDto
            {
                Label = "SameName",
                Description = "Updated desc",
                Parameter = "2"
            };

            var result = await service.updateAsync(id, dto);
            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }

        [Fact]
        public async Task Delete_Returns_Success_When_Not_In_Use()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var id = Guid.NewGuid();
            await provider.addAsync(new EffectDescription
            {
                Id = id,
                Label = "ToDelete",
                Description = "Desc",
                Parameter = null
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
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var result = await service.deleteAsync(Guid.NewGuid());
            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Delete_Returns_409_When_In_Use()
        {
            using VortexDbContext db = CreateDb();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);

            var effectDescId = Guid.NewGuid();
            await provider.addAsync(new EffectDescription
            {
                Id = effectDescId,
                Label = "InUse",
                Description = "Desc",
                Parameter = null
            });

            // Add an Effect that references this description (minimal required fields)
            db.Effects.Add(new VortexTCG.DataAccess.Models.Effect
            {
                Id = Guid.NewGuid(),
                Title = "Test Effect",
                Parameter = "test",
                EffectTypeId = Guid.NewGuid(),
                EffectDescriptionId = effectDescId,
                StartConditionId = Guid.NewGuid(),
                EndConditionId = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "Test",
                UpdatedAtUtc = DateTime.UtcNow,
                UpdatedBy = "Test"
            });
            await db.SaveChangesAsync();

            var result = await service.deleteAsync(effectDescId);
            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
            Assert.Contains("utilisée", result.message!);
        }
    }
}
