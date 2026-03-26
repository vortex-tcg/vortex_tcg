using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VortexTCG.Scripts.DTOs
{
    public class MatchInitUserDto
    {
        [JsonPropertyName("matchId")]
        public Guid MatchId { get; set; }

        [JsonPropertyName("self")]
        public MatchInitSideDto Self { get; set; } = new MatchInitSideDto();

        [JsonPropertyName("opponent")]
        public MatchInitSideDto Opponent { get; set; } = new MatchInitSideDto();

        [JsonPropertyName("opponentHandSize")]
        public int OpponentHandSize { get; set; }
    }

    public class MatchInitSideDto
    {
        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("champion")]
        public MatchInitChampionDto Champion { get; set; } = new MatchInitChampionDto();

        [JsonPropertyName("gold")]
        public int Gold { get; set; }

        [JsonPropertyName("secondaryCurrencyName")]
        public string SecondaryCurrencyName { get; set; } = "";

        [JsonPropertyName("secondaryCurrency")]
        public int SecondaryCurrency { get; set; }

        [JsonPropertyName("drawnCards")]
        public List<MatchInitCardDto> DrawnCards { get; set; } = new List<MatchInitCardDto>();
    }

    public class MatchInitChampionDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }

    public class MatchInitCardDto
    {
        [JsonPropertyName("gameCardId")]
        public int GameCardId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("hp")]
        public int Hp { get; set; }

        [JsonPropertyName("attack")]
        public int Attack { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("cardType")]
        public int CardType { get; set; }

        [JsonPropertyName("classes")]
        public List<string> Classes { get; set; } = new List<string>();

        [JsonPropertyName("states")]
        public List<string> States { get; set; } = new List<string>();

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = "";
    }
}