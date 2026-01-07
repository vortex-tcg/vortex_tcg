using System;
using VortexTCG.Game.Object;
using Xunit;

namespace game.Tests
{
    public class DeckObjectTests
    {
        [Fact]
        public void InitDeck_ShouldCreateDeckWithThirtyCards()
        {
            Deck deck = new Deck();
            deck.initDeck(Guid.NewGuid());

            Assert.Equal(30, deck.GetCount());
        }

        [Fact]
        public void DrawCard_ShouldReturnCardsThenNull()
        {
            Deck deck = new Deck();
            deck.initDeck(Guid.NewGuid());

            int initialCount = deck.GetCount();
            Card? first = deck.DrawCard();
            Assert.NotNull(first);
            Assert.Equal(initialCount - 1, deck.GetCount());

            for (int i = 0; i < 29; i++)
            {
                deck.DrawCard();
            }

            Assert.Equal(0, deck.GetCount());
            Card? none = deck.DrawCard();
            Assert.Null(none);
        }
    }
}
