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

        public int GetCount() => _cards.Count;

        public Card? DrawCard()
        {
            if (_cards.Count == 0) return null;
            Card card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }
    }
}