namespace VortexTCG.Api.Deck.DTOs
{
    public class DeckResponseDto
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = default!;
        public Guid UserId { get; set; }
        public Guid ChampionId { get; set; }
        public Guid FactionId { get; set; }
    }
}