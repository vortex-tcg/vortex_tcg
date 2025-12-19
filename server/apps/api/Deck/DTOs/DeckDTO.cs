using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Champion.DTOs;

namespace VortexTCG.Api.Deck.DTOs
{
    public class DeckDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<CardDto> Cards { get; set; }
        public DeckChampionDto Champion { get; set; }
    }
}
