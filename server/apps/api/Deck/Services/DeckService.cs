using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Api.Deck.Interface;
using VortexTCG.Api.Deck.Providers;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Deck.Services
{
    public class DeckService : IDeckService
    {
        private readonly DeckProvider _deckProvider;

        public DeckService(DeckProvider deckProvider)
        {
            _deckProvider = deckProvider;
        }

        public ResultDTO<DeckDTO> GetDeckById(string deckId)
        => new ResultDTO<DeckDTO> {
            success = true,
            statusCode = 200,
            data = _deckProvider.GetMockDeck(deckId)
        };
        public async Task<ResultDTO<DeckDataDto>> GetDeckDataAsync(Guid deckId)
        {
            var data = await _deckProvider.GetDeckDataAsync(deckId);

            if (data == null)
            {
                return new ResultDTO<DeckDataDto>
                {
                    success = false,
                    statusCode = 404,
                    message = "Deck not found",
                    data = null
                };
            }

            return new ResultDTO<DeckDataDto>
            {
                success = true,
                statusCode = 200,
                data = data
            };
        }
    }
}
