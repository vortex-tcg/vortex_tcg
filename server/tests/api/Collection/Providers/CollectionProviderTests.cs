using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.Collection.Providers;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using CollectionModel = VortexTCG.DataAccess.Models.Collection;
using UserModel = VortexTCG.DataAccess.Models.User;
using Xunit;

namespace VortexTCG.Tests.Api.Collection.Providers
{
    public class CollectionProviderTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static UserModel NewUser() => new UserModel
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Username = $"user_{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@test.com",
            Password = "hash",
            Language = "fr",
            Role = Role.USER,
            Status = UserStatus.DISCONNECTED
        };

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoCollections()
        {
            using VortexDbContext db = CreateDb();
            CollectionProvider provider = new CollectionProvider(db);

            List<CollectionModel> result = await provider.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAll_WhenCollectionsExist()
        {
            using VortexDbContext db = CreateDb();
            db.Collections.Add(new CollectionModel { Id = Guid.NewGuid() });
            db.Collections.Add(new CollectionModel { Id = Guid.NewGuid() });
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);

            List<CollectionModel> result = await provider.GetAllAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionProvider provider = new CollectionProvider(db);

            CollectionModel? result = await provider.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCollection_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);

            CollectionModel? result = await provider.GetByIdAsync(entity.Id);

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result!.Id);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNull_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionProvider provider = new CollectionProvider(db);

            CollectionModel? result = await provider.GetByUserIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsCollection_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = NewUser();
            db.Users.Add(user);
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid(), User = user };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);

            CollectionModel? result = await provider.GetByUserIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result!.Id);
        }

        [Fact]
        public async Task AddAsync_PersistsCollection_WhenUserExists()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = NewUser();
            db.Users.Add(user);
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);
            CollectionModel entity = new CollectionModel
            {
                Id = Guid.NewGuid(),
                User = new UserModel { Id = user.Id }
            };

            CollectionModel result = await provider.AddAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(1, db.Collections.Count());
        }

        [Fact]
        public async Task AddAsync_PersistsCollection_WhenUserNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionProvider provider = new CollectionProvider(db);
            CollectionModel entity = new CollectionModel
            {
                Id = Guid.NewGuid(),
                User = new UserModel { Id = Guid.NewGuid() }
            };

            CollectionModel result = await provider.AddAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(1, db.Collections.Count());
        }

        [Fact]
        public async Task AddAsync_PersistsCollection_WithoutUser()
        {
            using VortexDbContext db = CreateDb();
            CollectionProvider provider = new CollectionProvider(db);
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };

            CollectionModel result = await provider.AddAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(1, db.Collections.Count());
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionProvider provider = new CollectionProvider(db);
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };

            bool result = await provider.UpdateAsync(entity);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);

            bool result = await provider.UpdateAsync(entity);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenUserFoundInDb()
        {
            using VortexDbContext db = CreateDb();
            UserModel user = NewUser();
            db.Users.Add(user);
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);
            entity.User = new UserModel { Id = user.Id };

            bool result = await provider.UpdateAsync(entity);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTrue_WhenUserNotFoundInDb()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);
            entity.User = new UserModel { Id = Guid.NewGuid() };

            bool result = await provider.UpdateAsync(entity);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            using VortexDbContext db = CreateDb();
            CollectionProvider provider = new CollectionProvider(db);

            bool result = await provider.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_AndRemoves()
        {
            using VortexDbContext db = CreateDb();
            CollectionModel entity = new CollectionModel { Id = Guid.NewGuid() };
            db.Collections.Add(entity);
            await db.SaveChangesAsync();
            CollectionProvider provider = new CollectionProvider(db);

            bool result = await provider.DeleteAsync(entity.Id);

            Assert.True(result);
            Assert.Equal(0, db.Collections.Count());
        }
    }
}