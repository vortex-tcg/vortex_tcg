using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Providers;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Deck.Services
{
    public class DeckService
    {
        private readonly DeckProvider _deckProvider;

        public DeckService()
        {
            _deckProvider = new DeckProvider();
        }

        public ResultDTO<DeckDTO> GetDeckById(string deckId)
        => new ResultDTO<DeckDTO> {
            success = true,
            statusCode = 200,
            data = _deckProvider.GetMockDeck(deckId)
        };
    }
}
