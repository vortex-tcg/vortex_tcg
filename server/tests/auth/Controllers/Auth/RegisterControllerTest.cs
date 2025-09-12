using Xunit;
using VortexTCG.Auth.Controllers;
using Microsoft.AspNetCore.Mvc;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Auth.DTOs;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace Tests
{
    /// <summary>
    /// Classe de tests unitaires pour le <see cref="RegisterController"/>.
    /// Vérifie tous les scénarios liés à l'inscription des utilisateurs.
    /// </summary>
    public class RegisterControllerTest
    {
        /// <summary>
        /// Crée un contexte en mémoire pour les tests afin d'isoler la base de données.
        /// </summary>
        private VortexDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // DB unique par test
                .Options;

            return new VortexDbContext(options);
        }

        /// <summary>
        /// Teste la réponse lorsque tous les champs ne sont pas fournis.
        /// </summary>
        [Fact]
        public async Task All_Fields_Register()
        {
            var db = GetInMemoryDbContext();
            var controller = new RegisterController(db);

            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@gmail.com",
                Password = "Password1!",
                PasswordConfirmation = "Password1!"
                // Username manquant exprès
            };

            var result = await controller.Register(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Tous les champs sont requis", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque les mots de passe ne correspondent pas.
        /// </summary>
        [Fact]
        public async Task Password_Match_Register()
        {
            var db = GetInMemoryDbContext();
            var controller = new RegisterController(db);

            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                Password = "Password1!",
                PasswordConfirmation = "DifferentPassword1!"
            };

            var result = await controller.Register(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Les mots de passe ne correspondent pas", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque le mot de passe est trop court ou faible.
        /// </summary>
        [Fact]
        public async Task Password_Not_Long_Enough_Register()
        {
            var db = GetInMemoryDbContext();
            var controller = new RegisterController(db);

            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                Password = "weak",
                PasswordConfirmation = "weak"
            };

            var result = await controller.Register(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Le mot de passe doit contenir au minimum", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque le mot de passe ne contient pas de majuscule.
        /// </summary>
        [Fact]
        public async Task Password_Lower_Case_Register()
        {
            var db = GetInMemoryDbContext();
            var controller = new RegisterController(db);

            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                Password = "alllowercase",
                PasswordConfirmation = "alllowercase"
            };

            var result = await controller.Register(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("majuscule", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque le mot de passe ne contient pas de chiffre.
        /// </summary>
        [Fact]
        public async Task Password_No_Number_Register()
        {
            var db = GetInMemoryDbContext();
            var controller = new RegisterController(db);

            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                Password = "NoNumberPassword!",
                PasswordConfirmation = "NoNumberPassword!"
            };
            var result = await controller.Register(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("chiffre", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque le mot de passe ne contient pas de caractère spécial.
        /// </summary>
        [Fact]
        public async Task Password_No_SpecialChar_Register()
        {
            var db = GetInMemoryDbContext();
            var controller = new RegisterController(db);

            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                Password = "NoSpecialChar1",
                PasswordConfirmation = "NoSpecialChar1"
            };

            var result = await controller.Register(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("caractère spécial", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque l'email est déjà utilisé par un autre utilisateur.
        /// </summary>
        [Fact]
        public async Task Email_Already_Used_Register()
        {
            var db = GetInMemoryDbContext();
            db.Ranks.Add(new Rank { Label = "Bronze", nbVictory = 0 });
            await db.SaveChangesAsync();
            db.Roles.Add(new Role { Label = "User" });
            await db.SaveChangesAsync();
            db.Users.Add(new User
            {
                FirstName = "Existing",
                LastName = "User",
                Username = "existinguser",
                Email = "johndoe@gmail.com",
                Password = "HashedPassword1!",
                Language = "fr",
                CurrencyQuantity = 0,
                RoleId = 1,
                RankId = 1,
                CreatedBy = "System",
            });
            await db.SaveChangesAsync();

            var controller = new RegisterController(db);
            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                Password = "Password1!",
                PasswordConfirmation = "Password1!"
            };

            var result = await controller.Register(request);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var payload = conflictResult.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Email déjà utilisé", payload);
        }

        /// <summary>
        /// Teste la réponse lorsque le nom d'utilisateur est déjà pris.
        /// </summary>
        [Fact]
        public async Task Username_Already_Used_Register()
        {
            var db = GetInMemoryDbContext();
            db.Ranks.Add(new Rank { Label = "Bronze", nbVictory = 0 });
            await db.SaveChangesAsync();
            db.Roles.Add(new Role { Label = "User" });
            await db.SaveChangesAsync();
            db.Users.Add(new User
            {
                FirstName = "Existing",
                LastName = "User",
                Username = "johndoe",
                Email = "johndoe@gmail.com",
                Password = "HashedPassword1!",
                Language = "fr",
                CurrencyQuantity = 0,
                RoleId = 1,
                RankId = 1,
                CreatedBy = "System"
            });
            await db.SaveChangesAsync();
            var controller = new RegisterController(db);
            var request = new UserRegisterDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe2@gmail.com",
                Password = "Password1!",
                PasswordConfirmation = "Password1!"
            };
            var result = await controller.Register(request);
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var payload = conflictResult.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Nom d'utilisateur déjà pris", payload);
        }
    }
}
