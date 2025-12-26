using Microsoft.AspNetCore.Mvc;
using VortexTCG.Api.User.DTOs;
using VortexTCG.Api.User.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;

namespace VortexTCG.Api.User.Controllers
{
	[ApiController]
	[Route("api/user")]
	public class UserController : VortexBaseController
	{
		private readonly UserService _service;
		public UserController(UserService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var result = await _service.GetAllAsync();
			return toActionResult<UserDTO[]>(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var data = await _service.GetByIdAsync(id);
			var result = new ResultDTO<UserDTO> { success = data != null, statusCode = data != null ? 200 : 404, data = data };
			return toActionResult<UserDTO>(result);
		}

		[HttpPost]
		public async Task<IActionResult> Add([FromBody] UserCreateDTO dto)
		{
			var result = await _service.CreateAsync(dto);
			return toActionResult<UserDTO>(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UserCreateDTO dto)
		{
			var result = await _service.UpdateAsync(id, dto);
			return toActionResult<UserDTO>(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _service.DeleteAsync(id);
			return toActionResult<object>(result);
		}
	}
}
