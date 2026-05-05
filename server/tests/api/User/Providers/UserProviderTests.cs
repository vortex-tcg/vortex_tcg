using VortexTCG.Api.User.Providers;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using Xunit;

namespace VortexTCG.Tests.Api.User.Providers
{
    public class UserProviderTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static UserModel NewUser(string username = "user", string email = "user@test.com") => new UserModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Username = username,
            Email = email,
            Password = "hash",
            Language = "en",
            Role = Role.USER,
            Status = UserStatus.DISCONNECTED
        };

        [Fact]
        public void Query_ReturnsQueryable()
        {
            using VortexDbContext db = CreateDb();
            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);

            var query = provider.Query();

            Assert.NotNull(query);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);

            UserModel? result = await provider.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = NewUser("alice", "alice@test.com");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);
            UserModel? result = await provider.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal("alice", result!.Username);
        }

        [Fact]
        public async Task GetByUsernameAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);

            UserModel? result = await provider.GetByUsernameAsync("ghost");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUsernameAsync_ReturnsUser_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = NewUser("bob", "bob@test.com");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);
            UserModel? result = await provider.GetByUsernameAsync("bob");

            Assert.NotNull(result);
            Assert.Equal("bob@test.com", result!.Email);
        }

        [Fact]
        public async Task AddAsync_PersistsUser()
        {
            using VortexDbContext db = CreateDb();
            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);
            UserModel user = NewUser("charlie", "charlie@test.com");

            await provider.AddAsync(user);

            Assert.Equal(1, db.Users.Count());
        }

        [Fact]
        public async Task UpdateAsync_ModifiesUser()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = NewUser("dana", "dana@test.com");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);
            user.FirstName = "Updated";
            await provider.UpdateAsync(user);

            UserModel? updated = await provider.GetByIdAsync(user.Id);
            Assert.Equal("Updated", updated!.FirstName);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUser()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = NewUser("eve", "eve@test.com");
            db.Users.Add(user);
            await db.SaveChangesAsync();

            VortexTCG.Api.User.Providers.UserProvider provider = new VortexTCG.Api.User.Providers.UserProvider(db);
            await provider.DeleteAsync(user);

            Assert.Equal(0, db.Users.Count());
        }
    }
}