using VortexTCG.Faction.DTOs;
using Xunit;

namespace VortexTCG.Tests.Api.Faction.DTOs
{
    public class FactionDtoTests
    {
        [Fact]
        public void FactionDto_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            FactionDto dto = new FactionDto
            {
                Id = id,
                Label = "Alliance",
                Currency = "Gold",
                Condition = "Active"
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Alliance", dto.Label);
            Assert.Equal("Gold", dto.Currency);
            Assert.Equal("Active", dto.Condition);
        }

        [Fact]
        public void FactionDto_InitializedWithDefaults()
        {
            FactionDto dto = new FactionDto();

            Assert.Equal(Guid.Empty, dto.Id);
            Assert.Equal("", dto.Label);
            Assert.Equal("", dto.Currency);
            Assert.Equal("", dto.Condition);
        }

        [Fact]
        public void FactionCardDto_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            FactionCardDto dto = new FactionCardDto
            {
                Id = id,
                Name = "Warrior",
                Price = 100,
                Hp = 50,
                Attack = 30,
                Cost = 5,
                Description = "A strong warrior",
                Picture = "path/to/pic",
                Extension = "v1.0",
                CardType = "Unit"
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Warrior", dto.Name);
            Assert.Equal(100, dto.Price);
            Assert.Equal(50, dto.Hp);
            Assert.Equal(30, dto.Attack);
            Assert.Equal(5, dto.Cost);
            Assert.Equal("A strong warrior", dto.Description);
            Assert.Equal("path/to/pic", dto.Picture);
            Assert.Equal("v1.0", dto.Extension);
            Assert.Equal("Unit", dto.CardType);
        }

        [Fact]
        public void FactionCardDto_HpAndAttackCanBeNull()
        {
            FactionCardDto dto = new FactionCardDto
            {
                Id = Guid.NewGuid(),
                Name = "Spell",
                Price = 50,
                Hp = null,
                Attack = null,
                Cost = 3,
                Description = "Magic spell",
                Picture = "spell.jpg",
                Extension = "v1.0",
                CardType = "Spell"
            };

            Assert.Null(dto.Hp);
            Assert.Null(dto.Attack);
        }

        [Fact]
        public void FactionChampionDto_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            FactionChampionDto dto = new FactionChampionDto
            {
                Id = id,
                Name = "Dragon Lord",
                Description = "Mighty dragon",
                HP = 200,
                Picture = "dragon.jpg"
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Dragon Lord", dto.Name);
            Assert.Equal("Mighty dragon", dto.Description);
            Assert.Equal(200, dto.HP);
            Assert.Equal("dragon.jpg", dto.Picture);
        }

        [Fact]
        public void FactionWithCardsDto_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            List<FactionCardDto> cards = new List<FactionCardDto>
            {
                new FactionCardDto { Id = Guid.NewGuid(), Name = "Card1" },
                new FactionCardDto { Id = Guid.NewGuid(), Name = "Card2" }
            };

            FactionWithCardsDto dto = new FactionWithCardsDto
            {
                Id = id,
                Label = "Empire",
                Currency = "Silver",
                Condition = "Stable",
                Cards = cards
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Empire", dto.Label);
            Assert.Equal("Silver", dto.Currency);
            Assert.Equal("Stable", dto.Condition);
            Assert.Equal(2, dto.Cards.Count);
        }

        [Fact]
        public void FactionWithCardsDto_CardsDefaultToEmptyList()
        {
            FactionWithCardsDto dto = new FactionWithCardsDto();
            Assert.NotNull(dto.Cards);
            Assert.Empty(dto.Cards);
        }

        [Fact]
        public void FactionWithChampionDto_CanSetAndGetAllProperties()
        {
            Guid id = Guid.NewGuid();
            FactionChampionDto champion = new FactionChampionDto { Id = Guid.NewGuid(), Name = "Hero" };

            FactionWithChampionDto dto = new FactionWithChampionDto
            {
                Id = id,
                Label = "Kingdom",
                Currency = "Bronze",
                Condition = "Expanding",
                Champion = champion
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Kingdom", dto.Label);
            Assert.Equal("Bronze", dto.Currency);
            Assert.Equal("Expanding", dto.Condition);
            Assert.NotNull(dto.Champion);
            Assert.Equal("Hero", dto.Champion.Name);
        }
    }
}
