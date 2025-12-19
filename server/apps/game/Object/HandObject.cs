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
    }
}