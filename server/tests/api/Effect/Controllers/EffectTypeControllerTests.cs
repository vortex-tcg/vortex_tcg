using System;
using System.Collections.Generic;
using api.Effect.Controller;
using api.Effect.DTOs;
using api.Effect.Providers;
using api.Effect.Services;
using Microsoft.AspNetCore.Mvc;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace Tests
{
    public class EffectTypeControllerTests
    {
        private static (EffectTypeController, VortexDbContext) CreateControllerWithDb()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            EffectTypeProvider provider = new EffectTypeProvider(db);
            EffectTypeService service = new EffectTypeService(provider);
            EffectTypeController controller = new EffectTypeController(service);
            return (controller, db);
        }

        [Fact]
        public async Task List_Returns_200_With_Results()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            await db.EffectTypes.AddAsync(new EffectType
            {
                Id = Guid.NewGuid(),
                Label = "Burn"
            });
            await db.SaveChangesAsync();

            IActionResult result = await controller.list(CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            ResultDTO<List<EffectTypeDto>> dto = Assert.IsType<ResultDTO<List<EffectTypeDto>>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Single(dto.data!);
        }

        [Fact]
        public async Task List_Returns_Empty_When_No_Data()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            IActionResult result = await controller.list(CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            ResultDTO<List<EffectTypeDto>> dto = Assert.IsType<ResultDTO<List<EffectTypeDto>>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Empty(dto.data!);
        }

        [Fact]
        public async Task Get_Returns_200_When_Found()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();
            Guid id = Guid.NewGuid();

            await db.EffectTypes.AddAsync(new EffectType
            {
                Id = id,
                Label = "Poison"
            });
            await db.SaveChangesAsync();

            IActionResult result = await controller.get(id, CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            ResultDTO<EffectTypeDto> dto = Assert.IsType<ResultDTO<EffectTypeDto>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Equal(id, dto.data!.Id);
        }

        [Fact]
        public async Task Get_Returns_404_When_Not_Found()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            IActionResult result = await controller.get(Guid.NewGuid(), CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, okResult.StatusCode);

            ResultDTO<EffectTypeDto> dto = Assert.IsType<ResultDTO<EffectTypeDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Create_Returns_201_With_New_Entity()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            EffectTypeCreateDto createDto = new EffectTypeCreateDto { Label = "Freeze" };

            IActionResult result = await controller.create(createDto, CancellationToken.None);
            ObjectResult createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, createdResult.StatusCode);

            ResultDTO<EffectTypeDto> dto = Assert.IsType<ResultDTO<EffectTypeDto>>(createdResult.Value);
            Assert.True(dto.success);
            Assert.NotNull(dto.data);
            Assert.Equal("Freeze", dto.data.Label);
        }

        [Fact]
        public async Task Create_Returns_400_For_Invalid_Input()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            EffectTypeCreateDto createDto = new EffectTypeCreateDto { Label = "   " };

            IActionResult result = await controller.create(createDto, CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, okResult.StatusCode);

            ResultDTO<EffectTypeDto> dto = Assert.IsType<ResultDTO<EffectTypeDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Create_Returns_409_For_Duplicate_Label()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            await db.EffectTypes.AddAsync(new EffectType
            {
                Id = Guid.NewGuid(),
                Label = "Stun"
            });
            await db.SaveChangesAsync();

            EffectTypeCreateDto createDto = new EffectTypeCreateDto { Label = "Stun" };

            IActionResult result = await controller.create(createDto, CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, okResult.StatusCode);

            ResultDTO<EffectTypeDto> dto = Assert.IsType<ResultDTO<EffectTypeDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Update_Returns_200_On_Success()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();
            Guid id = Guid.NewGuid();

            EffectType entity = new EffectType
            {
                Id = id,
                Label = "OldName"
            };
            await db.EffectTypes.AddAsync(entity);
            await db.SaveChangesAsync();

            db.ChangeTracker.Clear();

            EffectTypeUpdateDto updateDto = new EffectTypeUpdateDto { Label = "NewName" };

            IActionResult result = await controller.update(id, updateDto, CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            ResultDTO<EffectTypeDto> dto = Assert.IsType<ResultDTO<EffectTypeDto>>(okResult.Value);
            Assert.True(dto.success);
            Assert.Equal("NewName", dto.data!.Label);
        }

        [Fact]
        public async Task Update_Returns_404_When_Not_Found()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            EffectTypeUpdateDto updateDto = new EffectTypeUpdateDto { Label = "SomeName" };

            IActionResult result = await controller.update(Guid.NewGuid(), updateDto, CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, okResult.StatusCode);

            ResultDTO<EffectTypeDto> dto = Assert.IsType<ResultDTO<EffectTypeDto>>(okResult.Value);
            Assert.False(dto.success);
        }

        [Fact]
        public async Task Delete_Returns_200_On_Success()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();
            Guid id = Guid.NewGuid();

            await db.EffectTypes.AddAsync(new EffectType
            {
                Id = id,
                Label = "ToDelete"
            });
            await db.SaveChangesAsync();

            IActionResult result = await controller.delete(id, CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);

            ResultDTO<bool> dto = Assert.IsType<ResultDTO<bool>>(okResult.Value);
            Assert.True(dto.success);
            Assert.True(dto.data);
        }

        [Fact]
        public async Task Delete_Returns_404_When_Not_Found()
        {
            (EffectTypeController controller, VortexDbContext db) = CreateControllerWithDb();

            IActionResult result = await controller.delete(Guid.NewGuid(), CancellationToken.None);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, okResult.StatusCode);

            ResultDTO<bool> dto = Assert.IsType<ResultDTO<bool>>(okResult.Value);
            Assert.False(dto.success);
        }
    }
}
