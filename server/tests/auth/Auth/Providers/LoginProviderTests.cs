using System;
using System.Threading.Tasks;
using VortexTCG.Auth.DTOs;
using VortexTCG.Auth.Providers;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using VortexTCG.Common.Services;
using Xunit;

namespace VortexTCG.Tests.Auth.Providers
{
    public class LoginProviderTests
    {
        [Fact]
        public async Task GetFirstUserByEmail_ReturnsUser_WhenExists()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            LoginProvider provider = new LoginProvider(db);

            db.Users.Add(new UserModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                Username = "jane",
                Email = "jane@example.com",
                Password = "hash",
                Role = Role.ADMIN,
                Language = "fr",
                CurrencyQuantity = 0
            });
            await db.SaveChangesAsync();

            LoginUserDTO? result = await provider.getFirstUserByEmail("jane@example.com");

            Assert.NotNull(result);
            Assert.Equal("jane", result!.Username);
            Assert.Equal(Role.ADMIN, result.Role);
        }

        [Fact]
        public async Task GetFirstUserByEmail_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            LoginProvider provider = new LoginProvider(db);

            LoginUserDTO? result = await provider.getFirstUserByEmail("missing@example.com");

            Assert.Null(result);
        }
    }
}
