using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Agregate;
using ValueObject;
using Entity;

public class DeckData
{
    public DeckId DeckId { get; set; }
    public List<GameCardDto> Cards { get; set; } = new();
    public GameChampionDto Champion { get; set; } = default!;
}