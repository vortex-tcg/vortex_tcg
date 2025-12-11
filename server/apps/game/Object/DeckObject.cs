using VortexTCG.Game.Utils;

namespace VortexTCG.Game.Object
{
    public class Deck
    {
        private Guid _deck_id;

        private List<Card> _cards = new List<Card>();

        public void initDeck(Guid deck_id)
        {
            _deck_id = deck_id;
            _cards = DeckFactory.genDeck();

            return;
        }
    }
}