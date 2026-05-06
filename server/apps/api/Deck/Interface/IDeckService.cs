using VortexTCG.Api.Deck.DTOs;
using VortexTCG.Common.DTO;

namespace VortexTCG.Api.Deck.Interface
{
    public interface IDeckService
    {
        Task<ResultDTO<DeckDataDto>> GetDeckDataAsync(Guid deckId);
        Task<ResultDTO<List<DeckDTO>>> GetDecksByUserIdAsync(Guid userId);
        Task<ResultDTO<bool>> UpdateDeckAsync(Guid deckId, UpdateDeckDto dto);
        Task<ResultDTO<DeckResponseDto>> CreateAsync(CreateDeckDto dto);
        Task<ResultDTO<bool>> DeleteAsync(Guid deckId);
    }
}