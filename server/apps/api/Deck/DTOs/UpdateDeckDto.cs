namespace VortexTCG.Api.Deck.DTOs
{
    public class UpdateDeckDto
    {
        public string Name { get; set; } = default!;
        public Guid ChampionId { get; set; }
        public Guid FactionId { get; set; }
        public List<UpdateDeckCardDto> Cards { get; set; } = new();
    }

    public class UpdateDeckCardDto
    {
        public Guid CollectionCardId { get; set; }
        public int Quantity { get; set; }
    }
}