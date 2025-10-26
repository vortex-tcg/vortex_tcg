using Microsoft.EntityFrameworkCore;
using Scrypt;
using System.Text.RegularExpressions;
using VortexTCG.Auth.DTOs;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.Providers;

namespace VortexTCG.Auth.Services
{
	public class RegisterService
	{
		private readonly UserProvider _userProvider;
		private static readonly Regex PasswordRegex = new(
			pattern: @"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""':{}|<>]).{8,}$",
			options: RegexOptions.Compiled
		);

		public RegisterService(VortexDbContext db)
		{
			_userProvider = new UserProvider(db);
		}

		public record RegisterResult(bool Success, int StatusCode, string Message);

		public async Task<RegisterResult> RegisterAsync(RegisterDTO request, CancellationToken ct = default)
		{
			var nullCheck = CheckNullFields(request);
			if (nullCheck is not null)
				return nullCheck;

			var passwordCheck = CheckPassword(request.password, request.password_confirmation);
			if (passwordCheck is not null)
				return passwordCheck;

			var uniquenessCheck = await CheckUniquenessAsync(request, ct);
			if (uniquenessCheck is not null)
				return uniquenessCheck;

			var encoder = new ScryptEncoder();
			string hashedPassword = encoder.Encode(request.password);

			var user = new User
			{
				FirstName = request.first_name,
				LastName = request.last_name,
				Username = request.username,
				Email = request.email,
				Password = hashedPassword,
				Language = "fr",
				CurrencyQuantity = 0,
			};

			_userProvider.AddUser(user);
			await _userProvider.SaveChangesAsync(ct);

			return new(true, 201, "Utilisateur créé avec succès ✅");
		}

		private RegisterResult? CheckNullFields(RegisterDTO request)
		{
			if (string.IsNullOrWhiteSpace(request.first_name) ||
				string.IsNullOrWhiteSpace(request.last_name) ||
				string.IsNullOrWhiteSpace(request.username) ||
				string.IsNullOrWhiteSpace(request.email) ||
				string.IsNullOrWhiteSpace(request.password) ||
				string.IsNullOrWhiteSpace(request.password_confirmation))
			{
				return new(false, 400, "Tous les champs sont requis.");
			}
			return null;
		}

	private RegisterResult? CheckPassword(string password, string confirmation)
		{
			if (password != confirmation)
				return new(false, 400, "Les mots de passe ne correspondent pas.");

			if (!PasswordRegex.IsMatch(password))
				return new(false, 400, "Le mot de passe doit contenir au minimum 8 caractères, une majuscule, un chiffre et un caractère spécial.");

			return null;
		}

		private async Task<RegisterResult?> CheckUniquenessAsync(RegisterDTO request, CancellationToken ct)
		{
			if (await _userProvider.EmailExistsAsync(request.email, ct))
				return new(false, 409, "Email déjà utilisé.");

			if (await _userProvider.UsernameExistsAsync(request.username, ct))
				return new(false, 409, "Nom d'utilisateur déjà pris.");

			return null;
		}
	}
}

