namespace game.Infrastructure.DTO;
using System.Text.Json.Serialization;


public sealed class ApiDeckDataDto
{
    [JsonPropertyName("cards")]
    public List<ApiDeckCardDto> Cards { get; set; } = new();

    [JsonPropertyName("champion")]
    public ApiDeckChampionDto Champion { get; set; } = default!;
}
