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
    [Serializable]
    public class BattleAgainstCardDataDto
    {
        [JsonPropertyName("isAttackerDead")] public bool isAttackerDead { get; set; }

        [JsonPropertyName("isDefenderDead")] public bool isDefenderDead { get; set; }

        [JsonPropertyName("attackerDamageDeal")]
        public int attackerDamageDeal { get; set; }

        [JsonPropertyName("defenderDamageDeal")]
        public int defenderDamageDeal { get; set; }

        [JsonPropertyName("attackerCard")] public GameCardDto attackerCard { get; set; }

        [JsonPropertyName("defenderCard")] public GameCardDto defenderCard { get; set; }

        [JsonPropertyName("attackerChamp")] public BattleChampionDto attackerChamp { get; set; }

        [JsonPropertyName("defenderChamp")] public BattleChampionDto defenderChamp { get; set; }
    }

    [Serializable]
    public class BattlaAgainstChampDataDto
    {
        [JsonPropertyName("isChampDead")] public bool isChampDead { get; set; }

        [JsonPropertyName("isCardDead")] public bool isCardDead { get; set; }

        [JsonPropertyName("attackerDamageDeal")]
        public int attackerDamageDeal { get; set; }

        [JsonPropertyName("championDamageDeal")]
        public int championDamageDeal { get; set; }

        [JsonPropertyName("attackerCard")] public GameCardDto attackerCard { get; set; }

        [JsonPropertyName("attackerChamp")] public BattleChampionDto attackerChamp { get; set; }

        [JsonPropertyName("defenderChamp")] public BattleChampionDto defenderChamp { get; set; }
    }

    [Serializable]
    public class BattleDataDto
    {
        [JsonPropertyName("isAgainstChamp")] public bool isAgainstChamp { get; set; }

        [JsonPropertyName("againstCard")] public BattleAgainstCardDataDto againstCard { get; set; }

        [JsonPropertyName("againstChamp")] public BattlaAgainstChampDataDto againstChamp { get; set; }
    }

    [Serializable]
    public class BattlesDataDto
    {
        [JsonPropertyName("battles")] public List<BattleDataDto> battles { get; set; } = new();
    } 
    
}