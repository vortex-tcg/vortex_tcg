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
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private CardController CreateController(VortexDbContext db)
        {
            var provider = new CardProvider(db);
            var service = new CardService(provider);
            return new CardController(service);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithList()
        {
            using var db = CreateDb();
            var controller = CreateController(db);

            // Seed one card via provider/service
            var create = await controller.Create(new CardCreateDTO { Name = "Test", Attack = 1, Hp = 1, Price = 1, Description = "d", Picture = "p" });
            Assert.IsType<ObjectResult>(create);

            var result = await controller.GetAll();

            var ok = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CardDTO[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.NotNull(payload.data);
            Assert.Single(payload.data!);
            Assert.Equal("Test", payload.data![0].Name);
        }

        [Fact]
        public async Task GetById_NotFound_WhenMissing()
        {
            using var db = CreateDb();
            var controller = CreateController(db);

            var result = await controller.GetById(Guid.NewGuid());

            var notFound = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CardDTO>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task GetById_Ok_WhenFound()
        {
            using var db = CreateDb();
            var controller = CreateController(db);

            var create = await controller.Create(new CardCreateDTO { Name = "Found", Attack = 2, Hp = 3, Price = 4, Description = "desc", Picture = "pic" });
            var created = Assert.IsType<ObjectResult>(create);
            var payloadCreate = Assert.IsType<ResultDTO<CardDTO>>(created.Value);
            Assert.True(payloadCreate.success);

            var result = await controller.GetById(payloadCreate.data!.Id);

            var ok = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CardDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Found", payload.data!.Name);
        }
    }
}
