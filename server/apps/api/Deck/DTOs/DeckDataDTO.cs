using VortexTCG.Api.Card.DTOs;
using VortexTCG.Api.Champion.DTOs;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.Api.Deck.DTOs
{
    public class DeckDataDto
    {
        public List<DeckCardDto> Cards { get; set; } = new();
        public DeckChampionDto Champion { get; set; } = default!;
    }
}
