using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.Services;
using VortexTCG.Auth.DTOs;

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
            try
            {
                return Ok(await _login_service.login(data));
            }
            catch (Exception error)
            {
                switch (error.Message)
                {
                    case "UNAUTHORIZED":
                        return Unauthorized(new { error = "Invalid credentials." });
                    case "BAD_REQUEST":
                        return BadRequest(new { error = "Email ou mot de passe sont requis." });
                    default:
                        return BadRequest(new { error = error.Message });
                }
            }
        }
    }
}