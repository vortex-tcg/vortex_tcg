using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using VortexTCG.Common.Services;
using VortexTCG.Common.DTO;
using Scrypt;
using Xunit;

namespace Tests
{
    public class LoginServiceTests
    {
        private static async Task SeedUserAsync(VortexDbContext db, string email, string password, Role role)
        {
            ScryptEncoder encoder = new ScryptEncoder();
            string hash = encoder.Encode(password);

            db.Users.Add(new UserModel
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Username = "john",
                Email = email,
                Password = hash,
                Role = role,
                Language = "fr",
                CurrencyQuantity = 0
            });
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task Login_Returns400_WhenInputMissing()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            LoginService service = new LoginService(db, config);

            ResultDTO<LoginResponseDTO> result = await service.login(new LoginDTO { email = "", password = "" });

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Login_Returns401_WhenUserNotFound()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            LoginService service = new LoginService(db, config);

            ResultDTO<LoginResponseDTO> result = await service.login(new LoginDTO { email = "missing@example.com", password = "P@ssw0rd1!" });

            Assert.False(result.success);
            Assert.Equal(401, result.statusCode);
        }

        [Fact]
        public async Task Login_Returns401_WhenPasswordIncorrect()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            await SeedUserAsync(db, "user@example.com", "CorrectPassword1!", Role.USER);
            LoginService service = new LoginService(db, config);

            ResultDTO<LoginResponseDTO> result = await service.login(new LoginDTO { email = "user@example.com", password = "WrongPass1!" });

            Assert.False(result.success);
            Assert.Equal(401, result.statusCode);
        }

        [Fact]
        public async Task Login_Returns200_WithToken_WhenCredentialsValid()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();
            await SeedUserAsync(db, "admin@example.com", "CorrectPassword1!", Role.ADMIN);
            LoginService service = new LoginService(db, config);

            ResultDTO<LoginResponseDTO> result = await service.login(new LoginDTO { email = "admin@example.com", password = "CorrectPassword1!" });

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.False(string.IsNullOrWhiteSpace(result.data!.token));
            Assert.Equal("ADMIN", result.data.role);
        }

        [Fact]
        public async Task Login_Throws_WhenSecretKeyMissing()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            await SeedUserAsync(db, "user2@example.com", "CorrectPassword1!", Role.USER);
            LoginService service = new LoginService(db, config);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.login(new LoginDTO
            {
                email = "user2@example.com",
                password = "CorrectPassword1!"
            }));
        }
    }
}
