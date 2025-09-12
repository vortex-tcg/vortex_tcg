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

namespace Tests
{
    public class LoginControllerTest
    {
        private VortexDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new VortexDbContext(options);
        }

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

        [Fact]
        public async Task Login_With_Invalid_Input_Returns_BadRequest()
        {
            var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();
            var controller = new LoginController(db, config);

            var request = new UserLoginDTO
            {
                Email = "",
                Password = ""
            };

            var result = await controller.Login(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var payload = badRequest.Value?.ToString();
            Assert.NotNull(payload);
            Assert.Contains("Email ou mot de passe sont requis", payload);
        }

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

        [Fact]
        public async Task Login_With_Wrong_Password_Returns_Unauthorized()
        {
            var db = GetInMemoryDbContext();
            var config = GetTestConfiguration();

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

            Assert.NotNull(response.Token);
            Assert.Equal("johndoe", response.Username);
            Assert.Equal("User", response.Role); // maintenant retourne le Label du rôle
        }
    }
}
