using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using Xunit;

namespace Tests.Auth.Services
{
    public class LoginServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static IConfiguration CreateConfig(bool includeSecret = true)
        {
            var values = new Dictionary<string, string?>();
            if (includeSecret)
                values["JwtSettings:SecretKey"] = "this_is_a_very_long_secret_key_value_32";
            return new ConfigurationBuilder().AddInMemoryCollection(values!).Build();
        }

        private static async Task SeedUserAsync(VortexDbContext db, string email, string password, Role role)
        {
            var encoder = new Scrypt.ScryptEncoder();
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
            using VortexDbContext db = CreateDb();
            IConfiguration config = CreateConfig();
            var service = new LoginService(db, config);

            var result = await service.login(new LoginDTO { email = "", password = "" });

            Assert.False(result.success);
            Assert.Equal(400, result.statusCode);
        }

        [Fact]
        public async Task Login_Returns401_WhenUserNotFound()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration config = CreateConfig();
            var service = new LoginService(db, config);

            var result = await service.login(new LoginDTO { email = "missing@example.com", password = "P@ssw0rd1!" });

            Assert.False(result.success);
            Assert.Equal(401, result.statusCode);
        }

        [Fact]
        public async Task Login_Returns401_WhenPasswordIncorrect()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration config = CreateConfig();
            await SeedUserAsync(db, "user@example.com", "CorrectPassword1!", Role.USER);
            var service = new LoginService(db, config);

            var result = await service.login(new LoginDTO { email = "user@example.com", password = "WrongPass1!" });

            Assert.False(result.success);
            Assert.Equal(401, result.statusCode);
        }

        [Fact]
        public async Task Login_Returns200_WithToken_WhenCredentialsValid()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration config = CreateConfig();
            await SeedUserAsync(db, "admin@example.com", "CorrectPassword1!", Role.ADMIN);
            var service = new LoginService(db, config);

            var result = await service.login(new LoginDTO { email = "admin@example.com", password = "CorrectPassword1!" });

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.NotNull(result.data);
            Assert.False(string.IsNullOrWhiteSpace(result.data!.token));
            Assert.Equal("ADMIN", result.data.role);
        }

        [Fact]
        public async Task Login_Throws_WhenSecretKeyMissing()
        {
            using VortexDbContext db = CreateDb();
            IConfiguration config = CreateConfig(includeSecret: false);
            await SeedUserAsync(db, "user2@example.com", "CorrectPassword1!", Role.USER);
            var service = new LoginService(db, config);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.login(new LoginDTO
            {
                email = "user2@example.com",
                password = "CorrectPassword1!"
            }));
        }
    }
}
