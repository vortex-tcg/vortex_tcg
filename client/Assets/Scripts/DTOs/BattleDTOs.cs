using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class AttackResponseDto
    {
        [JsonPropertyName("attackCardsId")]
        public List<int> AttackCardsId { get; set; } = new();
        [JsonPropertyName("playerId")]
        public Guid PlayerId { get; set; }
        [JsonPropertyName("opponentId")]
        public Guid OpponentId { get; set; }
    }

    [Serializable]
    public class DefenseCardDataDto
    {
        [JsonPropertyName("cardId")]
        public int cardId { get; set; }

        [JsonPropertyName("opponentCardId")]
        public int opponentCardId { get; set; }
    }

    [Serializable]
    public class DefenseDataResponseDto
    {
        [JsonPropertyName("attackCardsId")]
        public List<int> AttackCardsId { get; set; } = new();

        [JsonPropertyName("defenseCards")]
        public List<DefenseCardDataDto> DefenseCards { get; set; } = new();
    }

    [Serializable]
    public class DefenseResponseDto
    {
        [JsonPropertyName("data")]
        public DefenseDataResponseDto data { get; set; } = new();

        [JsonPropertyName("playerId")]
        public Guid PlayerId { get; set; }

        [JsonPropertyName("opponentId")]
        public Guid OpponentId { get; set; }
    }

    // TODO BattleResponseDto 
}