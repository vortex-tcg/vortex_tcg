using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VortexTCG.Auth.Controllers;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.Common.Services;
using VortexTCG.Auth.DTOs;
using Scrypt;
using VortexTCG.Common.DTO;

namespace Tests
{
    public class AuthControllerTest
    {
        private async Task createUser(VortexDbContext db)
        {
            ScryptEncoder encoder = new ScryptEncoder();
            string hashedPassword = encoder.Encode("CorrectPassword1!");

            Rank rank = db.Ranks.Add(new Rank { Label = "Bronze", nbVictory = 0 }).Entity;
            db.Users.Add(new User
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "johndoe@example.com",
                Password = hashedPassword,
                Language = "fr",
                CurrencyQuantity = 0,
                Role = Role.USER,
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
    }
}