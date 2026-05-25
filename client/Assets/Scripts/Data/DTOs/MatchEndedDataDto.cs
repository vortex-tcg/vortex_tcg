using System;

namespace VortexTCG.Scripts.DTOs
{
    public sealed class MatchEndedDataDto
    {
        public Guid MatchId { get; set; }
        public Guid WinnerUserId { get; set; }
        public Guid LoserUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}