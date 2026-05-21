using System;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class VortexDbContextTests
    {
        private static VortexDbContext CreateDb(IHttpContextAccessor? accessor = null)
        {
            DbContextOptions<VortexDbContext> options = new DbContextOptionsBuilder<VortexDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new VortexDbContext(options, accessor);
        }

        private static User BuildUser() => new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Username = "testuser",
            Password = "pwd",
            Email = "test@example.com",
            Language = "fr",
            Role = Role.USER,
            Status = UserStatus.DISCONNECTED
        };

        [Fact]
        public void SaveChanges_SetsCreatedAuditFields_OnAddedEntity()
        {
            using VortexDbContext db = CreateDb();
            User user = BuildUser();

            db.Users.Add(user);
            db.SaveChanges();

            EntityEntry<User> entry = db.Entry(user);
            Assert.NotNull(entry.Property("CreatedAt").CurrentValue);
            Assert.Equal("System", entry.Property("CreatedBy").CurrentValue);
        }

        [Fact]
        public void SaveChanges_SetsUpdatedAuditFields_OnModifiedEntity()
        {
            using VortexDbContext db = CreateDb();
            User user = BuildUser();

            db.Users.Add(user);
            db.SaveChanges();

            user.Username = "updated";
            db.SaveChanges();

            EntityEntry<User> entry = db.Entry(user);
            Assert.NotNull(entry.Property("UpdatedAt").CurrentValue);
            Assert.Equal("System", entry.Property("UpdatedBy").CurrentValue);
        }

        [Fact]
        public void SetAuditFields_UsesIdentityName_WhenHttpContextHasAuthenticatedUser()
        {
            Mock<IIdentity> identity = new Mock<IIdentity>();
            identity.Setup(i => i.Name).Returns("alice");

            Mock<ClaimsPrincipal> principal = new Mock<ClaimsPrincipal>();
            principal.Setup(p => p.Identity).Returns(identity.Object);

            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User).Returns(principal.Object);

            Mock<IHttpContextAccessor> accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext.Object);

            using VortexDbContext db = CreateDb(accessor.Object);
            User user = BuildUser();

            db.Users.Add(user);
            db.SaveChanges();

            Assert.Equal("alice", db.Entry(user).Property("CreatedBy").CurrentValue);
        }

        [Fact]
        public void SetAuditFields_FallsBackToSystem_WhenHttpContextIsNull()
        {
            Mock<IHttpContextAccessor> accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            using VortexDbContext db = CreateDb(accessor.Object);
            User user = BuildUser();

            db.Users.Add(user);
            db.SaveChanges();

            Assert.Equal("System", db.Entry(user).Property("CreatedBy").CurrentValue);
        }

        [Fact]
        public void SetAuditFields_FallsBackToSystem_WhenIdentityNameIsNull()
        {
            Mock<IIdentity> identity = new Mock<IIdentity>();
            identity.Setup(i => i.Name).Returns((string?)null);

            Mock<ClaimsPrincipal> principal = new Mock<ClaimsPrincipal>();
            principal.Setup(p => p.Identity).Returns(identity.Object);

            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User).Returns(principal.Object);

            Mock<IHttpContextAccessor> accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext.Object);

            using VortexDbContext db = CreateDb(accessor.Object);
            User user = BuildUser();

            db.Users.Add(user);
            db.SaveChanges();

            Assert.Equal("System", db.Entry(user).Property("CreatedBy").CurrentValue);
        }

        [Fact]
        public void SetAuditFields_UsesIdentityName_OnModifiedEntity()
        {
            Mock<IIdentity> identity = new Mock<IIdentity>();
            identity.Setup(i => i.Name).Returns("bob");

            Mock<ClaimsPrincipal> principal = new Mock<ClaimsPrincipal>();
            principal.Setup(p => p.Identity).Returns(identity.Object);

            Mock<HttpContext> httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.User).Returns(principal.Object);

            Mock<IHttpContextAccessor> accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(httpContext.Object);

            using VortexDbContext db = CreateDb(accessor.Object);
            User user = BuildUser();

            db.Users.Add(user);
            db.SaveChanges();

            user.Username = "updated";
            db.SaveChanges();

            Assert.Equal("bob", db.Entry(user).Property("UpdatedBy").CurrentValue);
        }
    }
}
