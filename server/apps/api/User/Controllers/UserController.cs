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
		public async Task<ActionResult<List<UserDTO>>> GetAll()
		{
			var users = await _service.GetAllAsync();
			return Ok(users);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<UserDTO>> GetById(Guid id)
		{
			var user = await _service.GetByIdAsync(id);
			if (user == null) return NotFound();
			return Ok(user);
		}

		[HttpPost]
		public async Task<ActionResult<ResultDTO<UserDTO>>> Add([FromBody] UserCreateDTO dto)
		{
			var result = await _service.CreateAsync(dto);
			if (!result.success)
				return StatusCode(result.statusCode, result);
			return CreatedAtAction(nameof(GetById), new { id = result.data!.Id }, result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<ResultDTO<UserDTO>>> Update(Guid id, [FromBody] UserCreateDTO dto)
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
