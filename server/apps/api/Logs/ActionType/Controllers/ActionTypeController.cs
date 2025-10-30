
using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.Logs.ActionType.DTOs;
using VortexTCG.Api.Logs.ActionType.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using ActionTypeModel = VortexTCG.DataAccess.Models.ActionType;

namespace VortexTCG.Api.Logs.ActionType.Controllers
{
	[ApiController]
	[Route("api/actiontype")]
	public class ActionTypeController : VortexBaseController
	{
		private readonly ActionTypeService _service;
		public ActionTypeController(ActionTypeService service)
		{
			_service = service;
		}

		[HttpGet]
		public ActionResult<List<ActionTypeDTO>> GetAll()
		{
			var actions = _service.GetAllAsync();
			return Ok(actions);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<ActionTypeDTO>> GetById(Guid id)
		{
			var action = await _service.GetByIdAsync(id);
			if (action == null) return NotFound();
			return Ok(action);
		}

		[HttpPost]
		public async Task<ActionResult<ResultDTO<ActionTypeDTO>>> Add([FromBody] ActionTypeCreateDTO dto)
		{
			var result = await _service.CreateAsync(dto);
			if (!result.success)
				return StatusCode(result.statusCode, result);
			return CreatedAtAction(nameof(GetById), new { id = result.data!.Id }, result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<ResultDTO<ActionTypeDTO>>> Update(Guid id, [FromBody] ActionTypeCreateDTO dto)
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
			return Ok(result);
		}
	}
}
