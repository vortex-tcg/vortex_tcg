namespace VortexTCG.Game.Object
{
    public class Hand
    {
        private List<Card> _cards = new List<Card>();
        public bool AddCard(Card card)
        {
            if (_cards.Count >= 7) return false;

            _cards.Add(card);
            return true;
        }   
        public int GetCount() => _cards.Count;

        public bool TryGetCard(int cardId, out Card playedCard) {
            foreach(Card card in _cards.Where(c => c.GetGameCardId() == cardId)) {
                playedCard = card;
                return true;
            }
            playedCard = null;
            return false;
        }

        public void DeleteFromId(int cardId) {
            Card cardToDelete = null;
            foreach(Card card in _cards.Where(card => card.GetGameCardId() == cardId)) {
            cardToDelete = card;
                break;
            }
            _cards.Remove(cardToDelete);
        }
    }
}