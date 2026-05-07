using VortexTCG.Auth.DTOs;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.Auth.DTOs
{
    public class AuthDtosTests
    {
        [Fact]
        public void UserLoginDTO_CanSetAndGetProperties()
        {
            UserLoginDTO dto = new UserLoginDTO
            {
                Email = "user@example.com",
                Password = "MyPassword1!"
            };

            Assert.Equal("user@example.com", dto.Email);
            Assert.Equal("MyPassword1!", dto.Password);
        }

        [Fact]
        public void UserLoginDTO_DefaultValues_AreEmpty()
        {
            UserLoginDTO dto = new UserLoginDTO();

            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.Password);
        }

        [Fact]
        public void UserRegisterDTO_CanSetAndGetAllProperties()
        {
            UserRegisterDTO dto = new UserRegisterDTO
            {
                FirstName = "Jean",
                LastName = "Dupont",
                Username = "jdupont",
                Email = "jean@example.com",
                Password = "SecurePass1!",
                PasswordConfirmation = "SecurePass1!"
            };

            Assert.Equal("Jean", dto.FirstName);
            Assert.Equal("Dupont", dto.LastName);
            Assert.Equal("jdupont", dto.Username);
            Assert.Equal("jean@example.com", dto.Email);
            Assert.Equal("SecurePass1!", dto.Password);
            Assert.Equal("SecurePass1!", dto.PasswordConfirmation);
        }

        [Fact]
        public void UserRegisterDTO_DefaultValues_AreEmpty()
        {
            UserRegisterDTO dto = new UserRegisterDTO();

            Assert.Equal(string.Empty, dto.FirstName);
            Assert.Equal(string.Empty, dto.LastName);
            Assert.Equal(string.Empty, dto.Username);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.Password);
            Assert.Equal(string.Empty, dto.PasswordConfirmation);
        }

        [Fact]
        public void UserResponseDTO_CanSetAndGetAllProperties()
        {
            UserResponseDTO dto = new UserResponseDTO
            {
                Id = 42,
                Username = "alice",
                Email = "alice@example.com",
                FirstName = "Alice",
                LastName = "Smith",
                Token = "jwt-token-here",
                Role = "ADMIN",
                Language = "en",
                CurrencyQuantity = 250
            };

            Assert.Equal(42, dto.Id);
            Assert.Equal("alice", dto.Username);
            Assert.Equal("alice@example.com", dto.Email);
            Assert.Equal("Alice", dto.FirstName);
            Assert.Equal("Smith", dto.LastName);
            Assert.Equal("jwt-token-here", dto.Token);
            Assert.Equal("ADMIN", dto.Role);
            Assert.Equal("en", dto.Language);
            Assert.Equal(250, dto.CurrencyQuantity);
        }

        [Fact]
        public void UserResponseDTO_DefaultValues_AreCorrect()
        {
            UserResponseDTO dto = new UserResponseDTO();

            Assert.Equal(0, dto.Id);
            Assert.Equal(string.Empty, dto.Username);
            Assert.Equal(string.Empty, dto.Email);
            Assert.Equal(string.Empty, dto.FirstName);
            Assert.Equal(string.Empty, dto.LastName);
            Assert.Equal(string.Empty, dto.Token);
            Assert.Equal(string.Empty, dto.Role);
            Assert.Equal("fr", dto.Language);
            Assert.Equal(0, dto.CurrencyQuantity);
        }

        [Fact]
        public void LoginUserDTO_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();

            LoginUserDTO dto = new LoginUserDTO
            {
                Id = id,
                Username = "bob",
                Password = "hashedpw",
                Role = Role.ADMIN
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("bob", dto.Username);
            Assert.Equal("hashedpw", dto.Password);
            Assert.Equal(Role.ADMIN, dto.Role);
        }

        [Fact]
        public void LoginUserDTO_DefaultRole_IsUser()
        {
            LoginUserDTO dto = new LoginUserDTO();

            Assert.Equal(Role.USER, dto.Role);
        }
    }
}
