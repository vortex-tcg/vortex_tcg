using System.Collections.Generic;
using VortexTCG.Game.Object;
using VortexTCG.Game.Utils;
using Xunit;

namespace game.Tests
{
    public class DeckFactoryTests
    {
        [Fact]
        public void GenDeck_ShouldCreateThirtyCards()
        {
            List<Card> deck = DeckFactory.genDeck();

            Assert.Equal(30, deck.Count);
            Assert.All(deck, card => Assert.NotNull(card));
            Assert.Equal(0, deck[0].GetGameCardId());
            Assert.Equal(29, deck[29].GetGameCardId());
        }
    }
}
