namespace game.Infrastructure.DTO;

using System.Text.Json.Serialization;
public sealed class ApiDeckChampionDto
{
    [JsonPropertyName("championID")]
    public Guid ChampionID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("hp")]
    public int HP { get; set; }

    [JsonPropertyName("picture")]
    public string Picture { get; set; } = default!;

    [JsonPropertyName("factionId")]
    public Guid FactionId { get; set; }
}
