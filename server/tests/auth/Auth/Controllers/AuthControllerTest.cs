using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VortexTCG.Auth.Controllers;
using VortexTCG.DataAccess;
using UserModel = VortexTCG.DataAccess.Models.User;
using VortexTCG.Common.Services;
using VortexTCG.Auth.DTOs;
using Scrypt;
using VortexTCG.Common.DTO;
using RoleEnum = VortexTCG.DataAccess.Models.Role;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Auth.Services;

namespace Tests
{
    public class AuthControllerTest
    {
        private async Task createUser(VortexDbContext db)
        {
            ScryptEncoder encoder = new ScryptEncoder();
            string hashedPassword = encoder.Encode("CorrectPassword1!");

            var rank = db.Ranks.Add(new VortexTCG.DataAccess.Models.Rank { Label = "Bronze", nbVictory = 0 }).Entity;
            db.Users.Add(new UserModel
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@example.com",
                Password = hashedPassword,
                Language = "fr",
                CurrencyQuantity = 0,
                Role = RoleEnum.USER,
                RankId = rank.Id,
                CreatedBy = "System",
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        [Fact(DisplayName = "login with invalid input returns bad request")]
        public async Task loginWithInvalidInputReturnsBadRequest()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            AuthController controller = new AuthController(db, config);

            LoginDTO request = new LoginDTO
            {
                email = "",
                password = ""
            };

            var result = await controller.login(request);
            ObjectResult badRequest = Assert.IsType<ObjectResult>(result);
            ResultDTO<LoginResponseDTO> payload = Assert.IsType<ResultDTO<LoginResponseDTO>>(badRequest.Value);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.NotNull(payload);
            Assert.Contains("Email ou mot de passe sont requis", payload.message);
        }

        [Fact(DisplayName = "login with no email returns bad request")]
        public async Task loginWithNoEmailReturnsBadRequest()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            AuthController controller = new AuthController(db, config);

            LoginDTO request = new LoginDTO
            {
                email = "",
                password = "Password1!"
            };

            IActionResult result = await controller.login(request);
            ObjectResult badRequest = Assert.IsType<ObjectResult>(result);
            ResultDTO<LoginResponseDTO> payload = Assert.IsType<ResultDTO<LoginResponseDTO>>(badRequest.Value);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.NotNull(payload);
            Assert.Contains("Email ou mot de passe sont requis", payload.message);
        }

        [Fact(DisplayName = "login with no password returns bad request")]
        public async Task loginWithNoPasswordReturnsBadRequest()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            AuthController controller = new AuthController(db, config);

            LoginDTO request = new LoginDTO
            {
                email = "nonexistent@example.com",
                password = ""
            };

            IActionResult result = await controller.login(request);
            ObjectResult badRequest = Assert.IsType<ObjectResult>(result);
            ResultDTO<LoginResponseDTO> payload = Assert.IsType<ResultDTO<LoginResponseDTO>>(badRequest.Value);
            Assert.Equal(400, badRequest.StatusCode);
            Assert.NotNull(payload);
            Assert.Contains("Email ou mot de passe sont requis", payload.message);
        }

        [Fact(DisplayName = "login with non existent user returns unauthorized")]
        public async Task loginWithNonExistentUserReturnsUnauthorized()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            AuthController controller = new AuthController(db, config);

            LoginDTO request = new LoginDTO
            {
                email = "nonexistent@example.com",
                password = "Password1!"
            };

            IActionResult result = await controller.login(request);
            ObjectResult unauthorized = Assert.IsType<ObjectResult>(result);
            ResultDTO<LoginResponseDTO> payload = Assert.IsType<ResultDTO<LoginResponseDTO>>(unauthorized.Value);
            Assert.Equal(401, unauthorized.StatusCode);
            Assert.NotNull(payload);
            Assert.Contains("Invalid credentials", payload.message);
        }

        [Fact(DisplayName = "login with wrong password returns unauthorized")]
        public async Task loginWithWrongPasswordReturnsUnauthorized()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();

            await createUser(db);

            AuthController controller = new AuthController(db, config);

            LoginDTO request = new LoginDTO
            {
                email = "johndoe@example.com",
                password = "Password2rzarae!"
            };

            IActionResult result = await controller.login(request);
            ObjectResult unauthorized = Assert.IsType<ObjectResult>(result);
            ResultDTO<LoginResponseDTO> payload = Assert.IsType<ResultDTO<LoginResponseDTO>>(unauthorized.Value);
            Assert.Equal(401, unauthorized.StatusCode);
            Assert.NotNull(payload);
            Assert.Contains("Invalid credentials", payload.message);
        }

        [Fact(DisplayName = "login with valid credentials user returns ok with token")]
        public async Task loginWithValidCredentialsReturnsOkWithToken()
        {
            VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();

            await createUser(db);

            AuthController controller = new AuthController(db, config);

            LoginDTO request = new LoginDTO
            {
                email = "johndoe@example.com",
                password = "CorrectPassword1!"
            };

            IActionResult result = await controller.login(request);
            ObjectResult okResult = Assert.IsType<ObjectResult>(result);
            ResultDTO<LoginResponseDTO> payload = Assert.IsType<ResultDTO<LoginResponseDTO>>(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
            Assert.NotNull(payload.data?.token);
            Assert.Equal("johndoe", payload.data?.username);
            Assert.Equal("USER", payload.data?.role); 
        }

        private static VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        [Fact]
        public async Task Missing_fields_returns_400()
        {
            using var db = CreateDb();
            var service = new RegisterService(db);
            var dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                // username missing
                email = "john@example.com",
                password = "P@ssw0rd1!",
                password_confirmation = "P@ssw0rd1!"
            };

            var result = await service.RegisterAsync(dto);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Tous les champs sont requis", result.Message);
        }

        [Fact]
        public async Task Password_mismatch_returns_400()
        {
            using var db = CreateDb();
            var service = new RegisterService(db);
            var dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                username = "john",
                email = "john@example.com",
                password = "P@ssw0rd1!",
                password_confirmation = "P@ssw0rd2!"
            };

            var result = await service.RegisterAsync(dto);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Les mots de passe ne correspondent pas", result.Message);
        }

        [Theory]
        [InlineData("weak")]
        [InlineData("alllowercase")]
        [InlineData("NoNumberPassword!")]
        [InlineData("NoSpecialChar1")]
        public async Task Weak_password_returns_400(string password)
        {
            using var db = CreateDb();
            var service = new RegisterService(db);
            var dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                username = "john",
                email = "john@example.com",
                password = password,
                password_confirmation = password
            };

            var result = await service.RegisterAsync(dto);
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Le mot de passe doit contenir au minimum 8 caractères, une majuscule, un chiffre et un caractère spécial", result.Message);
        }

        [Fact]
        public async Task Email_already_used_returns_409()
        {
            using var db = CreateDb();
            db.Users.Add(new UserModel
            {
                FirstName = "Existing",
                LastName = "User",
                Username = "existing",
                Email = "john@example.com",
                Password = "hash",
                Language = "fr",
                CurrencyQuantity = 0
            });
            await db.SaveChangesAsync();

            var service = new RegisterService(db);
            var dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                username = "john",
                email = "john@example.com",
                password = "P@ssw0rd1!",
                password_confirmation = "P@ssw0rd1!"
            };

            var result = await service.RegisterAsync(dto);
            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("Email déjà utilisé", result.Message);
        }

        [Fact]
        public async Task Username_already_used_returns_409()
        {
            using var db = CreateDb();
            db.Users.Add(new UserModel
            {
                FirstName = "Existing",
                LastName = "User",
                Username = "john",
                Email = "other@example.com",
                Password = "hash",
                Language = "fr",
                CurrencyQuantity = 0
            });
            await db.SaveChangesAsync();

            var service = new RegisterService(db);
            var dto = new RegisterDTO
            {
                first_name = "John",
                last_name = "Doe",
                username = "john",
                email = "john2@example.com",
                password = "P@ssw0rd1!",
                password_confirmation = "P@ssw0rd1!"
            };

            var result = await service.RegisterAsync(dto);
            Assert.False(result.Success);
            Assert.Equal(409, result.StatusCode);
            Assert.Contains("Nom d'utilisateur déjà pris", result.Message);
        }

        [Fact]
        public async Task Success_returns_201_and_persists_user()
        {
            using var db = CreateDb();
            var service = new RegisterService(db);
            var dto = new RegisterDTO
            {
                first_name = "Ada",
                last_name = "Lovelace",
                username = "ada",
                email = "ada@example.com",
                password = "P@ssw0rd1!",
                password_confirmation = "P@ssw0rd1!"
            };

            var result = await service.RegisterAsync(dto);
            Assert.True(result.Success);
            Assert.Equal(201, result.StatusCode);
            Assert.Contains("Utilisateur créé", result.Message);

            var saved = await db.Users.SingleOrDefaultAsync(u => u.Email == "ada@example.com");
            Assert.NotNull(saved);
            Assert.Equal("ada", saved!.Username);
        }
    }
}