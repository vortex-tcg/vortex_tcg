using VortexTCG.Api.Deck.DTOs;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.Api.Deck.DTOs
{
    public class DeckCardDtoTests
    {
        [Fact]
        public void DeckCardDto_CanSetAndGetAllProperties()
        {
            Guid deckCardId = Guid.NewGuid();
            Guid collCardId = Guid.NewGuid();
            Guid cardId = Guid.NewGuid();

            DeckCardDto dto = new DeckCardDto
            {
                DeckCardId = deckCardId,
                Quantity = 2,
                CollectionCardId = collCardId,
                Rarity = Rarity.EPIC,
                CardId = cardId,
                Name = "Fire Golem",
                Hp = 5,
                Attack = 3,
                Cost = 4,
                Description = "A powerful golem",
                Picture = "golem.png",
                Extension = Extension.BASIC,
                CardType = CardType.GUARD,
                Price = 10,
                Classes = new List<string> { "warrior", "mage" }
            };

            Assert.Equal(deckCardId, dto.DeckCardId);
            Assert.Equal(2, dto.Quantity);
            Assert.Equal(collCardId, dto.CollectionCardId);
            Assert.Equal(Rarity.EPIC, dto.Rarity);
            Assert.Equal(cardId, dto.CardId);
            Assert.Equal("Fire Golem", dto.Name);
            Assert.Equal(5, dto.Hp);
            Assert.Equal(3, dto.Attack);
            Assert.Equal(4, dto.Cost);
            Assert.Equal("A powerful golem", dto.Description);
            Assert.Equal("golem.png", dto.Picture);
            Assert.Equal(Extension.BASIC, dto.Extension);
            Assert.Equal(CardType.GUARD, dto.CardType);
            Assert.Equal(10, dto.Price);
            Assert.Equal(2, dto.Classes.Count);
            Assert.Contains("warrior", dto.Classes);
        }

        [Fact]
        public void DeckCardDto_DefaultClasses_IsEmpty()
        {
            DeckCardDto dto = new DeckCardDto();

            Assert.NotNull(dto.Classes);
            Assert.Empty(dto.Classes);
        }
    }
}