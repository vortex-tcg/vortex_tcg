namespace game.Infrastructure.Manager;

using System.Net.Http.Json;
using Domaine.Match.ValueObject;
using DTO;
using Interface;


public sealed class DeckApiClientManager : IDeckApiClient
{
    private readonly HttpClient _http;

    public DeckApiClientManager(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiDeckDataDto> GetDeckDataAsync(DeckId deckId, CancellationToken ct = default)
    {
        string path = $"/api/deck/getDeckData/{(Guid)deckId}";
        ApiResultDto<ApiDeckDataDto>? envelope =
            await _http.GetFromJsonAsync<ApiResultDto<ApiDeckDataDto>>(path, ct);

        if (envelope is null)
            throw new InvalidOperationException("Deck API returned null response.");

        if (!envelope.success || envelope.data is null)
            throw new InvalidOperationException(
                $"Deck API error (statusCode={envelope.statusCode}): {envelope.message ?? "Unknown error"}");

        return envelope.data;
    }
}
