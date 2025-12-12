namespace VortexTCG.Game.Object
{
    public class Hand
    {
        private List<Card> _cards = new List<Card>();
        public void AddCard(Card card) => _cards.Add(card);
        public int GetCount() => _cards.Count;
    }
}