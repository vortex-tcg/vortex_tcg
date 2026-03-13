namespace game.Domaine.Match.Entity;
using ValueObject;
public class GameChampionDto
{
    public ChampionId Id { get; set; }
    public DeckId DeckId { get; set; }
    public ChampionBaseHp BaseHp { get; set; }
    public ChampionHp Hp { get; set; }
    public ChampionBaseGold BaseGold { get; set; }
    public ChampionGold Gold { get; set; }
    public ChampionSecondaryCurrency SecondaryCurrency { get; set; }
    public ChampionSecondaryCurrencyName SecondaryCurrencyName { get; set; } = default!;
    public ChampionFatigueCounter FatigueCounter { get; set; }
    public ChampionName Name { get; set; } = default!;
    public ChampionDescription Description { get; set; } = default!;

}
