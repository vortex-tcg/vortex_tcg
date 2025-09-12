using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.DTOs; // DTOs unifiés
using Scrypt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VortexTCG.Auth.Controllers;

[ApiController]
[Route("api/auth/[controller]")]
public class LoginController : ControllerBase
{
    private readonly VortexDbContext _db;
    private readonly IConfiguration _configuration;

    public LoginController(VortexDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email ou mot de passe sont requis." });
        }

        var user = await _db.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new { error = "Identifiants invalides." });

        var encoder = new ScryptEncoder();
        bool passwordMatches;

        try
        {
            passwordMatches = encoder.Compare(request.Password, user.Password);
        }
        catch
        {
            passwordMatches = false;
        }

        if (!passwordMatches)
            return Unauthorized(new { error = "Identifiants invalides." });

        // Génération du token JWT
        var token = GenerateAccessToken(user.Username);

        var response = new UserResponseDTO
        {
            Id = user.Id,
            Username = user.Username,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = user.Role?.Label ?? "User" // Label lisible plutôt qu'un simple Id
        };

        return Ok(response);
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
        return new JwtSecurityToken(
            claims: claims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            expires: DateTime.UtcNow.AddHours(2) // durée de vie configurable
        );
    }
}
