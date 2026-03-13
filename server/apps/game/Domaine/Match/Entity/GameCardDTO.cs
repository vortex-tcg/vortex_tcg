namespace game.Domaine.Match.Entity;

using ValueObject;


public class GameCardDto
{
    public CardId Id { get; set; }
    public GameCardId GameCardId { get; set; } = default!;
    public CardName Name { get; set; } = default!;
    public CardHpValue Hp { get; set; } = default!;
    public CardAttackValue Attack { get; set; } = default!;
    public CardAttackValue BaseAttack { get; set; } = default!;
    public CardCostValue Cost { get; set; } = default!;
    public CardCostValue BaseCost { get; set; } = default!;
    public CardDescription Description { get; set; } = default!;
    public CardType CardType { get; set; } = default!;
    public CardClasses Classes { get; set; } = default!;
    public CardStates States { get; set; } = default!;
    public CardImageUrl ImageUrl { get; set; } = default!;
}
