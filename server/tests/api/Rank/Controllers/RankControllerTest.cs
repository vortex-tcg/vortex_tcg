#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Rank.Controllers;
using VortexTCG.Api.Rank.DTOs;
using VortexTCG.Api.Rank.Providers;
using VortexTCG.Api.Rank.Services;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace VortexTCG.Tests.Api.Rank.Controllers
{
    public class RankControllerTest
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static RankController CreateController(VortexDbContext db)
        {
            RankProvider provider = new RankProvider(db);
            RankService service = new RankService(provider);
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
            using VortexDbContext db = CreateDb();
            RankController controller = CreateController(db);
            RankCreateDTO dto = new RankCreateDTO { Label = "Bronze", nbVictory = 0 };
            IActionResult result = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(result);
            ResultDTO<RankDTO> payload = Assert.IsType<ResultDTO<RankDTO>>(created.Value);
            Assert.True(payload.success);
            Assert.Equal(201, payload.statusCode);
            Assert.Equal("Bronze", payload.data.Label);
        }

        [Fact]
        public async Task CreateRank_DuplicateLabel_ReturnsConflict()
        {
            using VortexDbContext db = CreateDb();
            RankController controller = CreateController(db);
            RankCreateDTO dto = new RankCreateDTO { Label = "Bronze", nbVictory = 0 };
            await controller.Add(dto);
            IActionResult result = await controller.Add(dto);
            ObjectResult conflict = Assert.IsType<ObjectResult>(result);
            ResultDTO<RankDTO> payload = Assert.IsType<ResultDTO<RankDTO>>(conflict.Value);
            Assert.False(payload.success);
            Assert.Equal(409, payload.statusCode);
        }

        [Fact]
        public async Task GetById_ReturnsRank()
        {
            using VortexDbContext db = CreateDb();
            RankController controller = CreateController(db);
            RankCreateDTO dto = new RankCreateDTO { Label = "Silver", nbVictory = 10 };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<RankDTO> payloadCreate = Assert.IsType<ResultDTO<RankDTO>>(created.Value);
            IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
            ObjectResult ok = Assert.IsType<ObjectResult>(getResult);
            ResultDTO<RankDTO> payload = Assert.IsType<ResultDTO<RankDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Silver", payload.data.Label);
            Assert.Equal(10, payload.data.nbVictory);
        }

        [Fact]
        public async Task UpdateRank_ChangesValues()
        {
            using VortexDbContext db = CreateDb();
            RankController controller = CreateController(db);
            RankCreateDTO dto = new RankCreateDTO { Label = "Gold", nbVictory = 20 };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<RankDTO> payloadCreate = Assert.IsType<ResultDTO<RankDTO>>(created.Value);
            RankCreateDTO updateDto = new RankCreateDTO { Label = "Platinum", nbVictory = 30 };
            IActionResult updateResult = await controller.Update(payloadCreate.data.Id, updateDto);
            ObjectResult ok = Assert.IsType<ObjectResult>(updateResult);
            ResultDTO<RankDTO> payload = Assert.IsType<ResultDTO<RankDTO>>(ok.Value);
            Assert.True(payload.success);
            Assert.Equal("Platinum", payload.data.Label);
            Assert.Equal(30, payload.data.nbVictory);
        }

        [Fact]
        public async Task DeleteRank_RemovesRank()
        {
            using VortexDbContext db = CreateDb();
            RankController controller = CreateController(db);
            RankCreateDTO dto = new RankCreateDTO { Label = "Diamond", nbVictory = 50 };
            IActionResult createResult = await controller.Add(dto);
            ObjectResult created = Assert.IsType<ObjectResult>(createResult);
            ResultDTO<RankDTO> payloadCreate = Assert.IsType<ResultDTO<RankDTO>>(created.Value);
            IActionResult deleteResult = await controller.Delete(payloadCreate.data.Id);
            ObjectResult deleted = Assert.IsType<ObjectResult>(deleteResult);
            ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(deleted.Value);
            Assert.False(payload.success == false && payload.statusCode == 404);
            IActionResult getResult = await controller.GetById(payloadCreate.data.Id);
            if (getResult is NotFoundResult)
            {
                Assert.IsType<NotFoundResult>(getResult);
            }
            else
            {
                ObjectResult ok = Assert.IsType<ObjectResult>(getResult);
                ResultDTO<RankDTO> payloadGet = Assert.IsType<ResultDTO<RankDTO>>(ok.Value);
                if (payloadGet != null)
                {
                    Assert.False(payloadGet.success);
                    Assert.Equal(404, payloadGet.statusCode);
                }
            }
        }
    }
}
