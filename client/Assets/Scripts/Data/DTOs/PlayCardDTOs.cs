using System;
using System.Text.Json.Serialization;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class PlayCardResponseDto
    {
        public PlayCardPlayerResultDto PlayerResult { get; set; }
        public PlayCardOpponentResultDto OpponentResult { get; set; }
    }

    [Serializable]
    public class PlayCardPlayerResultDto
    {
        public Guid PlayerId { get; set; }
        public GameCardDto PlayedCard { get; set; }
        public PlayCardChampionDto Champion { get; set; }
        public int location { get; set; }
        public bool canPlayed { get; set; }
    }

    [Serializable]
    public class PlayCardOpponentResultDto
    {
        public Guid PlayerId { get; set; }
        public GameCardDto PlayedCard { get; set; }
        public PlayCardChampionDto Champion { get; set; }
        public int location { get; set; }
    }

    [Serializable]
    public class PlayCardChampionDto
    {
        public Guid Id { get; set; }
        public int Hp { get; set; }
        public int Gold { get; set; }
        public int SecondaryCurrency { get; set; }
    }

    [Serializable]
    public class GameCardDto
    {
        [JsonPropertyName("gameCardId")]
        public int GameCardId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("hp")]
        public int Hp { get; set; }

        [JsonPropertyName("attack")]
        public int Attack { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("cardType")]
        public CardType CardType { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = "";
    }
}