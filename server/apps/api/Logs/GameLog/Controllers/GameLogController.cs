// ...existing code...
// Utilisation de GameLogModel si besoin dans le contrôleur
using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Logs.GameLog.DTOs;
using VortexTCG.Api.Logs.GameLog.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;

namespace VortexTCG.Api.Logs.GameLog.Controllers
{
	[ApiController]
	[Route("api/gamelog")]
	public class GameLogController : VortexBaseController
	{
		private readonly GameLogService _service;
		public GameLogController(GameLogService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var result = await _service.GetAllAsync();
			return toActionResult<GameLogDTO[]>(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var data = await _service.GetByIdAsync(id);
			var result = new ResultDTO<GameLogDTO> { success = data != null, statusCode = data != null ? 200 : 404, data = data };
			return toActionResult<GameLogDTO>(result);
		}

		[HttpPost]
		public async Task<IActionResult> Add([FromBody] GameLogCreateDTO dto)
		{
			var result = await _service.CreateAsync(dto);
			return toActionResult<GameLogDTO>(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] GameLogCreateDTO dto)
		{
			var result = await _service.UpdateAsync(id, dto);
			return toActionResult<GameLogDTO>(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _service.DeleteAsync(id);
			return toActionResult<object>(result);
		}
	}
}
