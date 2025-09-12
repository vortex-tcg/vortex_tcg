using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.DTOs; // DTOs unifiés pour la sérialisation et désérialisation
using Scrypt;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VortexTCG.Auth.Controllers;

/// <summary>
/// Contrôleur responsable de l’authentification des utilisateurs.
/// Fournit une route d’API permettant la connexion via email et mot de passe,
/// et génère un jeton JWT en cas de succès.
/// </summary>
[ApiController]
[Route("api/auth/[controller]")]
public class LoginController : ControllerBase
{
    private readonly VortexDbContext _db;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initialise une nouvelle instance du <see cref="LoginController"/>.
    /// </summary>
    /// <param name="db">Le contexte de base de données injecté.</param>
    /// <param name="configuration">La configuration de l’application (utilisée pour la clé JWT).</param>
    public LoginController(VortexDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    /// <summary>
    /// Tente de connecter un utilisateur à partir de ses identifiants.
    /// </summary>
    /// <param name="request">Objet contenant l’email et le mot de passe fournis par l’utilisateur.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description><see cref="BadRequestObjectResult"/> si l’email ou le mot de passe est vide.</description></item>
    /// <item><description><see cref="UnauthorizedObjectResult"/> si les identifiants sont incorrects.</description></item>
    /// <item><description><see cref="OkObjectResult"/> contenant un <see cref="UserResponseDTO"/> si la connexion réussit.</description></item>
    /// </list>
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO request)
    {
        // Vérification basique des champs obligatoires
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email ou mot de passe sont requis." });
        }

        // Recherche de l’utilisateur correspondant à l’email
        var user = await _db.Users
            .Include(u => u.Role) // On charge également le rôle pour la réponse
            .SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new { error = "Identifiants invalides." });

        var encoder = new ScryptEncoder();
        bool passwordMatches;

        try
        {
            // Vérification du mot de passe fourni avec le hash stocké
            passwordMatches = encoder.Compare(request.Password, user.Password);
        }
        catch
        {
            // En cas de problème avec la comparaison, on considère le mot de passe invalide
            passwordMatches = false;
        }

        if (!passwordMatches)
            return Unauthorized(new { error = "Identifiants invalides." });

        // Génération du token JWT
        var token = GenerateAccessToken(user.Username);

        // Préparation de la réponse normalisée
        var response = new UserResponseDTO
        {
            Id = user.Id,
            Username = user.Username,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = user.Role?.Label ?? "User" // Retourne le label du rôle (ex: "User", "Admin")
        };

        return Ok(response);
    }

    /// <summary>
    /// Génère un jeton JWT pour un utilisateur donné.
    /// </summary>
    /// <param name="userName">Nom d’utilisateur (inclus comme claim dans le token).</param>
    /// <returns>Un objet <see cref="JwtSecurityToken"/> signé et valide.</returns>
    /// <exception cref="InvalidOperationException">Si la clé secrète JWT n’est pas configurée.</exception>
    private JwtSecurityToken GenerateAccessToken(string userName)
    {
        // Définition des informations (claims) contenues dans le token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName)
        };

        // Récupération de la clé secrète depuis la configuration
        var secretKey = _configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured.");

        var key = Encoding.UTF8.GetBytes(secretKey);

        // Construction du token avec une durée de validité de 2 heures
        return new JwtSecurityToken(
            claims: claims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            expires: DateTime.UtcNow.AddHours(2)
        );
    }
}
