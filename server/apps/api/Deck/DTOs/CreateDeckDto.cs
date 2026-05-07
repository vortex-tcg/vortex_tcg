namespace VortexTCG.Api.Deck.DTOs
{
    public class CreateDeckDto
    {
        public string Label { get; set; } = default!;
        public Guid UserId { get; set; }
        public Guid ChampionId { get; set; }
        public Guid FactionId { get; set; }
    }
}