namespace game.Infrastructure.DTO;
using System.Text.Json.Serialization;

public sealed class ApiDeckCardDto
{
    [JsonPropertyName("deckCardId")]
    public Guid DeckCardId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("collectionCardId")]
    public Guid CollectionCardId { get; set; }

    [JsonPropertyName("rarity")]
    public int Rarity { get; set; }

    [JsonPropertyName("cardId")]
    public Guid CardId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("hp")]
    public int? Hp { get; set; }

    [JsonPropertyName("attack")]
    public int? Attack { get; set; }

    [JsonPropertyName("cost")]
    public int Cost { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("picture")]
    public string Picture { get; set; } = default!;

    [JsonPropertyName("extension")]
    public int Extension { get; set; }

    [JsonPropertyName("cardType")]
    public int CardType { get; set; }

    [JsonPropertyName("price")]
    public int Price { get; set; }

    [JsonPropertyName("classes")]
    public List<string> Classes { get; set; } = new();
}
