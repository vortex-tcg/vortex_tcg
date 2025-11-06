using Microsoft.AspNetCore.Mvc;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Services;
using VortexTCG.DataAccess;
using VortexTCG.Common.DTO;

namespace VortexTCG.Auth.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
		private readonly VortexDbContext _db;
		private readonly IConfiguration _configuration;
		private readonly LoginService _login_service;
		private readonly RegisterService _registerService;

		public AuthController(VortexDbContext db, IConfiguration configuration)
		{
			_db = db;
			_configuration = configuration;
			_login_service = new LoginService(_db, _configuration);
			_registerService = new RegisterService(_db);
		}

		[HttpPost("login")]
		public async Task<IActionResult> login([FromBody] LoginDTO data)
		{
            ResultDTO<LoginResponseDTO> result = await _login_service.login(data);
			return StatusCode(result.statusCode, result);
		}

	[HttpPost("register")]
	public async Task<IActionResult> register([FromBody] RegisterDTO request, CancellationToken ct)
		{
			if (!ModelState.IsValid)
				return ValidationProblem(ModelState);

			RegisterService.RegisterResult result = await _registerService.RegisterAsync(request, ct);
			return StatusCode(result.StatusCode, new { message = result.Message });
		}
	}
}