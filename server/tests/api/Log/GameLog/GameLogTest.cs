#nullable disable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using VortexTCG.Api.Logs.GameLog.Controllers;
using VortexTCG.Api.Logs.GameLog.DTOs;
using VortexTCG.Api.Logs.GameLog.Providers;
using VortexTCG.Api.Logs.GameLog.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.DTO;
using Microsoft.EntityFrameworkCore;

namespace Tests.Logs.GameLog
{
	public class GameLogTest
	{
		private VortexDbContext CreateDb()
		{
			DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			return new VortexDbContext(options);
		}

		private GameLogController CreateController(VortexDbContext db)
		{
			GameLogProvider provider = new GameLogProvider(db);
			GameLogService service = new GameLogService(provider);
			return new GameLogController(service);
		}

		[Fact]
		public async Task CreateGameLog_ReturnsCreated()
		{
			using VortexDbContext db = CreateDb();
			GameLogController controller = CreateController(db);
			GameLogCreateDTO dto = new GameLogCreateDTO {
				Label = "Game 1",
				TurnNumber = 10,
				UserId = null,
				ActionIds = null
			};
			ActionResult<ResultDTO<GameLogDTO>> result = await controller.Add(dto);
			CreatedAtActionResult created = Assert.IsType<CreatedAtActionResult>(result.Result);
			ResultDTO<GameLogDTO> payload = Assert.IsType<ResultDTO<GameLogDTO>>(created.Value);
			Assert.True(payload.success);
			Assert.Equal(201, payload.statusCode);
			Assert.Equal("Game 1", payload.data.Label);
		}

		[Fact]
		public async Task GetById_ReturnsGameLog()
		{
			using VortexDbContext db = CreateDb();
			GameLogController controller = CreateController(db);
			GameLogCreateDTO dto = new GameLogCreateDTO {
				Label = "Game 2",
				TurnNumber = 20,
				UserId = null,
				ActionIds = null
			};
			ActionResult<ResultDTO<GameLogDTO>> createResult = await controller.Add(dto);
			GameLogDTO created = ((ResultDTO<GameLogDTO>)((CreatedAtActionResult)createResult.Result).Value).data;
			ActionResult<GameLogDTO> getResult = await controller.GetById(created.Id);
			OkObjectResult ok = Assert.IsType<OkObjectResult>(getResult.Result);
			GameLogDTO payload = Assert.IsType<GameLogDTO>(ok.Value);
			Assert.Equal("Game 2", payload.Label);
			Assert.Equal(20, payload.TurnNumber);
		}

		[Fact]
		public async Task UpdateGameLog_ChangesValues()
		{
			using VortexDbContext db = CreateDb();
			GameLogController controller = CreateController(db);
			GameLogCreateDTO dto = new GameLogCreateDTO {
				Label = "Game 3",
				TurnNumber = 30,
				UserId = null,
				ActionIds = null
			};
			ActionResult<ResultDTO<GameLogDTO>> createResult = await controller.Add(dto);
			GameLogDTO created = ((ResultDTO<GameLogDTO>)((CreatedAtActionResult)createResult.Result).Value).data;
			GameLogCreateDTO updateDto = new GameLogCreateDTO {
				Label = "Game 3 Updated",
				TurnNumber = 31,
				UserId = null,
				ActionIds = null
			};
			ActionResult<ResultDTO<GameLogDTO>> updateResult = await controller.Update(created.Id, updateDto);
			OkObjectResult ok = Assert.IsType<OkObjectResult>(updateResult.Result);
			ResultDTO<GameLogDTO> payload = Assert.IsType<ResultDTO<GameLogDTO>>(ok.Value);
			Assert.True(payload.success);
			Assert.Equal("Game 3 Updated", payload.data.Label);
			Assert.Equal(31, payload.data.TurnNumber);
		}

		[Fact]
		public async Task DeleteGameLog_RemovesGameLog()
		{
			using VortexDbContext db = CreateDb();
			GameLogController controller = CreateController(db);
			GameLogCreateDTO dto = new GameLogCreateDTO {
				Label = "Game 4",
				TurnNumber = 40,
				UserId = null,
				ActionIds = null
			};
			ActionResult<ResultDTO<GameLogDTO>> createResult = await controller.Add(dto);
			GameLogDTO created = ((ResultDTO<GameLogDTO>)((CreatedAtActionResult)createResult.Result).Value).data;
			ActionResult<ResultDTO<object>> deleteResult = await controller.Delete(created.Id);
			ObjectResult deleted = Assert.IsType<ObjectResult>(deleteResult.Result);
			ResultDTO<object> payload = Assert.IsType<ResultDTO<object>>(deleted.Value);
			Assert.True(payload.success);
			ActionResult<GameLogDTO> getResult = await controller.GetById(created.Id);
			Assert.IsType<NotFoundResult>(getResult.Result);
		}
	}
}
