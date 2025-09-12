using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Scrypt;
using System.Text.RegularExpressions;
using VortexTCG.Auth.DTOs;

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

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            // Vérif basiques
            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.PasswordConfirmation))
            {
                return BadRequest(new { error = "Tous les champs sont requis." });
            }

            if (dto.Password != dto.PasswordConfirmation)
            {
                return BadRequest(new { error = "Les mots de passe ne correspondent pas." });
            }

            // Vérif sécurité du mot de passe
            var passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>]).{8,}$";
            if (!Regex.IsMatch(dto.Password, passwordPattern))
            {
                return BadRequest(new { error = "Le mot de passe doit contenir au minimum 8 caractères, une majuscule, un chiffre et un caractère spécial." });
            }

            // Vérif email ou username déjà utilisés
            if (_db.Users.Any(u => u.Email == dto.Email))
            {
                return Conflict(new { error = "Email déjà utilisé." });
            }

            if (_db.Users.Any(u => u.Username == dto.Username))
            {
                return Conflict(new { error = "Nom d'utilisateur déjà pris." });
            }

            // Hash du mot de passe
            var encoder = new ScryptEncoder();
            var hashedPassword = encoder.Encode(dto.Password);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                Email = dto.Email,
                Password = hashedPassword,
                Language = "fr",
                CurrencyQuantity = 0,
                RoleId = 2, // Rôle par défaut
                RankId = 1  // Rang par défaut
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Mapping vers DTO de réponse
            var response = new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Language = user.Language,
                CurrencyQuantity = user.CurrencyQuantity
            };

            return Ok(response);
        }
    }
}
