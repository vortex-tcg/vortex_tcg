using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VortexTCG.Auth.Providers;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using Xunit;

namespace Tests.Auth.Providers
{
    public class LoginProviderTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        [Fact]
        public async Task GetFirstUserByEmail_ReturnsUser_WhenExists()
        {
            using VortexDbContext db = CreateDb();
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

            var result = await provider.getFirstUserByEmail("jane@example.com");

            Assert.NotNull(result);
            Assert.Equal("jane", result!.Username);
            Assert.Equal(Role.ADMIN, result.Role);
        }

        [Fact]
        public async Task GetFirstUserByEmail_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            LoginProvider provider = new LoginProvider(db);

            var result = await provider.getFirstUserByEmail("missing@example.com");

            Assert.Null(result);
        }
    }
}
