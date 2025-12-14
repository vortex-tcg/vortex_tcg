#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Champion.Controllers;
using VortexTCG.Api.Champion.DTOs;
using VortexTCG.Api.Champion.Providers;
using VortexTCG.Api.Champion.Services;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace Tests.Champion
{
    public class ChampionControllerTest
    {
        private VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private ChampionController CreateController(VortexDbContext db)
        {
            var provider = new ChampionProvider(db);
            var service = new ChampionService(provider);
            return new ChampionController(service);
        }

        [Fact]
        public async Task CreateChampion_ReturnsCreated()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new ChampionCreateDTO
            {
                Name = "Test Champion",
                Description = "A test champion for unit testing",
                HP = 100,
                Picture = "test.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var result = await controller.Create(dto);
            var created = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.NotEqual(Guid.Empty, payload.data.Id);
            Assert.Equal("Test Champion", payload.data.Name);
            Assert.Equal(100, payload.data.HP);
        }

        [Fact]
        public async Task GetById_ReturnsChampion()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new ChampionCreateDTO
            {
                Name = "Test Champion",
                Description = "A test champion",
                HP = 100,
                Picture = "test.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var createResult = await controller.Create(dto);
            var created = Assert.IsType<ObjectResult>(createResult);
            var payloadCreate = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            var getResult = await controller.GetById(payloadCreate.data.Id);
            var ok = Assert.IsType<ObjectResult>(getResult);
            var payload = Assert.IsType<ResultDTO<ChampionDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(payloadCreate.data.Id, payload.data.Id);
            Assert.Equal("Test Champion", payload.data.Name);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsNotFound()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var nonExistentId = Guid.NewGuid();
            var result = await controller.GetById(nonExistentId);
            var notFound = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<ChampionDTO>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task UpdateChampion_ChangesValues()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new ChampionCreateDTO
            {
                Name = "Original Champion",
                Description = "Original description",
                HP = 100,
                Picture = "original.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var createResult = await controller.Create(dto);
            var created = Assert.IsType<ObjectResult>(createResult);
            var payloadCreate = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            var updateDto = new ChampionCreateDTO
            {
                Name = "Updated Champion",
                Description = "Updated description",
                HP = 150,
                Picture = "updated.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var updateResult = await controller.Update(payloadCreate.data.Id, updateDto);
            var ok = Assert.IsType<ObjectResult>(updateResult);
            var payload = Assert.IsType<ResultDTO<ChampionDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Updated Champion", payload.data.Name);
            Assert.Equal(150, payload.data.HP);
            Assert.Equal("Updated description", payload.data.Description);
        }

        [Fact]
        public async Task UpdateChampion_NonExistent_ReturnsNotFound()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var nonExistentId = Guid.NewGuid();
            var updateDto = new ChampionCreateDTO
            {
                Name = "Updated Champion",
                Description = "Updated description",
                HP = 150,
                Picture = "updated.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var result = await controller.Update(nonExistentId, updateDto);
            var notFound = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<ChampionDTO>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task DeleteChampion_RemovesChampion()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new ChampionCreateDTO
            {
                Name = "Test Champion",
                Description = "A test champion",
                HP = 100,
                Picture = "test.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var createResult = await controller.Create(dto);
            var created = Assert.IsType<ObjectResult>(createResult);
            var payloadCreate = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            var deleteResult = await controller.Delete(payloadCreate.data.Id);
            var deleted = Assert.IsType<ObjectResult>(deleteResult);
            var payload = Assert.IsType<ResultDTO<object>>(deleted.Value);
            Assert.True(payload.success);
            var getResult = await controller.GetById(payloadCreate.data.Id);
            var notFound = Assert.IsType<ObjectResult>(getResult);
            var payloadGet = Assert.IsType<ResultDTO<ChampionDTO>>(notFound.Value);
            Assert.False(payloadGet.success);
            Assert.Equal(404, payloadGet.statusCode);
        }

        [Fact]
        public async Task DeleteChampion_NonExistent_ReturnsNotFound()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var nonExistentId = Guid.NewGuid();
            var result = await controller.Delete(nonExistentId);
            var notFound = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<object>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsAllChampions()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto1 = new ChampionCreateDTO
            {
                Name = "Champion One",
                Description = "First champion",
                HP = 100,
                Picture = "champion1.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var dto2 = new ChampionCreateDTO
            {
                Name = "Champion Two",
                Description = "Second champion",
                HP = 120,
                Picture = "champion2.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            await controller.Create(dto1);
            await controller.Create(dto2);
            var result = await controller.GetAll();
            var ok = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<ChampionDTO[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(2, payload.data.Length);
        }

        [Fact]
        public async Task GetAll_EmptyDatabase_ReturnsEmptyArray()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var result = await controller.GetAll();
            var ok = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<ChampionDTO[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.Empty(payload.data);
        }

        [Fact]
        public async Task CreateChampion_WithZeroHP_ReturnsCreated()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new ChampionCreateDTO
            {
                Name = "Zero HP Champion",
                Description = "Champion with zero HP",
                HP = 0,
                Picture = "zero.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var result = await controller.Create(dto);
            var created = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(0, payload.data.HP);
        }

        [Fact]
        public async Task CreateChampion_WithHighHP_ReturnsCreated()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new ChampionCreateDTO
            {
                Name = "High HP Champion",
                Description = "Champion with high HP",
                HP = 9999,
                Picture = "high.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            var result = await controller.Create(dto);
            var created = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(9999, payload.data.HP);
        }
    }
}
