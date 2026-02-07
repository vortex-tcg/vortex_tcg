namespace game.Infrastructure.Interface;

using Domaine.Match.ValueObject;
using DTO;

public interface IDeckApiClient
{
    Task<ApiDeckDataDto> GetDeckDataAsync(DeckId deckId, CancellationToken ct = default);
}
