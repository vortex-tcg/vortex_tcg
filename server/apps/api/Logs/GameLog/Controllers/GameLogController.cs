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
		public ActionResult<List<GameLogDTO>> GetAll()
		{
			var logs = _service.GetAllAsync();
			return Ok(logs);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<GameLogDTO>> GetById(Guid id)
		{
			var log = await _service.GetByIdAsync(id);
			if (log == null) return NotFound();
			return Ok(log);
		}

		[HttpPost]
		public async Task<ActionResult<ResultDTO<GameLogDTO>>> Add([FromBody] GameLogCreateDTO dto)
		{
			var result = await _service.CreateAsync(dto);
			if (!result.success)
				return StatusCode(result.statusCode, result);
			return CreatedAtAction(nameof(GetById), new { id = result.data!.Id }, result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<ResultDTO<GameLogDTO>>> Update(Guid id, [FromBody] GameLogCreateDTO dto)
		{
			var result = await _service.UpdateAsync(id, dto);
			if (!result.success)
				return StatusCode(result.statusCode, result);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult<ResultDTO<object>>> Delete(Guid id)
		{
			var result = await _service.DeleteAsync(id);
			if (!result.success)
				return StatusCode(result.statusCode, result);
			return StatusCode(result.statusCode, result);
		}
	}
}
