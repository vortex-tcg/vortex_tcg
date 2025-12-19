namespace VortexTCG.Game.Object
{
    public class Graveyard
    {
        private readonly List<Card> _cards = new();

        public void AddCard(Card card)
        {
            if (card == null) return;
            _cards.Add(card);
        }

        public void AddCards(IEnumerable<Card> cards)
        {
            if (cards == null) return;
            foreach (var c in cards)
                if (c != null) _cards.Add(c);
        }

        public int GetCount() => _cards.Count;

        public IReadOnlyList<Card> GetCards() => _cards.AsReadOnly();

        public void Clear() => _cards.Clear();
    }
}