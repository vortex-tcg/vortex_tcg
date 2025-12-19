#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Collection.Controllers;
using VortexTCG.Api.Collection.DTOs;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Api.Collection.Services;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace Tests.Collection
{
    public class CollectionControllerTest
    {
        private VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private CollectionController CreateController(VortexDbContext db)
        {
            CollectionProvider provider = new CollectionProvider(db);
            CollectionService service = new CollectionService(provider);
            return new CollectionController(service);
        }

        [Fact]
        public async Task CreateCollection_ReturnsCreated()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            CollectionCreateDto dto = new CollectionCreateDto { UserId = Guid.NewGuid() };
            IActionResult result = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.NotEqual(Guid.Empty, payload.data.Id);
        }

        [Fact]
        public async Task GetById_ReturnsCollection()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            CollectionCreateDto dto = new CollectionCreateDto { UserId = Guid.NewGuid() };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<CollectionDto> payloadCreate = Assert.IsType<ResultDTO<CollectionDto>>(created.Value);
            IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
            ObjectResult ok = Assert.IsType<ObjectResult>(getResult);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(payloadCreate.data.Id, payload.data.Id);
        }

        [Fact]
        public async Task GetCollectionCard()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            Guid userId = new Guid();
            IActionResult getCollectionResult = await controller.GetCollectionByUserId(userId);
            ObjectResult result = Assert.IsType<ObjectResult>(getCollectionResult);
            ResultDTO<UserCollectionDto> data = Assert.IsType<ResultDTO<UserCollectionDto>>(result.Value);
            Assert.True(data.success);
            Assert.Equal(data.data.Decks.Count, 2);
            Assert.Equal(data.data.Faction.Count, 2);
            Assert.Equal(data.data.Cards.Count, 120);
        }

        [Fact]
        public async Task UpdateCollection_ChangesValues()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            CollectionCreateDto dto = new CollectionCreateDto { UserId = Guid.NewGuid() };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<CollectionDto> payloadCreate = Assert.IsType<ResultDTO<CollectionDto>>(created.Value);
            CollectionCreateDto updateDto = new CollectionCreateDto { UserId = Guid.NewGuid() };
            IActionResult updateResult = await controller.Update(payloadCreate.data.Id, updateDto);
            ObjectResult ok = Assert.IsType<ObjectResult>(updateResult);
            ResultDTO<CollectionDto> payload = Assert.IsType<ResultDTO<CollectionDto>>(ok.Value);
            Assert.True(payload.success);
        }

        [Fact]
        public async Task DeleteCollection_RemovesCollection()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            CollectionCreateDto dto = new CollectionCreateDto { UserId = Guid.NewGuid() };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<CollectionDto> payloadCreate = Assert.IsType<ResultDTO<CollectionDto>>(created.Value);
            IActionResult deleteResult = await controller.Delete(payloadCreate.data.Id);
            ObjectResult deleted = Assert.IsType<ObjectResult>(deleteResult);
            ResultDTO<bool> payload = Assert.IsType<ResultDTO<bool>>(deleted.Value);
            Assert.False(payload.success == false && payload.statusCode == 404);
            IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
            if (getResult is NotFoundResult)
            {
                Assert.IsType<NotFoundResult>(getResult);
            }
            else
            {
                ObjectResult ok = Assert.IsType<ObjectResult>(getResult);
                ResultDTO<CollectionDto> payloadGet = Assert.IsType<ResultDTO<CollectionDto>>(ok.Value);
                if (payloadGet != null)
                {
                    Assert.False(payloadGet.success);
                    Assert.Equal(404, payloadGet.statusCode);
                }
            }
        }

        [Fact]
        public async Task GetAll_ReturnsAllCollections()
        {
            using VortexDbContext db = CreateDb();
            CollectionController controller = CreateController(db);
            CollectionCreateDto dto1 = new CollectionCreateDto { UserId = Guid.NewGuid() };
            CollectionCreateDto dto2 = new CollectionCreateDto { UserId = Guid.NewGuid() };
            await controller.Add(dto1);
            await controller.Add(dto2);
            IActionResult result = await controller.GetAll();
            ObjectResult ok = Assert.IsType<ObjectResult>(result);
            ResultDTO<CollectionDto[]> payload = Assert.IsType<ResultDTO<CollectionDto[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(2, payload.data.Length);
        }
    }
}
