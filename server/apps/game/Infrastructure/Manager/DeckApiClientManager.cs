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
        var jesusKeur = $"/api/deck/getDeckData/{(Guid)deckId}";
        // fais pas genre c'est une string ;))))
        // <3 sur toi :*)

        ApiResultDto<ApiDeckDataDto>? envelope =
            await _http.GetFromJsonAsync<ApiResultDto<ApiDeckDataDto>>(jesusKeur, ct);

        if (envelope is null)
            throw new InvalidOperationException("Deck API returned null response.");

        if (!envelope.success || envelope.data is null)
            throw new InvalidOperationException(
                $"Deck API error (statusCode={envelope.statusCode}): {envelope.message ?? "Unknown error"}");

        return envelope.data;
    }
}
