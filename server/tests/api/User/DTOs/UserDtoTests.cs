using VortexTCG.Api.User.DTOs;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.Api.User.DTOs
{
    public class UserDtoTests
    {
        [Fact]
        public void UserDTO_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            Guid rankId = Guid.NewGuid();

            UserDTO dto = new UserDTO
            {
                Id = id,
                FirstName = "Marie",
                LastName = "Curie",
                Username = "mcurie",
                Email = "marie@example.com",
                CurrencyQuantity = 500,
                Language = "fr",
                Role = Role.ADMIN,
                Status = UserStatus.CONNECTED,
                RankId = rankId
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Marie", dto.FirstName);
            Assert.Equal("Curie", dto.LastName);
            Assert.Equal("mcurie", dto.Username);
            Assert.Equal("marie@example.com", dto.Email);
            Assert.Equal(500, dto.CurrencyQuantity);
            Assert.Equal("fr", dto.Language);
            Assert.Equal(Role.ADMIN, dto.Role);
            Assert.Equal(UserStatus.CONNECTED, dto.Status);
            Assert.Equal(rankId, dto.RankId);
        }

        [Fact]
        public void UserDTO_RankId_CanBeNull()
        {
            UserDTO dto = new UserDTO { RankId = null };

            Assert.Null(dto.RankId);
        }

        [Fact]
        public void UserCreateDTO_CanSetAndGetAllProperties()
        {
            Guid rankId = Guid.NewGuid();

            UserCreateDTO dto = new UserCreateDTO
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Username = "ada",
                Password = "pass123",
                Email = "ada@example.com",
                CurrencyQuantity = 100,
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.DISCONNECTED,
                RankId = rankId
            };

            Assert.Equal("Ada", dto.FirstName);
            Assert.Equal("Lovelace", dto.LastName);
            Assert.Equal("ada", dto.Username);
            Assert.Equal("pass123", dto.Password);
            Assert.Equal("ada@example.com", dto.Email);
            Assert.Equal(100, dto.CurrencyQuantity);
            Assert.Equal("en", dto.Language);
            Assert.Equal(Role.USER, dto.Role);
            Assert.Equal(UserStatus.DISCONNECTED, dto.Status);
            Assert.Equal(rankId, dto.RankId);
        }
    }
}