using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexTCG.DataAccess.Seeds;
using Xunit;

namespace VortexTCG.Tests.DataAccess.Seeds
{
    public class UsersInitializerTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        [Fact]
        public void Seed_AddsTwoUsers_WhenDatabaseEmpty()
        {
            using VortexDbContext db = CreateDb();
            UsersInitializer seeder = new UsersInitializer(db);

            seeder.Seed();

            System.Collections.Generic.List<User> users = db.Users.ToList();
            Assert.Equal(2, users.Count);

            User john = users.Single(u => u.Username == "Superman");
            Assert.Equal(Role.USER, john.Role);
            Assert.NotEqual("Password123", john.Password); // password must be hashed

            User jane = users.Single(u => u.Username == "Batman");
            Assert.Equal("jane.doe@email.com", jane.Email);
            Assert.NotEqual("Password456", jane.Password);
        }

        [Fact]
        public void Seed_DoesNothing_WhenUsersExist()
        {
            using VortexDbContext db = CreateDb();
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Existing",
                LastName = "User",
                Username = "ExistingUser",
                Password = "pwd",
                Email = "existing@example.com",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.CONNECTED,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedBy = "test"
            });
            db.SaveChanges();

            UsersInitializer seeder = new UsersInitializer(db);
            seeder.Seed();

            Assert.Equal(1, db.Users.Count());
        }
    }
}
