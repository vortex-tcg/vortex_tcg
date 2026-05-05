#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Collection.Controllers;
using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Api.Collection.Services;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace VortexTCG.Tests.Api.Collection.Controllers
{
    public class CollectionControllerTest
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static CollectionController CreateController(VortexDbContext db)
        {
            CollectionProvider provider = new CollectionProvider(db);
            CollectionService service = new CollectionService(provider);
            return new CollectionController(service);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithEmptyList()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.GetAll();

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto[]> payload = Assert.IsType<ResultDTO<CollectionDto[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);
            Assert.Empty(payload.data!);
        }

        [Fact]
        public async Task GetById_Returns404_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.GetById(Guid.NewGuid());

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task Add_Returns400_WhenUserIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.Add(new CollectionCreateDto { UserId = Guid.Empty });

            ObjectResult badRequest = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(badRequest.Value);
            Assert.False(payload.success);
            Assert.Equal(400, payload.statusCode);
        }

        [Fact]
        public async Task Add_Returns201_WhenValid()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.Add(new CollectionCreateDto { UserId = Guid.NewGuid() });

            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
        }

        [Fact]
        public async Task GetCollectionByUserId_Returns400_WhenUserIdEmpty()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.GetCollectionByUserId(Guid.Empty);

            ObjectResult badRequest = Assert.IsType<ObjectResult>(result);
            ResultDTO<UserCollectionDto> payload = Assert.IsType<ResultDTO<UserCollectionDto>>(badRequest.Value);
            Assert.False(payload.success);
            Assert.Equal(400, payload.statusCode);
        }

        [Fact]
        public async Task Delete_Returns404_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.Delete(Guid.NewGuid());

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<bool> payload = Assert.IsType<ResultDTO<bool>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task Delete_ReturnsSuccess_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            IActionResult addResult = await controller.Add(new CollectionCreateDto { UserId = Guid.NewGuid() });
            ObjectResult addOk = Assert.IsType<ObjectResult>(addResult);
            ResultDTO<CollectionDto> added = Assert.IsType<ResultDTO<CollectionDto>>(addOk.Value);

            IActionResult result = await controller.Delete(added.data!.Id);

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<bool> payload = Assert.IsType<ResultDTO<bool>>(ok.Value);
            Assert.True(payload.success);
        }

        [Fact]
        public async Task Update_Returns404_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.Update(Guid.NewGuid(), new CollectionCreateDto { UserId = Guid.NewGuid() });

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }

        [Fact]
        public async Task Update_Returns200_WhenValid()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            IActionResult addResult = await controller.Add(new CollectionCreateDto { UserId = Guid.NewGuid() });
            ObjectResult addOk = Assert.IsType<ObjectResult>(addResult);
            ResultDTO<CollectionDto> added = Assert.IsType<ResultDTO<CollectionDto>>(addOk.Value);

            IActionResult result = await controller.Update(added.data!.Id, new CollectionCreateDto { UserId = Guid.NewGuid() });

            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(200, payload.statusCode);
        }

        [Fact]
        public async Task GetCollectionByUserId_Returns404_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);

            IActionResult result = await controller.GetCollectionByUserId(Guid.NewGuid());

            ObjectResult notFound = Assert.IsType<ObjectResult>(result);
            ResultDTO<UserCollectionDto> payload = Assert.IsType<ResultDTO<UserCollectionDto>>(notFound.Value);
            Assert.False(payload.success);
            Assert.Equal(404, payload.statusCode);
        }
    }
}
