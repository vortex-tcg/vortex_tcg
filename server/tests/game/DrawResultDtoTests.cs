using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using Xunit;

namespace game.Tests
{
    public class DrawResultDtoTests
    {
        [Fact]
        public void DrawnCardDto_ShouldHoldValues()
        {
            DrawnCardDTO dto = new DrawnCardDTO
            {
                GameCardId = 5,
                Name = "Fireball",
                Hp = 2,
                Attack = 6,
                Cost = 3,
                Description = "Deal damage",
                CardType = CardType.SPELL
            };

            Assert.Equal(5, dto.GameCardId);
            Assert.Equal("Fireball", dto.Name);
            Assert.Equal(2, dto.Hp);
            Assert.Equal(6, dto.Attack);
            Assert.Equal(3, dto.Cost);
            Assert.Equal("Deal damage", dto.Description);
            Assert.Equal(CardType.SPELL, dto.CardType);
        }

        [Fact]
        public void DrawResultForPlayerDTO_ShouldAggregateCards()
        {
            DrawnCardDTO card1 = new DrawnCardDTO { GameCardId = 1, Name = "A" };
            DrawnCardDTO card2 = new DrawnCardDTO { GameCardId = 2, Name = "B" };
            DrawnCardDTO burned = new DrawnCardDTO { GameCardId = 3, Name = "C" };

            DrawResultForPlayerDTO dto = new DrawResultForPlayerDTO
            {
                PlayerId = Guid.NewGuid(),
                DrawnCards = new List<DrawnCardDTO> { card1, card2 },
                SentToGraveyard = new List<DrawnCardDTO> { burned },
                FatigueCount = 1,
                BaseFatigue = 2
            };

            Assert.Equal(2, dto.DrawnCards.Count);
            Assert.Single(dto.SentToGraveyard);
            Assert.Equal(1, dto.FatigueCount);
            Assert.Equal(2, dto.BaseFatigue);
            Assert.NotEqual(Guid.Empty, dto.PlayerId);
        }

        [Fact]
        public void DrawResultForOpponentDTO_ShouldHoldValues()
        {
            DrawResultForOpponentDTO dto = new DrawResultForOpponentDTO
            {
                PlayerId = Guid.NewGuid(),
                CardsDrawnCount = 2,
                CardsBurnedCount = 1,
                FatigueCount = 3,
                BaseFatigue = 1
            };

            Assert.Equal(2, dto.CardsDrawnCount);
            Assert.Equal(1, dto.CardsBurnedCount);
            Assert.Equal(3, dto.FatigueCount);
            Assert.Equal(1, dto.BaseFatigue);
        }

        [Fact]
        public void DrawCardsResultDTO_ShouldInitializeDefaults()
        {
            DrawCardsResultDTO dto = new DrawCardsResultDTO();

            Assert.NotNull(dto.PlayerResult);
            Assert.NotNull(dto.OpponentResult);
        }
    }
}
