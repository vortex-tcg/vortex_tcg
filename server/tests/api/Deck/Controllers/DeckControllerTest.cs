using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Deck.Controllers;
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Providers;
using VortexTCG.Api.Deck.Services;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using Xunit;

namespace VortexTCG.Tests.Api.Deck.Controllers
{
    public class DeckControllerTest
    {
        private VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private DeckController CreateController(VortexDbContext db)
        {
            DeckProvider provider = new DeckProvider(db);
            DeckService service = new DeckService(provider);
            return new DeckController(service);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithEmptyList()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.GetAll();

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto[]> payload = Assert.IsType<ResultDTO<DeckDto[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Empty(payload.data!);
        }

        [Fact]
        public async Task Create_ReturnsCreated_WithValidData()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            Guid userId = Guid.NewGuid();
            Guid championId = Guid.NewGuid();
            Guid factionId = Guid.NewGuid();

            DeckCreateDto dto = new DeckCreateDto
            {
                Label = "TestDeck",
                UserId = userId,
                ChampionId = championId,
                FactionId = factionId
            };

            IActionResult result = await controller.Create(dto);

            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto> payload = Assert.IsType<ResultDTO<DeckDto>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.NotNull(payload.data);
            Assert.Equal("TestDeck", payload.data!.Label);
            Assert.Equal(userId, payload.data.UserId);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenLabelEmpty()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            DeckCreateDto dto = new DeckCreateDto
            {
                Label = "",
                UserId = Guid.NewGuid(),
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            IActionResult result = await controller.Create(dto);

            ObjectResult badRequest = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto> payload = Assert.IsType<ResultDTO<DeckDto>>(badRequest.Value);
            Assert.False(payload.success);
            Assert.Equal(400, payload.statusCode);
        }

        [Fact]
        public async Task Create_ReturnsConflict_WhenDuplicateLabelForUser()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            Guid userId = Guid.NewGuid();
            DeckCreateDto dto = new DeckCreateDto
            {
                Label = "Duplicate",
                UserId = userId,
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            await controller.Create(dto);
            IActionResult result = await controller.Create(dto);

            ObjectResult conflict = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto> payload = Assert.IsType<ResultDTO<DeckDto>>(conflict.Value);
            Assert.False(payload.success);
            Assert.Equal(409, payload.statusCode);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.GetById(Guid.NewGuid());

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto> payload = Assert.IsType<ResultDTO<DeckDto>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            DeckCreateDto dto = new DeckCreateDto
            {
                Label = "FindMe",
                UserId = Guid.NewGuid(),
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            IActionResult createResult = await controller.Create(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<DeckDto> createPayload = Assert.IsType<ResultDTO<DeckDto>>(created.Value);
            Assert.True(createPayload.success);

            IActionResult result = await controller.GetById(createPayload.data!.Id);

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto> payload = Assert.IsType<ResultDTO<DeckDto>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("FindMe", payload.data!.Label);
        }

        [Fact]
        public async Task GetByUserId_ReturnsOk_WithUserDecks()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            Guid userId = Guid.NewGuid();
            DeckCreateDto dto1 = new DeckCreateDto
            {
                Label = "Deck1",
                UserId = userId,
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };
            DeckCreateDto dto2 = new DeckCreateDto
            {
                Label = "Deck2",
                UserId = userId,
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            await controller.Create(dto1);
            await controller.Create(dto2);

            IActionResult result = await controller.GetByUserId(userId);

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto[]> payload = Assert.IsType<ResultDTO<DeckDto[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Equal(2, payload.data!.Length);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            DeckUpdateDto dto = new DeckUpdateDto
            {
                Label = "Updated",
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            IActionResult result = await controller.Update(Guid.NewGuid(), dto);

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto> payload = Assert.IsType<ResultDTO<DeckDto>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            DeckCreateDto createDto = new DeckCreateDto
            {
                Label = "Original",
                UserId = Guid.NewGuid(),
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            IActionResult createResult = await controller.Create(createDto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<DeckDto> createPayload = Assert.IsType<ResultDTO<DeckDto>>(created.Value);

            DeckUpdateDto updateDto = new DeckUpdateDto
            {
                Label = "Updated",
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            IActionResult result = await controller.Update(createPayload.data!.Id, updateDto);

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<DeckDto> payload = Assert.IsType<ResultDTO<DeckDto>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);
            Assert.Equal("Updated", payload.data!.Label);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            IActionResult result = await controller.Delete(Guid.NewGuid());

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            DeckController controller = CreateController(db);

            DeckCreateDto createDto = new DeckCreateDto
            {
                Label = "ToDelete",
                UserId = Guid.NewGuid(),
                ChampionId = Guid.NewGuid(),
                FactionId = Guid.NewGuid()
            };

            IActionResult createResult = await controller.Create(createDto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<DeckDto> createPayload = Assert.IsType<ResultDTO<DeckDto>>(created.Value);

            IActionResult result = await controller.Delete(createPayload.data!.Id);

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);

            // Verify it's deleted
            IActionResult getResult = await controller.GetById(createPayload.data.Id);
            ObjectResult notFound = Assert.IsType<ObjectResult>(getResult);
            ResultDTO<DeckDto> getPayload = Assert.IsType<ResultDTO<DeckDto>>(notFound.Value);
            Assert.False(getPayload.success);
            Assert.Equal(404, getPayload.statusCode);
        }
    }
}
