namespace Deck.DTOs
{
    using VortexTCG.Api.Card.DTOs;
    public class DeckDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<CardDto> Cards { get; set; }
    }
}
