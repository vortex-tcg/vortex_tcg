
using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Deck.Interface

{
    public interface IDeckService
    {
        Task<ResultDTO<DeckDataDto>> GetDeckDataAsync(Guid deckId);
        ResultDTO<DeckDTO> GetDeckById(string deckId);
    }
}
