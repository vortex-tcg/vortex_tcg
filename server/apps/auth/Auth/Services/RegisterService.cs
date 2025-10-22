using Microsoft.EntityFrameworkCore;
using Scrypt;
using System.Text.RegularExpressions;
using VortexTCG.Auth.DTOs;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Auth.Services
{
	public class RegisterService
	{
		private readonly VortexDbContext _db;
		private readonly ScryptEncoder _encoder = new ScryptEncoder();

		public RegisterService(VortexDbContext db)
		{
			_db = db;
		}

		public record RegisterResult(bool Success, int StatusCode, string Message);

		public async Task<RegisterResult> RegisterAsync(RegisterDTO request, CancellationToken ct = default)
		{
			// Vérifications basiques (double-sécurité au-delà des DataAnnotations)
			if (string.IsNullOrWhiteSpace(request.FirstName) ||
				string.IsNullOrWhiteSpace(request.LastName) ||
				string.IsNullOrWhiteSpace(request.Username) ||
				string.IsNullOrWhiteSpace(request.Email) ||
				string.IsNullOrWhiteSpace(request.Password) ||
				string.IsNullOrWhiteSpace(request.PasswordConfirmation))
			{
				return new(false, 400, "Tous les champs sont requis.");
			}

			if (request.Password != request.PasswordConfirmation)
			{
				return new(false, 400, "Les mots de passe ne correspondent pas.");
			}

			// Sécurité du mot de passe
			var passwordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>]).{8,}$";
			if (!Regex.IsMatch(request.Password, passwordPattern))
			{
				return new(false, 400, "Le mot de passe doit contenir au minimum 8 caractères, une majuscule, un chiffre et un caractère spécial.");
			}

			// Unicité email / username
			if (await _db.Users.AnyAsync(u => u.Email == request.Email, ct))
			{
				return new(false, 409, "Email déjà utilisé.");
			}

			if (await _db.Users.AnyAsync(u => u.Username == request.Username, ct))
			{
				return new(false, 409, "Nom d'utilisateur déjà pris.");
			}

			// Hash & création utilisateur
			String hashedPassword = _encoder.Encode(request.Password);

			User user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Email = request.Email,
                Password = hashedPassword,
                Language = "fr",
				CurrencyQuantity = 0,
                // RankId laissé null par défaut
            };

			_db.Users.Add(user);
			await _db.SaveChangesAsync(ct);

			return new(true, 201, "Utilisateur créé avec succès ✅");
		}
	}
}

