using Microsoft.AspNetCore.Mvc;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Services;
using VortexTCG.DataAccess;

namespace VortexTCG.Auth.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
	private readonly VortexDbContext _db;
	private readonly IConfiguration _configuration;
	private readonly RegisterService _registerService;

	public AuthController(VortexDbContext db, IConfiguration configuration, RegisterService registerService)
	{
		_db = db;
		_configuration = configuration;
		_registerService = registerService;
	}

	[HttpPost]
	[Route("api/auth/register")]
	public async Task<IActionResult> register([FromBody] RegisterDTO request, CancellationToken ct)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);

		RegisterService.RegisterResult result = await _registerService.RegisterAsync(request, ct);
		return StatusCode(result.StatusCode, new { message = result.Message });
	}
}