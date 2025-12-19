using Deck.DTOs;
using Deck.Providers;

namespace Deck.Services
{
    public class DeckService
    {
        private readonly DeckProvider _deckProvider;

        public DeckService()
        {
            _deckProvider = new DeckProvider();
        }

        public DeckDTO GetDeckById(string deckId)
        {
            DeckDTO deck = _deckProvider.GetMockDeck(deckId);
            return deck;
        }
    }
}
