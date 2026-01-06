using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;
using GameCard = VortexTCG.Game.Object.Card;
using Xunit;

namespace game.Tests
{
    public class GraveyardObjectTests
    {
        private GameCard CreateCard(int id)
        {
            CardDTO dto = new CardDTO
            {
                Id = Guid.NewGuid(),
                Name = "Ghost",
                Hp = 1,
                Attack = 1,
                Cost = 0,
                Description = "",
                CardType = CardType.GUARD,
                Class = new List<string> { "class" }
            };
            return new GameCard(dto, id);
        }

        [Fact]
        public void AddCard_ShouldIgnoreNullAndStoreCards()
        {
            Graveyard graveyard = new Graveyard();
            graveyard.AddCard((GameCard)null!);
            graveyard.AddCard(CreateCard(1));

            Assert.Equal(1, graveyard.GetCount());
        }

        [Fact]
        public void AddCards_ShouldFilterNulls()
        {
            Graveyard graveyard = new Graveyard();
            List<GameCard> cards = new List<GameCard> { CreateCard(2), (GameCard)null!, CreateCard(3) };

            graveyard.AddCards(cards);

            Assert.Equal(2, graveyard.GetCount());
            Assert.Equal(2, graveyard.GetCards().Count);
        }

        [Fact]
        public void Clear_ShouldRemoveAllCards()
        {
            Graveyard graveyard = new Graveyard();
            graveyard.AddCard(CreateCard(4));
            graveyard.AddCard(CreateCard(5));

            graveyard.Clear();

            Assert.Equal(0, graveyard.GetCount());
            Assert.Empty(graveyard.GetCards());
        }
    }
}
