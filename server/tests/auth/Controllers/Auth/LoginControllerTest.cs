using Xunit;
using VortexTCG.Auth.Controllers;
using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.DTOs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Scrypt;
using System;
using System.Collections.Generic;

namespace Tests
{
    /// <summary>
    /// Classe de tests unitaires pour le <see cref="LoginController"/>.
    /// </summary>
    public class LoginControllerTest
    {
        /// <summary>
        /// Crée un contexte en mémoire pour les tests afin d'isoler la base de données.
        /// </summary>
        private VortexDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VortexDbContext(options);
        }

        /// <summary>
        /// Fournit une configuration factice pour les tests, notamment pour le secret JWT.
        /// </summary>
        private IConfiguration GetTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                { "JwtSettings:SecretKey", "123soleiljspjesaispaaaaaaaaaahahahahahhahahah" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        /// <summary>
        /// Teste la réponse lorsque l'email ou le mot de passe sont vides.
        /// </summary>
        [Fact]
        public async Task Login_With_Invalid_Input_Returns_BadRequest()
        {
            var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new LoginController(db, config);

            var request = new UserLoginDTO { Email = "", Password = "" };

            var result = await controller.Login(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Email ou mot de passe sont requis", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque l'utilisateur n'existe pas.
        /// </summary>
        [Fact]
        public async Task Login_With_NonExistent_User_Returns_Unauthorized()
        {
            var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new LoginController(db, config);

            var request = new UserLoginDTO
            {
                Email = "nonexistent@example.com",
                Password = "Password1!"
            };

            var result = await controller.Login(request);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var payload = unauthorized.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Identifiants invalides", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque le mot de passe fourni est incorrect.
        /// </summary>
        [Fact]
        public async Task Login_With_Wrong_Password_Returns_Unauthorized()
        {
            var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();

            // Création d'un utilisateur avec mot de passe correct
            var encoder = new ScryptEncoder();
            var hashedPassword = encoder.Encode("CorrectPassword1!");

            db.Ranks.Add(new Rank { Label = "Bronze", nbVictory = 0 });
            db.Roles.Add(new Role { Label = "User" });
            db.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@example.com",
                Password = hashedPassword,
                Language = "fr",
                CurrencyQuantity = 0,
                RoleId = 1,
                RankId = 1,
                CreatedBy = "System",
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var controller = new LoginController(db, config);
            var request = new UserLoginDTO
            {
                Email = "johndoe@example.com",
                Password = "WrongPassword1!"
            };

            var result = await controller.Login(request);
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var payload = unauthorized.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Identifiants invalides", payload);
        }

        /// <summary>
        /// Teste la connexion avec des identifiants corrects.
        /// Vérifie que le token JWT est généré et que les informations utilisateur sont correctes.
        /// </summary>
        [Fact]
        public async Task Login_With_Valid_Credentials_Returns_Ok_With_Token()
        {
            var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();

            var encoder = new ScryptEncoder();
            var hashedPassword = encoder.Encode("Password1!");

            db.Ranks.Add(new Rank { Label = "Bronze", nbVictory = 0 });
            db.Roles.Add(new Role { Label = "User" });
            db.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@example.com",
                Password = hashedPassword,
                Language = "fr",
                CurrencyQuantity = 0,
                RoleId = 1,
                RankId = 1,
                CreatedBy = "System",
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var controller = new LoginController(db, config);
            var request = new UserLoginDTO
            {
                Email = "johndoe@example.com",
                Password = "Password1!"
            };

            var result = await controller.Login(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UserResponseDTO>(okResult.Value);

            // Vérifie que le token est généré et les données utilisateur correctes
            Assert.NotNull(response.Token);
            Assert.Equal("johndoe", response.Username);
            Assert.Equal("User", response.Role);
        }
    }
}
