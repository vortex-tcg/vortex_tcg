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
    private readonly VortexDbContext _db_context;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initialise une nouvelle instance du <see cref="LoginController"/>.
    /// </summary>
    /// <param name="db">Le contexte de base de données injecté.</param>
    /// <param name="configuration">La configuration de l’application (utilisée pour la clé JWT).</param>
    public LoginController(VortexDbContext db_context, IConfiguration configuration)
    {
        _db_context = db_context;
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
    public async Task<IActionResult> login([FromBody] UserLoginDTO request)
    {
        // Vérification basique des champs obligatoires
        if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
        {
            return BadRequest(new { error = "Email ou mot de passe sont requis." });
        }

        // Recherche de l’utilisateur correspondant à l’email
        var user = await _db_context.Users
            .Include(u => u.Role) // On charge également le rôle pour la réponse
            .SingleOrDefaultAsync(u => u.Email == request.email);

        if (user == null)
            return Unauthorized(new { error = "Identifiants invalides." });

        var scrypt_encoder = new ScryptEncoder();
        bool password_matches;

        try
        {
            // Vérification du mot de passe fourni avec le hash stocké
            password_matches = scrypt_encoder.Compare(request.password, user.Password);
        }
        catch
        {
            // En cas de problème avec la comparaison, on considère le mot de passe invalide
            password_matches = false;
        }

        if (!password_matches)
            return Unauthorized(new { error = "Identifiants invalides." });

        // Génération du token JWT
        var jwt_token = generateAccessToken(user.Username);

        // Préparation de la réponse normalisée
        var response_dto = new UserResponseDTO
        {
            id = user.Id,
            username = user.Username,
            token = new JwtSecurityTokenHandler().WriteToken(jwt_token),
            role = user.Role?.Label ?? "User" // Retourne le label du rôle (ex: "User", "Admin")
        };

        return Ok(response_dto);
    }

    /// <summary>
    /// Génère un jeton JWT pour un utilisateur donné.
    /// </summary>
    /// <param name="user_name">Nom d’utilisateur (inclus comme claim dans le token).</param>
    /// <returns>Un objet <see cref="JwtSecurityToken"/> signé et valide.</returns>
    /// <exception cref="InvalidOperationException">Si la clé secrète JWT n’est pas configurée.</exception>
    private JwtSecurityToken generateAccessToken(string user_name)
    {
        // Définition des informations (claims) contenues dans le token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user_name)
        };

        // Récupération de la clé secrète depuis la configuration
        var secret_key = _configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secret_key))
            throw new InvalidOperationException("JWT SecretKey is not configured.");

        var key_bytes = Encoding.UTF8.GetBytes(secret_key);

        // Construction du token avec une durée de validité de 2 heures
        return new JwtSecurityToken(
            claims: claims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key_bytes), SecurityAlgorithms.HmacSha256),
            expires: DateTime.UtcNow.AddHours(2)
        );
    }
}
