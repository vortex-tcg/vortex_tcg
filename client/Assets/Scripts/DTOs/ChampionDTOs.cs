using System;
using System.Text.Json.Serialization;

namespace VortexTCG.Scripts.DTOs
{
  

    [Serializable]
    public class BattleChampionDto
    {
        [JsonPropertyName("hp")]
        public int Hp { get; set; }

        [JsonPropertyName("secondaryCurrency")]
        public int SecondaryCurrency { get; set; }

        [JsonPropertyName("gold")]
        public int Gold { get; set; }
    }
}