using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using VortexTCG.Common.Services;
using Xunit;

namespace VortexTCG.Tests.Shared.Common.Services
{
    public class CurrentUserServiceTests
    {
        [Fact]
        public void GetCurrentUsername_ReturnsName_WhenIdentityHasName()
        {
            Mock<IHttpContextAccessor> accessorMock = new Mock<IHttpContextAccessor>();
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "alice") }, "test"));
            accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            CurrentUserService service = new CurrentUserService(accessorMock.Object);

            string result = service.GetCurrentUsername();

            Assert.Equal("alice", result);
        }

        [Fact]
        public void GetCurrentUsername_ReturnsSystem_WhenHttpContextIsNull()
        {
            Mock<IHttpContextAccessor> accessorMock = new Mock<IHttpContextAccessor>();
            accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            CurrentUserService service = new CurrentUserService(accessorMock.Object);

            string result = service.GetCurrentUsername();

            Assert.Equal("System", result);
        }

        [Fact]
        public void GetCurrentUsername_ReturnsSystem_WhenIdentityNameIsNull()
        {
            Mock<IHttpContextAccessor> accessorMock = new Mock<IHttpContextAccessor>();
            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            accessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            CurrentUserService service = new CurrentUserService(accessorMock.Object);

            string result = service.GetCurrentUsername();

            Assert.Equal("System", result);
        }
    }
}