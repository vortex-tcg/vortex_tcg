using System;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;
using GameCard = VortexTCG.Game.Object.Card;
using Xunit;

namespace game.Tests
{
    public class HandObjectTests
    {
        private GameCard CreateCard(int gameCardId)
        {
            CardDTO dto = new CardDTO
            {
                Id = Guid.NewGuid(),
                Name = "Unit",
                Hp = 5,
                Attack = 2,
                Cost = 1,
                Description = "",
                CardType = CardType.GUARD,
                Class = new System.Collections.Generic.List<string> { "class" }
            };
            return new GameCard(dto, gameCardId);
        }

        [Fact]
        public void AddCard_ShouldLimitToSeven()
        {
            Hand hand = new Hand();
            bool success = true;
            for (int i = 0; i < 7; i++)
            {
                success = hand.AddCard(CreateCard(i));
                Assert.True(success);
            }

            success = hand.AddCard(CreateCard(7));
            Assert.False(success);
            Assert.Equal(7, hand.GetCount());
        }

        [Fact]
        public void TryGetCard_ShouldReturnCardById()
        {
            Hand hand = new Hand();
            GameCard card = CreateCard(10);
            hand.AddCard(card);

            GameCard found;
            bool exists = hand.TryGetCard(10, out found);

            Assert.True(exists);
            Assert.Equal(card.GetGameCardId(), found.GetGameCardId());
        }

        [Fact]
        public void DeleteFromId_ShouldRemoveCard()
        {
            Hand hand = new Hand();
            hand.AddCard(CreateCard(20));
            hand.AddCard(CreateCard(21));

            hand.DeleteFromId(20);

            Assert.Equal(1, hand.GetCount());
            GameCard found;
            bool exists = hand.TryGetCard(20, out found);
            Assert.False(exists);
        }
    }
}
