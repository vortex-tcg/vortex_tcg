using game.Domaine.Match.Entity;

namespace game.Domaine.Match.DTO;

using ValueObject;


public class GameCardDtoData
{
    public Guid Id { get; set; }
    public int GameCardId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int Hp { get; set; } = default!;
    public int Attack { get; set; } = default!;
    public int BaseAttack { get; set; } = default!;
    public int Cost { get; set; } = default!;
    public int BaseCost { get; set; } = default!;
    public string Description { get; set; } = default!;
    public CardType CardType { get; set; } = default!;
    public CardClasses Classes { get; set; } = default!;
    public CardStates States { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
}
public static class GameCardMapper
{
    public static GameCardDtoData? ToData(GameCardDto? card)
    {
        if (card == null) return null;

        return new GameCardDtoData
        {
            Id = card.Id.Value,
            GameCardId = card.GameCardId.Value,
            Name = card.Name.Value,
            Hp = card.Hp.Value,
            Attack = card.Attack.Value,
            BaseAttack = card.BaseAttack.Value,
            Cost = card.Cost.Value,
            BaseCost = card.BaseCost.Value,
            Description = card.Description.Value,
            CardType = card.CardType,
            Classes = card.Classes,
            States = card.States,
            ImageUrl = card.ImageUrl.Value
        };
    }
}