using System;
using System.Text.Json.Serialization;

namespace VortexTCG.Scripts.DTOs
{
    [Serializable]
    public class PlayCardSignalDto
    {
        [JsonPropertyName("matchId")]
        public Guid MatchId { get; set; }

        [JsonPropertyName("self")]
        public PlayCardSignalSelfDto Self { get; set; } = new PlayCardSignalSelfDto();

        [JsonPropertyName("opponent")]
        public PlayCardSignalOpponentDto Opponent { get; set; } = new PlayCardSignalOpponentDto();
    }

    [Serializable]
    public class PlayCardSignalSelfDto
    {
        [JsonPropertyName("gold")]
        public int Gold { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("gameCardId")]
        public int GameCardId { get; set; }
    }

    [Serializable]
    public class PlayCardSignalOpponentDto
    {
        [JsonPropertyName("gold")]
        public int Gold { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("card")]
        public MatchInitCardDto Card { get; set; }
    }
}
