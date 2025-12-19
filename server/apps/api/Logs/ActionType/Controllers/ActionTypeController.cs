
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
		public async Task<IActionResult> GetAll()
		{
			var result = await _service.GetAllAsync();
			return toActionResult<ActionTypeDTO[]>(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var data = await _service.GetByIdAsync(id);
			var result = new ResultDTO<ActionTypeDTO> { success = data != null, statusCode = data != null ? 200 : 404, data = data };
			return toActionResult<ActionTypeDTO>(result);
		}

		[HttpPost]
		public async Task<IActionResult> Add([FromBody] ActionTypeCreateDTO dto)
		{
			var result = await _service.CreateAsync(dto);
			return toActionResult<ActionTypeDTO>(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] ActionTypeCreateDTO dto)
		{
			var result = await _service.UpdateAsync(id, dto);
			return toActionResult<ActionTypeDTO>(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _service.DeleteAsync(id);
			return toActionResult<object>(result);
		}
	}
}
