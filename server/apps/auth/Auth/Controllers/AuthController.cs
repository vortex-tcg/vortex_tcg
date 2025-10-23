using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.Services;
using VortexTCG.Auth.DTOs;
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

        public AuthController(VortexDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _login_service = new LoginService(_db, _configuration);
        }

        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] LoginDTO data)
        {
            ResultDTO<LoginResponseDTO> result = await _login_service.login(data);
            if (result.success && result.data != null)
            {
                return Ok(result.data);
            }
            return StatusCode(result.statusCode, new { result.message });
        }
    }
}