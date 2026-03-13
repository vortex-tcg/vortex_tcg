using VortexTCG.DataAccess.Models;

namespace VortexTCG.Api.Deck.DTOs
{
    public class DeckCardDto
    {
        public Guid DeckCardId  { get; set; }
        public int Quantity { get; set; }
        public Guid CollectionCardId { get; set; }
        public Rarity Rarity { get; set; }
        public Guid CardId { get; set; }
        public string Name { get; set; } = default!;
        public int? Hp { get; set; }
        public int? Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; } = default!;
        public string Picture { get; set; } = default!;
        public Extension Extension { get; set; }
        public CardType CardType { get; set; }
        public int Price { get; set; }
        public List<string> Classes { get; set; } = new();

    }
}
