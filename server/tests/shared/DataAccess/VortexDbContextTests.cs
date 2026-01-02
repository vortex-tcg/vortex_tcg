using System;
using Microsoft.EntityFrameworkCore;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class VortexDbContextTests
    {
        private static VortexDbContext CreateDb()
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options);
        }

        [Fact]
        public void SaveChanges_SetsCreatedAuditFields_OnAddedEntity()
        {
            using VortexDbContext db = CreateDb();

            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Doe",
                Username = "alice",
                Password = "pwd",
                Email = "alice@example.com",
                Language = "fr",
                Role = Role.USER,
                Status = UserStatus.DISCONNECTED
            };

            db.Users.Add(user);
            db.SaveChanges();

            var entry = db.Entry(user);
            Assert.NotNull(entry.Property("CreatedAt").CurrentValue);
            Assert.Equal("System", entry.Property("CreatedBy").CurrentValue);
        }

        [Fact]
        public void SaveChanges_SetsUpdatedAuditFields_OnModifiedEntity()
        {
            using VortexDbContext db = CreateDb();

            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Smith",
                Username = "bob",
                Password = "pwd",
                Email = "bob@example.com",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.DISCONNECTED
            };

            db.Users.Add(user);
            db.SaveChanges();

            user.Username = "newbob";
            db.SaveChanges();

            var entry = db.Entry(user);
            Assert.NotNull(entry.Property("UpdatedAt").CurrentValue);
            Assert.Equal("System", entry.Property("UpdatedBy").CurrentValue);
        }
    }
}
