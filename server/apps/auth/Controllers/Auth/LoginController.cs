using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Scrypt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VortexTCG.Auth.Controllers;

[ApiController]
[Route("api/auth/[controller]")]
public class LoginController(VortexDbContext db, IConfiguration configuration) : ControllerBase
{
    private readonly VortexDbContext _db = db;
    private readonly IConfiguration _configuration = configuration;

    public class LoginData
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string? Token { get; set; } 
        public string? Role { get; set; }
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
            return Unauthorized(new { error = "Invalid credentials." });

        var encoder = new ScryptEncoder();
        var passwordMatches = false;

        try
        {
            passwordMatches = encoder.Compare(data.Password, user.Password);
        }
        catch
        {
            passwordMatches = false;
        }

        if (!passwordMatches)
            return Unauthorized(new { error = "Invalid credentials." });

        var token = GenerateAccessToken(user.Username);

        var result = new LoginResponse
        {
            Id = user.Id,
            Username = user.Username,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = user.RoleId.ToString() 
        };

        return Ok(result);
    }

    private JwtSecurityToken GenerateAccessToken(string userName)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName)
        };

        var secretKey = _configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured.");

        var key = Encoding.UTF8.GetBytes(secretKey);
        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
}
