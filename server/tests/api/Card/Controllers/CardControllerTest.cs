using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Card.Controllers;
using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Card.Providers;
using VortexTCG.Api.Card.Services;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using Xunit;

namespace VortexTCG.Tests.Api.Card.Controllers
{
    public class CardControllerTest
    {
        private VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private CardController CreateController(VortexDbContext db)
        {
            CardProvider provider = new CardProvider(db);
            CardService service = new CardService(provider);
            return new CardController(service);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithList()
        {
            using VortexDbContext db = CreateDb();
            CardController controller = CreateController(db);

            // Seed one card via provider/service
            IActionResult create = await controller.Create(new CardCreateDto { Name = "Test", Attack = 1, Hp = 1, Price = 1, Description = "d", Picture = "p" });
            Assert.IsType<ObjectResult>(create);

            IActionResult result = await controller.GetAll();

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<CardDto[]> payload = Assert.IsType<ResultDTO<CardDto[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Single(payload.data!);
            Assert.Equal("Test", payload.data![0].Name);
        }

        [Fact]
        public async Task GetById_NotFound_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            CardController controller = CreateController(db);

            IActionResult result = await controller.GetById(Guid.NewGuid());

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<CardDto> payload = Assert.IsType<ResultDTO<CardDto>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task GetById_Ok_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            CardController controller = CreateController(db);

            IActionResult create = await controller.Create(new CardCreateDto { Name = "Found", Attack = 2, Hp = 3, Price = 4, Description = "desc", Picture = "pic" });
            ObjectResult created = Assert.IsType<ObjectResult>(create);
            ResultDTO<CardDto> payloadCreate = Assert.IsType<ResultDTO<CardDto>>(created.Value);
            Assert.True(payloadCreate.success);

            IActionResult result = await controller.GetById(payloadCreate.data!.Id);

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<CardDto> payload = Assert.IsType<ResultDTO<CardDto>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Found", payload.data!.Name);
        }
    }
}
