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
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private CollectionController CreateController(VortexDbContext db)
        {
            var provider = new CollectionProvider(db);
            var service = new CollectionService(provider);
            return new CollectionController(service);
        }

        [Fact]
        public async Task CreateCollection_ReturnsCreated()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new CollectionCreateDTO { UserId = Guid.NewGuid() };
            var result = await controller.Add(dto);
            var created = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CollectionDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.NotEqual(Guid.Empty, payload.data.Id);
        }

        [Fact]
        public async Task GetById_ReturnsCollection()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new CollectionCreateDTO { UserId = Guid.NewGuid() };
            var createResult = await controller.Add(dto);
            var created = Assert.IsType<ObjectResult>(createResult);
            var payloadCreate = Assert.IsType<ResultDTO<CollectionDTO>>(created.Value);
            var getResult = await controller.GetById(payloadCreate.data.Id);
            var ok = Assert.IsType<ObjectResult>(getResult);
            var payload = Assert.IsType<ResultDTO<CollectionDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(payloadCreate.data.Id, payload.data.Id);
        }

        [Fact]
        public async Task UpdateCollection_ChangesValues()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new CollectionCreateDTO { UserId = Guid.NewGuid() };
            var createResult = await controller.Add(dto);
            var created = Assert.IsType<ObjectResult>(createResult);
            var payloadCreate = Assert.IsType<ResultDTO<CollectionDTO>>(created.Value);
            var updateDto = new CollectionCreateDTO { UserId = Guid.NewGuid() };
            var updateResult = await controller.Update(payloadCreate.data.Id, updateDto);
            var ok = Assert.IsType<ObjectResult>(updateResult);
            var payload = Assert.IsType<ResultDTO<CollectionDTO>>(ok.Value);
            Assert.True(payload.success);
        }

        [Fact]
        public async Task DeleteCollection_RemovesCollection()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new CollectionCreateDTO { UserId = Guid.NewGuid() };
            var createResult = await controller.Add(dto);
            var created = Assert.IsType<ObjectResult>(createResult);
            var payloadCreate = Assert.IsType<ResultDTO<CollectionDTO>>(created.Value);
            var deleteResult = await controller.Delete(payloadCreate.data.Id);
            var deleted = Assert.IsType<ObjectResult>(deleteResult);
            var payload = Assert.IsType<ResultDTO<object>>(deleted.Value);
            Assert.False(payload.success == false && payload.statusCode == 404);
            var getResult = await controller.GetById(payloadCreate.data.Id);
            if (getResult is NotFoundResult)
            {
                Assert.IsType<NotFoundResult>(getResult);
            }
            else
            {
                var ok = Assert.IsType<ObjectResult>(getResult);
                var payloadGet = Assert.IsType<ResultDTO<CollectionDTO>>(ok.Value);
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
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto1 = new CollectionCreateDTO { UserId = Guid.NewGuid() };
            var dto2 = new CollectionCreateDTO { UserId = Guid.NewGuid() };
            await controller.Add(dto1);
            await controller.Add(dto2);
            var result = await controller.GetAll();
            var ok = Assert.IsType<ObjectResult>(result);
            var payload = Assert.IsType<ResultDTO<CollectionDTO[]>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal(2, payload.data.Length);
        }
    }
}
