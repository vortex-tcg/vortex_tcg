using Microsoft.Extensions.Configuration;
using VortexTCG.Common.Services;
using Xunit;

namespace VortexTCG.Tests.Shared.Common.Services
{
    public class TestConfigurationServiceTests
    {
        [Fact]
        public void GetTestConfiguration_ReturnsNonNull()
        {
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();

            Assert.NotNull(config);
        }

        [Fact]
        public void GetTestConfiguration_ContainsJwtSecretKey()
        {
            IConfiguration config = TestConfigurationBuilder.getTestConfiguration();

            string? secretKey = config["JwtSettings:SecretKey"];

            Assert.NotNull(secretKey);
            Assert.NotEmpty(secretKey!);
        }
    }
}