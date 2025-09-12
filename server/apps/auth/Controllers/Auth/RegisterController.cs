using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Scrypt;
using System.Text.RegularExpressions;
using VortexTCG.Auth.DTOs;

namespace VortexTCG.Auth.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion de l’inscription des utilisateurs.
    /// </summary>
    /// <remarks>
    /// Ce contrôleur gère la validation des données fournies par l’utilisateur,
    /// la vérification des doublons (email et nom d’utilisateur),
    /// le hashage sécurisé du mot de passe,
    /// et la création d’un nouvel utilisateur en base de données.
    /// </remarks>
    [ApiController]
    [Route("api/auth/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly VortexDbContext _db;

        /// <summary>
        /// Constructeur du <see cref="RegisterController"/>.
        /// </summary>
        /// <param name="db">Contexte de base de données injecté pour accéder aux utilisateurs et autres entités.</param>
        public RegisterController(VortexDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur dans le système.
        /// </summary>
        /// <remarks>
        /// Étapes effectuées par cette méthode :
        /// <list type="bullet">
        /// <item><description>Vérifie que tous les champs requis sont remplis</description></item>
        /// <item><description>Valide la correspondance entre mot de passe et confirmation</description></item>
        /// <item><description>Vérifie que le mot de passe respecte les critères de sécurité (longueur, majuscule, chiffre, caractère spécial)</description></item>
        /// <item><description>S’assure que l’email et le nom d’utilisateur ne sont pas déjà utilisés</description></item>
        /// <item><description>Hash le mot de passe avec Scrypt avant enregistrement</description></item>
        /// <item><description>Ajoute l’utilisateur à la base et retourne un DTO de réponse</description></item>
        /// </list>
        /// </remarks>
        /// <param name="dto">Objet <see cref="UserRegisterDTO"/> contenant les informations d’inscription.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item><description><see cref="BadRequestObjectResult"/> si un champ est manquant ou invalide</description></item>
        /// <item><description><see cref="ConflictObjectResult"/> si l’email ou le nom d’utilisateur est déjà pris</description></item>
        /// <item><description><see cref="OkObjectResult"/> contenant un <see cref="UserResponseDTO"/> si l’inscription réussit</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="InvalidOperationException">Si une erreur inattendue survient lors de la sauvegarde en base.</exception>
        /// <example>
        /// Exemple d’appel :
        /// <code>
        /// var request = new UserRegisterDTO
        /// {
        ///     FirstName = "John",
        ///     LastName = "Doe",
        ///     Username = "johndoe",
        ///     Email = "john.doe@example.com",
        ///     Password = "Password1!",
        ///     PasswordConfirmation = "Password1!"
        /// };
        /// var result = await controller.Register(request);
        /// </code>
        /// </example>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            // Vérification des champs requis
            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.PasswordConfirmation))
            {
                return BadRequest(new { error = "Tous les champs sont requis." });
            }

            // Vérifie la correspondance des mots de passe
            if (dto.Password != dto.PasswordConfirmation)
            {
                return BadRequest(new { error = "Les mots de passe ne correspondent pas." });
            }

            // Vérifie la complexité du mot de passe
            var passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>]).{8,}$";
            if (!Regex.IsMatch(dto.Password, passwordPattern))
            {
                return BadRequest(new { error = "Le mot de passe doit contenir au minimum 8 caractères, une majuscule, un chiffre et un caractère spécial." });
            }

            // Vérifie l’unicité de l’email et du nom d’utilisateur
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

            // Création de l’entité utilisateur
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Username = dto.Username,
                Email = dto.Email,
                Password = hashedPassword,
                Language = "fr",          // Langue par défaut
                CurrencyQuantity = 0,     // Valeur initiale
                RoleId = 2,               // Rôle "User" par défaut
                RankId = 1                // Rang par défaut
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Conversion en DTO de réponse
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
