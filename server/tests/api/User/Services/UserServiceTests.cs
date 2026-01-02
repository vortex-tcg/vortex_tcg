using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.User.DTOs;
using VortexTCG.Api.User.Providers;
using VortexTCG.Api.User.Services;
using VortexTCG.Common.DTO;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using Xunit;

namespace Tests.User.Services
{
    public class UserServiceTests
    {
        private static VortexDbContext CreateDb()
        {
            var options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        private static UserService CreateService(VortexDbContext db)
        {
            var provider = new UserProvider(db);
            return new UserService(provider);
        }

        [Fact]
        public async Task Create_Returns409_WhenUsernameExists()
        {
            using var db = CreateDb();
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "taken", Password = "x", Email = "a@b.c", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED });
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new UserCreateDTO { FirstName = "f", LastName = "l", Username = "taken", Password = "pwd", Email = "a@b.c", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };

            var result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new UserCreateDTO { FirstName = "f", LastName = "l", Username = "john", Password = "pwd", Email = "j@e.com", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };

            var result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("john", result.data!.Username);
        }

        [Fact]
        public async Task GetAll_ReturnsItems()
        {
            using var db = CreateDb();
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "a", Password = "x", Email = "a@a", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED });
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "f2", LastName = "l2", Username = "b", Password = "y", Email = "b@b", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED });
            await db.SaveChangesAsync();
            var service = CreateService(db);

            ResultDTO<UserDTO[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Length);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);
            var input = new UserCreateDTO { FirstName = "f", LastName = "l", Username = "x", Password = "y", Email = "z", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };

            var result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using var db = CreateDb();
            var entity = new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "old", Password = "p", Email = "o@o", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };
            db.Users.Add(entity);
            await db.SaveChangesAsync();
            var service = CreateService(db);
            var input = new UserCreateDTO { FirstName = "fn", LastName = "ln", Username = "new", Password = "np", Email = "n@n", Language = "fr", Role = Role.USER, Status = UserStatus.CONNECTED };

            var result = await service.UpdateAsync(entity.Id, input);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("new", result.data!.Username);
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            using var db = CreateDb();
            var service = CreateService(db);

            var result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using var db = CreateDb();
            var entity = new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "old", Password = "p", Email = "o@o", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };
            db.Users.Add(entity);
            await db.SaveChangesAsync();
            var service = CreateService(db);

            var result = await service.DeleteAsync(entity.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }
    }
}
