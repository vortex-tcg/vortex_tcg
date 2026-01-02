using System;
using api.Effect.Controller;
using api.Effect.DTOs;
using api.Effect.Providers;
using api.Effect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.Api.Effect.Controllers
{
    public class EffectDescriptionControllerTests
    {
        private static (EffectDescriptionController, VortexDbContext) CreateControllerWithDb()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            var provider = new EffectDescriptionProvider(db);
            var service = new EffectDescriptionService(provider);
            var controller = new EffectDescriptionController(service);
            return (controller, db);
        }

        [Fact]
        public async Task List_Returns_200_With_Results()
        {
            var (controller, db) = CreateControllerWithDb();

            await db.EffectDescriptions.AddAsync(new EffectDescription
            {
                Id = Guid.NewGuid(),
                Label = "Test",
                Description = "Test desc",
                Parameter = null
            });
            await db.SaveChangesAsync();

            var result = await controller.list(CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<List<EffectDescriptionDto>>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Single(dto.data!);
        }

        [Fact]
        public async Task List_Returns_Empty_When_No_Data()
        {
            var (controller, db) = CreateControllerWithDb();

            var result = await controller.list(CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<List<EffectDescriptionDto>>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Empty(dto.data!);
        }

        [Fact]
        public async Task Get_Returns_200_When_Found()
        {
            var (controller, db) = CreateControllerWithDb();
            var id = Guid.NewGuid();

            await db.EffectDescriptions.AddAsync(new EffectDescription
            {
                Id = id,
                Label = "Found",
                Description = "Found desc",
                Parameter = "param"
            });
            await db.SaveChangesAsync();

            var result = await controller.get(id, CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<EffectDescriptionDto>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Equal(id, dto.data!.Id);
        }

        [Fact]
        public async Task Get_Returns_404_When_Not_Found()
        {
            var (controller, db) = CreateControllerWithDb();

            var result = await controller.get(Guid.NewGuid(), CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<EffectDescriptionDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Create_Returns_201_With_New_Entity()
        {
            var (controller, db) = CreateControllerWithDb();

            var createDto = new EffectDescriptionInputDto
            {
                Label = "NewEffect",
                Description = "Effect description",
                Parameter = "5"
            };

            var result = await controller.create(createDto, CancellationToken.None);
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<EffectDescriptionDto>>(createdResult.Value);
            Assert.True(dto.success);
            Assert.NotNull(dto.data);
            Assert.Equal("NewEffect", dto.data.Label);
        }

        [Fact]
        public async Task Create_Returns_400_For_Invalid_Input()
        {
            var (controller, db) = CreateControllerWithDb();

            var createDto = new EffectDescriptionInputDto
            {
                Label = "  ",
                Description = "Effect description",
                Parameter = null
            };

            var result = await controller.create(createDto, CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<EffectDescriptionDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Create_Returns_409_For_Duplicate_Label()
        {
            var (controller, db) = CreateControllerWithDb();

            await db.EffectDescriptions.AddAsync(new EffectDescription
            {
                Id = Guid.NewGuid(),
                Label = "Existing",
                Description = "Desc",
                Parameter = null
            });
            await db.SaveChangesAsync();

            var createDto = new EffectDescriptionInputDto
            {
                Label = "Existing",
                Description = "New desc",
                Parameter = null
            };

            var result = await controller.create(createDto, CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<EffectDescriptionDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Update_Returns_200_On_Success()
        {
            var (controller, db) = CreateControllerWithDb();
            var id = Guid.NewGuid();

            var entity = new EffectDescription
            {
                Id = id,
                Label = "Original",
                Description = "Original desc",
                Parameter = null
            };
            await db.EffectDescriptions.AddAsync(entity);
            await db.SaveChangesAsync();

            // Clear tracking to avoid conflicts
            db.ChangeTracker.Clear();

            var updateDto = new EffectDescriptionInputDto
            {
                Label = "Updated",
                Description = "Updated desc",
                Parameter = "new"
            };

            var result = await controller.update(id, updateDto, CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<EffectDescriptionDto>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Equal("Updated", dto.data!.Label);
        }

        [Fact]
        public async Task Update_Returns_404_When_Not_Found()
        {
            var (controller, db) = CreateControllerWithDb();

            var updateDto = new EffectDescriptionInputDto
            {
                Label = "NonExistent",
                Description = "Desc",
                Parameter = null
            };

            var result = await controller.update(Guid.NewGuid(), updateDto, CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<EffectDescriptionDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Delete_Returns_200_On_Success()
        {
            var (controller, db) = CreateControllerWithDb();
            var id = Guid.NewGuid();

            await db.EffectDescriptions.AddAsync(new EffectDescription
            {
                Id = id,
                Label = "ToDelete",
                Description = "To be deleted",
                Parameter = null
            });
            await db.SaveChangesAsync();

            var result = await controller.delete(id, CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<bool>>(okResult.Value);
            Assert.True(dto.success);
            Assert.True(dto.data);
        }

        [Fact]
        public async Task Delete_Returns_404_When_Not_Found()
        {
            var (controller, db) = CreateControllerWithDb();

            var result = await controller.delete(Guid.NewGuid(), CancellationToken.None);
            var okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, okResult.StatusCode);

            var dto = Assert.IsType<ResultDTO<bool>>(okResult.Value);
            Assert.False(dto.success);
        }
    }
}
