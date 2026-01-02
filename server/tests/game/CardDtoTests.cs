using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;
using Xunit;

namespace game.Tests
{
    public class CardDtoTests
    {
        [Fact]
        public void CardDTO_ShouldStoreAllProperties()
        {
            Guid id = Guid.NewGuid();
            CardDTO dto = new CardDTO
            {
                Id = id,
                Name = "Guardian",
                Hp = 7,
                Attack = 3,
                Cost = 2,
                Description = "Tank",
                CardType = CardType.GUARD,
                Class = new List<string> { "stone", "earth" }
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal("Guardian", dto.Name);
            Assert.Equal(7, dto.Hp);
            Assert.Equal(3, dto.Attack);
            Assert.Equal(2, dto.Cost);
            Assert.Equal("Tank", dto.Description);
            Assert.Equal(CardType.GUARD, dto.CardType);
            Assert.Equal(new List<string> { "stone", "earth" }, dto.Class);
        }

        [Fact]
        public void GameCardDto_ShouldStoreAllProperties()
        {
            Guid id = Guid.NewGuid();
            List<string> classes = new List<string> { "mage" };
            List<CardState> states = new List<CardState> { CardState.ENGAGE, CardState.ATTACK_ENGAGE };

            GameCardDto dto = new GameCardDto
            {
                Id = id,
                GameCardId = 12,
                Name = "Fire Mage",
                Hp = 9,
                Attack = 6,
                Cost = 4,
                Description = "Burn everything",
                CardType = CardType.SPELL,
                Class = classes,
                State = states
            };

            Assert.Equal(id, dto.Id);
            Assert.Equal(12, dto.GameCardId);
            Assert.Equal("Fire Mage", dto.Name);
            Assert.Equal(9, dto.Hp);
            Assert.Equal(6, dto.Attack);
            Assert.Equal(4, dto.Cost);
            Assert.Equal("Burn everything", dto.Description);
            Assert.Equal(CardType.SPELL, dto.CardType);
            Assert.Equal(classes, dto.Class);
            Assert.Equal(states, dto.State);
        }
    }
}