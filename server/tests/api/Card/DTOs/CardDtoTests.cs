using VortexTCG.Api.Card.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Card.DTOs
{
    public class CardDtoTests
    {
        [Fact]
        public void CardDto_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            Guid factionId = Guid.NewGuid();

            CardDto dto = new CardDto
            {
                Id = id,
                Name = "Fire Dragon",
                Price = 10,
                Hp = 8,
                Attack = 6,
                Cost = 5,
                Description = "A powerful dragon",
                Picture = "dragon.png",
                Extension = "BASIC",
                CardType = "GUARD",
                Class = new List<string> { "warrior" },
                Factions = new List<Guid> { factionId }
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Fire Dragon", dto.Name);
            Assert.Equal(10, dto.Price);
            Assert.Equal(8, dto.Hp);
            Assert.Equal(6, dto.Attack);
            Assert.Equal(5, dto.Cost);
            Assert.Equal("A powerful dragon", dto.Description);
            Assert.Equal("dragon.png", dto.Picture);
            Assert.Equal("BASIC", dto.Extension);
            Assert.Equal("GUARD", dto.CardType);
            Assert.Single(dto.Class);
            Assert.Single(dto.Factions);
            Assert.Equal(factionId, dto.Factions[0]);
        }

        [Fact]
        public void CardDto_DefaultLists_AreEmpty()
        {
            CardDto dto = new CardDto();

            Assert.NotNull(dto.Class);
            Assert.Empty(dto.Class);
            Assert.NotNull(dto.Factions);
            Assert.Empty(dto.Factions);
        }

        [Fact]
        public void CardCreateDto_CanSetAndGetAllProperties()
        {
            CardCreateDto dto = new CardCreateDto
            {
                Name = "Iron Sword",
                Price = 3,
                Hp = 0,
                Attack = 4,
                Description = "A sharp blade",
                Picture = "sword.png"
            };

            Assert.Equal("Iron Sword", dto.Name);
            Assert.Equal(3, dto.Price);
            Assert.Equal(0, dto.Hp);
            Assert.Equal(4, dto.Attack);
            Assert.Equal("A sharp blade", dto.Description);
            Assert.Equal("sword.png", dto.Picture);
        }
    }
}