using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Scrypt;

namespace VortexTCG.Auth.Controllers;

[ApiController]
[Route("api/auth/[controller]")]

public class LoginController : ControllerBase
{
    public class LoginData
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
    }

    private readonly VortexDbContext _db;

    public LoginController(VortexDbContext db)
    {
        _db = db;
    }
    
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginData data)
    {
        if (data == null ||
            string.IsNullOrWhiteSpace(data.Email) ||
            string.IsNullOrWhiteSpace(data.Password))
        {
            return BadRequest(new { error = "Email ou mot de passe sont requis." });
        }

        var user = await _db.Users
            .SingleOrDefaultAsync(u => u.Email == data.Email);

        if (user == null)
        {
            return Unauthorized(new { error = "Invalid credentials." });
        }

        var encoder = new ScryptEncoder();
        var passwordMatches = false;

        try
        {
            passwordMatches = encoder.Compare(data.Password, user.Password);
        }
        catch (Exception ex)
        {
            passwordMatches = false;
        }

        if (!passwordMatches)
            return Unauthorized(new { error = $"Invalid password: {ex.Message}" });

        var result = new LoginResponse
        {
            Id = user.Id,
            Username = user.Username,
        };

        return Ok(result);
    }
}
