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
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private ChampionController CreateController(VortexDbContext db)
        {
            ChampionProvider provider = new ChampionProvider(db);
            ChampionService service = new ChampionService(provider);
            return new ChampionController(service);
        }

        [Fact]
        public async Task CreateChampion_ReturnsCreated()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            ChampionCreateDTO dto = new ChampionCreateDTO
            {
                Name = "Test Champion",
                Description = "A test champion for unit testing",
                HP = 100,
                Picture = "test.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult result = await controller.Create(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<ChampionDTO> payload = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.NotEqual(Guid.Empty, payload.data.Id);
            Assert.Equal("Test Champion", payload.data.Name);
            Assert.Equal(100, payload.data.HP);
        }

        [Fact]
        public async Task GetById_ReturnsChampion()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            ChampionCreateDTO dto = new ChampionCreateDTO
            {
                Name = "Test Champion",
                Description = "A test champion",
                HP = 100,
                Picture = "test.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult createResult = await controller.Create(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<ChampionDTO> payloadCreate = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
            ObjectResult ok = Assert.IsType<ObjectResult>(getResult);
            ResultDTO<ChampionDTO> payload = Assert.IsType<ResultDTO<ChampionDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(payloadCreate.data.Id, payload.data.Id);
            Assert.Equal("Test Champion", payload.data.Name);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsNotFound()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            Guid nonExistentId = Guid.NewGuid();
            IActionResult result = await controller.GetById(nonExistentId);
            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<ChampionDTO> payload = Assert.IsType<ResultDTO<ChampionDTO>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task UpdateChampion_ChangesValues()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            ChampionCreateDTO dto = new ChampionCreateDTO
            {
                Name = "Original Champion",
                Description = "Original description",
                HP = 100,
                Picture = "original.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult createResult = await controller.Create(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<ChampionDTO> payloadCreate = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            ChampionCreateDTO updateDto = new ChampionCreateDTO
            {
                Name = "Updated Champion",
                Description = "Updated description",
                HP = 150,
                Picture = "updated.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult updateResult = await controller.Update(payloadCreate.data.Id, updateDto);
            ObjectResult ok = Assert.IsType<ObjectResult>(updateResult);
            ResultDTO<ChampionDTO> payload = Assert.IsType<ResultDTO<ChampionDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Updated Champion", payload.data.Name);
            Assert.Equal(150, payload.data.HP);
            Assert.Equal("Updated description", payload.data.Description);
        }

        [Fact]
        public async Task UpdateChampion_NonExistent_ReturnsNotFound()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            Guid nonExistentId = Guid.NewGuid();
            ChampionCreateDTO updateDto = new ChampionCreateDTO
            {
                Name = "Updated Champion",
                Description = "Updated description",
                HP = 150,
                Picture = "updated.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult result = await controller.Update(nonExistentId, updateDto);
            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<ChampionDTO> payload = Assert.IsType<ResultDTO<ChampionDTO>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task DeleteChampion_RemovesChampion()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            ChampionCreateDTO dto = new ChampionCreateDTO
            {
                Name = "Test Champion",
                Description = "A test champion",
                HP = 100,
                Picture = "test.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult createResult = await controller.Create(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<ChampionDTO> payloadCreate = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            IActionResult deleteResult = await controller.Delete(payloadCreate.data.Id);
            ObjectResult deleted = Assert.IsType<ObjectResult>(deleteResult);
            ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(deleted.Value);
            Assert.True(payload.success);
            IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
            ObjectResult notFound = Assert.IsType<ObjectResult>(getResult);
            ResultDTO<ChampionDTO> payloadGet = Assert.IsType<ResultDTO<ChampionDTO>>(notFound.Value);
            Assert.False(payloadGet.success);
            Assert.Equal(404, payloadGet.statusCode);
        }

        [Fact]
        public async Task DeleteChampion_NonExistent_ReturnsNotFound()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            Guid nonExistentId = Guid.NewGuid();
            IActionResult result = await controller.Delete(nonExistentId);
            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsAllChampions()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            ChampionCreateDTO dto1 = new ChampionCreateDTO
            {
                Name = "Champion One",
                Description = "First champion",
                HP = 100,
                Picture = "champion1.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            ChampionCreateDTO dto2 = new ChampionCreateDTO
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
            IActionResult result = await controller.GetAll();
            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<ChampionDTO[]> payload = Assert.IsType<ResultDTO<ChampionDTO[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(2, payload.data.Length);
        }

        [Fact]
        public async Task GetAll_EmptyDatabase_ReturnsEmptyArray()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            IActionResult result = await controller.GetAll();
            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<ChampionDTO[]> payload = Assert.IsType<ResultDTO<ChampionDTO[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.Empty(payload.data);
        }

        [Fact]
        public async Task CreateChampion_WithZeroHP_ReturnsCreated()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            ChampionCreateDTO dto = new ChampionCreateDTO
            {
                Name = "Zero HP Champion",
                Description = "Champion with zero HP",
                HP = 0,
                Picture = "zero.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult result = await controller.Create(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<ChampionDTO> payload = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(0, payload.data.HP);
        }

        [Fact]
        public async Task CreateChampion_WithHighHP_ReturnsCreated()
        {
            using VortexDbContext db = CreateDb();
            ChampionController controller = CreateController(db);
            ChampionCreateDTO dto = new ChampionCreateDTO
            {
                Name = "High HP Champion",
                Description = "Champion with high HP",
                HP = 9999,
                Picture = "high.png",
                FactionId = Guid.NewGuid(),
                EffectId = Guid.NewGuid()
            };
            IActionResult result = await controller.Create(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<ChampionDTO> payload = Assert.IsType<ResultDTO<ChampionDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(9999, payload.data.HP);
        }
    }
}
