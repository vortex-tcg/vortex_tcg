using Microsoft.EntityFrameworkCore;
using VortexTCG.Api.User.DTOs;
using VortexTCG.Api.User.Providers;
using VortexTCG.Api.User.Services;
using VortexTCG.Common.DTO;
using VortexTCG.Common.Services;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using UserModel = VortexTCG.DataAccess.Models.User;
using Xunit;

namespace VortexTCG.Tests.Api.User.Services
{
    public class UserServiceTests
    {
        private static VortexDbContext CreateDb() => VortexDbCoontextFactory.getInMemoryDbContext();

        private static UserService CreateService(VortexDbContext db)
        {
            UserProvider provider = new UserProvider(db);
            return new UserService(provider);
        }

        [Fact]
        public async Task Create_Returns409_WhenUsernameExists()
        {
            using VortexDbContext db = CreateDb();
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "taken", Password = "x", Email = "a@b.c", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED });
            await db.SaveChangesAsync();
            UserService service = CreateService(db);
            UserCreateDTO input = new UserCreateDTO { FirstName = "f", LastName = "l", Username = "taken", Password = "pwd", Email = "a@b.c", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };

            ResultDTO<UserDTO> result = await service.CreateAsync(input);

            Assert.False(result.success);
            Assert.Equal(409, result.statusCode);
        }

        [Fact]
        public async Task Create_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            UserService service = CreateService(db);
            UserCreateDTO input = new UserCreateDTO { FirstName = "f", LastName = "l", Username = "john", Password = "pwd", Email = "j@e.com", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };

            ResultDTO<UserDTO> result = await service.CreateAsync(input);

            Assert.True(result.success);
            Assert.Equal(201, result.statusCode);
            Assert.Equal("john", result.data!.Username);
        }

        [Fact]
        public async Task GetAll_ReturnsItems()
        {
            using VortexDbContext db = CreateDb();
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "a", Password = "x", Email = "a@a", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED });
            db.Users.Add(new UserModel { Id = Guid.NewGuid(), FirstName = "f2", LastName = "l2", Username = "b", Password = "y", Email = "b@b", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED });
            await db.SaveChangesAsync();
            UserService service = CreateService(db);

            ResultDTO<UserDTO[]> result = await service.GetAllAsync();

            Assert.True(result.success);
            Assert.Equal(2, result.data!.Length);
        }

        [Fact]
        public async Task Update_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            UserService service = CreateService(db);
            UserCreateDTO input = new UserCreateDTO { FirstName = "f", LastName = "l", Username = "x", Password = "y", Email = "z", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };

            ResultDTO<UserDTO> result = await service.UpdateAsync(Guid.NewGuid(), input);

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Update_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            UserModel entity = new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "old", Password = "p", Email = "o@o", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };
            db.Users.Add(entity);
            await db.SaveChangesAsync();
            UserService service = CreateService(db);
            UserCreateDTO input = new UserCreateDTO { FirstName = "fn", LastName = "ln", Username = "new", Password = "np", Email = "n@n", Language = "fr", Role = Role.USER, Status = UserStatus.CONNECTED };

            ResultDTO<UserDTO> result = await service.UpdateAsync(entity.Id, input);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
            Assert.Equal("new", result.data!.Username);
        }

        [Fact]
        public async Task Delete_Returns404_WhenMissing()
        {
            using VortexDbContext db = CreateDb();
            UserService service = CreateService(db);

            ResultDTO<object> result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result.success);
            Assert.Equal(404, result.statusCode);
        }

        [Fact]
        public async Task Delete_Succeeds()
        {
            using VortexDbContext db = CreateDb();
            UserModel entity = new UserModel { Id = Guid.NewGuid(), FirstName = "f", LastName = "l", Username = "old", Password = "p", Email = "o@o", Language = "en", Role = Role.USER, Status = UserStatus.DISCONNECTED };
            db.Users.Add(entity);
            await db.SaveChangesAsync();
            UserService service = CreateService(db);

            ResultDTO<object> result = await service.DeleteAsync(entity.Id);

            Assert.True(result.success);
            Assert.Equal(200, result.statusCode);
        }
    }
}
