using System;
using VortexTCG.Auth.DTOs;
using Xunit;

namespace VortexTCG.Tests.Auth.DTOs
{
    public class LoginResponseDTOTests
    {
        [Fact]
        public void Defaults_AreInitialized()
        {
            LoginResponseDTO dto = new LoginResponseDTO();

            Assert.Equal(Guid.Empty, dto.id);
            Assert.Equal(string.Empty, dto.username);
            Assert.Equal(string.Empty, dto.token);
            Assert.Equal("USER", dto.role);
        }

        [Fact]
        public void Properties_AreSettable()
        {
            Guid expectedId = Guid.NewGuid();
            LoginResponseDTO dto = new LoginResponseDTO
            {
                id = expectedId,
                username = "john",
                token = "token-value",
                role = "ADMIN"
            };

            Assert.Equal(expectedId, dto.id);
            Assert.Equal("john", dto.username);
            Assert.Equal("token-value", dto.token);
            Assert.Equal("ADMIN", dto.role);
        }
    }
}
