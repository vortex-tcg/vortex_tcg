using System;
using System.Threading.Tasks;
using VortexTCG.Auth.Providers;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using VortexTCG.Common.Services;
using Xunit;

namespace VortexTCG.Tests.Auth.Providers
{
    public class UserProviderTests
    {
        [Fact]
        public async Task EmailAndUsernameExistsAsync_Work_AsExpected()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            UserProvider provider = new UserProvider(db);

            db.Users.Add(new UserModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                Username = "jane",
                Email = "jane@example.com",
                Password = "hash",
                Language = "fr",
                CurrencyQuantity = 0
            });
            await db.SaveChangesAsync();

            Assert.True(await provider.EmailExistsAsync("jane@example.com"));
            Assert.False(await provider.EmailExistsAsync("other@example.com"));
            Assert.True(await provider.UsernameExistsAsync("jane"));
            Assert.False(await provider.UsernameExistsAsync("other"));
        }

        [Fact]
        public async Task AddUserAndSaveChanges_Persists_User()
        {
            using VortexDbContext db = VortexDbCoontextFactory.getInMemoryDbContext();
            UserProvider provider = new UserProvider(db);

            UserModel user = new UserModel
            {
                Id = Guid.NewGuid(),
                FirstName = "Ada",
                LastName = "Lovelace",
                Username = "ada",
                Email = "ada@example.com",
                Password = "hash",
                Language = "fr",
                CurrencyQuantity = 0
            };

            provider.AddUser(user);
            int saved = await provider.SaveChangesAsync();

            Assert.True(saved > 0);
            Assert.Single(db.Users);
        }
    }
}
