#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Rank.Controllers;
using VortexTCG.Api.Rank.DTOs;
using VortexTCG.Api.Rank.Providers;
using VortexTCG.Api.Rank.Services;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace Tests.Rank
{
    public class RankControllerTest
    {
        private VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private RankController CreateController(VortexDbContext db)
        {
            var provider = new RankProvider(db);
            var service = new RankService(provider);
            return new RankController(service);
        }

        private class TestCurrentUserService : VortexTCG.Common.Services.ICurrentUserService
        {
            public Guid UserId => Guid.Empty;
            public string Username => "test";
            public string Role => "USER";
            public string GetCurrentUsername() => "test";
        }

        [Fact]
        public async Task CreateRank_ReturnsCreated()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new RankCreateDTO { Label = "Bronze", nbVictory = 0 };
            var result = await controller.Add(dto);
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var payload = Assert.IsType<ResultDTO<RankDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.Equal("Bronze", payload.data.Label);
        }

        [Fact]
        public async Task CreateRank_DuplicateLabel_ReturnsConflict()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new RankCreateDTO { Label = "Bronze", nbVictory = 0 };
            await controller.Add(dto);
            var result = await controller.Add(dto);
            var conflict = Assert.IsType<ObjectResult>(result.Result);
            var payload = Assert.IsType<ResultDTO<RankDTO>>(conflict.Value);
            Assert.False(payload.success);
            Assert.Equal(409, payload.statusCode);
        }

        [Fact]
        public async Task GetById_ReturnsRank()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new RankCreateDTO { Label = "Silver", nbVictory = 10 };
            var createResult = await controller.Add(dto);
            var created = ((ResultDTO<RankDTO>)((CreatedAtActionResult)createResult.Result).Value).data;
            var getResult = await controller.GetById(created.Id);
            var ok = Assert.IsType<OkObjectResult>(getResult.Result);
            var payload = Assert.IsType<ResultDTO<RankDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Silver", payload.data.Label);
            Assert.Equal(10, payload.data.nbVictory);
        }

        [Fact]
        public async Task UpdateRank_ChangesValues()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new RankCreateDTO { Label = "Gold", nbVictory = 20 };
            var createResult = await controller.Add(dto);
            var created = ((ResultDTO<RankDTO>)((CreatedAtActionResult)createResult.Result).Value).data;
            var updateDto = new RankCreateDTO { Label = "Platinum", nbVictory = 30 };
            var updateResult = await controller.Update(created.Id, updateDto);
            var ok = Assert.IsType<OkObjectResult>(updateResult.Result);
            var payload = Assert.IsType<ResultDTO<RankDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Platinum", payload.data.Label);
            Assert.Equal(30, payload.data.nbVictory);
        }

        [Fact]
        public async Task DeleteRank_RemovesRank()
        {
            using var db = CreateDb();
            var controller = CreateController(db);
            var dto = new RankCreateDTO { Label = "Diamond", nbVictory = 50 };
            var createResult = await controller.Add(dto);
            var created = ((ResultDTO<RankDTO>)((CreatedAtActionResult)createResult.Result).Value).data;
            var deleteResult = await controller.Delete(created.Id);
            var deleted = Assert.IsType<ObjectResult>(deleteResult.Result);
            var payload = Assert.IsType<ResultDTO<object>>(deleted.Value);
            Assert.False(payload.success == false && payload.statusCode == 404);
            var getResult = await controller.GetById(created.Id);
            if (getResult.Result is NotFoundResult)
            {
                Assert.IsType<NotFoundResult>(getResult.Result);
            }
            else
            {
                var ok = Assert.IsType<OkObjectResult>(getResult.Result);
                var payloadGet = Assert.IsType<ResultDTO<RankDTO>>(ok.Value);
                Assert.False(payloadGet.success);
                Assert.Equal(404, payloadGet.statusCode);
            }
        }
    }
}
