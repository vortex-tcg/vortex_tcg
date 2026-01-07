using VortexTCG.Auth.DTOs;
using Xunit;

namespace VortexTCG.Tests.Auth.DTOs
{
    public class LoginDTOTests
    {
        [Fact]
        public void Defaults_AreEmpty()
        {
            LoginDTO dto = new LoginDTO();

            Assert.Equal(string.Empty, dto.email);
            Assert.Equal(string.Empty, dto.password);
        }

        [Fact]
        public void Properties_AreSettable()
        {
            LoginDTO dto = new LoginDTO
            {
                email = "user@example.com",
                password = "Password1!"
            };

            Assert.Equal("user@example.com", dto.email);
            Assert.Equal("Password1!", dto.password);
        }
    }
}
