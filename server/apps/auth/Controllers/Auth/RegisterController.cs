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
        private readonly VortexDbContext _db_context;

        /// <summary>
        /// Constructeur du <see cref="RegisterController"/>.
        /// </summary>
        /// <param name="db_context">Contexte de base de données injecté pour accéder aux utilisateurs et autres entités.</param>
        public RegisterController(VortexDbContext db_context)
        {
            _db_context = db_context;
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
        public async Task<IActionResult> register([FromBody] UserRegisterDTO user_register_dto)
        {
            // Vérification des champs requis
            if (string.IsNullOrWhiteSpace(user_register_dto.first_name) ||
                string.IsNullOrWhiteSpace(user_register_dto.last_name) ||
                string.IsNullOrWhiteSpace(user_register_dto.username) ||
                string.IsNullOrWhiteSpace(user_register_dto.email) ||
                string.IsNullOrWhiteSpace(user_register_dto.password) ||
                string.IsNullOrWhiteSpace(user_register_dto.password_confirmation))
            {
                return BadRequest(new { error = "Tous les champs sont requis." });
            }

            // Vérifie la correspondance des mots de passe
            if (user_register_dto.password != user_register_dto.password_confirmation)
            {
                return BadRequest(new { error = "Les mots de passe ne correspondent pas." });
            }

            // Vérifie la complexité du mot de passe
            var password_pattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>]).{8,}$";
            if (!Regex.IsMatch(user_register_dto.password, password_pattern))
            {
                return BadRequest(new { error = "Le mot de passe doit contenir au minimum 8 caractères, une majuscule, un chiffre et un caractère spécial." });
            }

            // Vérifie l’unicité de l’email et du nom d’utilisateur
            if (_db_context.Users.Any(u => u.Email == user_register_dto.email))
            {
                return Conflict(new { error = "Email déjà utilisé." });
            }

            if (_db_context.Users.Any(u => u.Username == user_register_dto.username))
            {
                return Conflict(new { error = "Nom d'utilisateur déjà pris." });
            }

            // Hash du mot de passe
            var scrypt_encoder = new ScryptEncoder();
            var hashed_password = scrypt_encoder.Encode(user_register_dto.password);

            // Création de l’entité utilisateur
            var user = new User
            {
                FirstName = user_register_dto.first_name,
                LastName = user_register_dto.last_name,
                Username = user_register_dto.username,
                Email = user_register_dto.email,
                Password = hashed_password,
                Language = "fr",          // Langue par défaut
                CurrencyQuantity = 0,     // Valeur initiale
                RoleId = 2,               // Rôle "User" par défaut
                RankId = 1                // Rang par défaut
            };

            _db_context.Users.Add(user);
            await _db_context.SaveChangesAsync();

            // Conversion en DTO de réponse
            var user_response_dto = new UserResponseDTO
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                first_name = user.FirstName,
                last_name = user.LastName,
                language = user.Language,
                currency_quantity = user.CurrencyQuantity
            };

            return Ok(user_response_dto);
        }
    }
}
