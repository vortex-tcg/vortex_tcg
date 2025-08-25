using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using BCrypt.Net;

namespace VortexTCG.Auth.Controllers
{
    [ApiController]
    [Route("api/auth/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly VortexDbContext _db;

        public RegisterController(VortexDbContext db)
        {
            _db = db;
        }

        public class RegisterRequest
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string PasswordConfirmation { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Vérif basiques
            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.PasswordConfirmation))
            {
                return BadRequest(new { error = "Tous les champs sont requis." });
            }

            if (request.Password != request.PasswordConfirmation)
            {
                return BadRequest(new { error = "Les mots de passe ne correspondent pas." });
            }

            // Vérif email ou username déjà utilisés
            if (_db.Users.Any(u => u.Email == request.Email))
            {
                return Conflict(new { error = "Email déjà utilisé." });
            }

            if (_db.Users.Any(u => u.Username == request.Username))
            {
                return Conflict(new { error = "Nom d'utilisateur déjà pris." });
            }

            // Hash du password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,
                Language = "fr", // Langue par défaut
                CurrencyQuantity = 0, // Quantité de monnaie par défaut
                RoleId = 2, // Rôle par défaut (utilisateur)
                RankId = 1, // Rang par défaut (peut être ajusté selon votre logique)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Utilisateur créé avec succès ✅" });
        }
    }
}
