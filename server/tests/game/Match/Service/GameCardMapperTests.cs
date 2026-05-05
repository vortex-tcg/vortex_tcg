using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Tests.Helpers;

namespace game.Tests.Domain.Service;

public class GameCardMapperTests
{
    [Fact]
    public void ToData_ReturnsNull_WhenCardIsNull()
    {
        GameCardDtoData? result = GameCardMapper.ToData(null);

        Assert.Null(result);
    }

    [Fact]
    public void ToData_MapsAllFields()
    {
        GameCardDto card = MatchHelpers.MakeCard(gameCardId: 5, hp: 8, attack: 4, cost: 3);

        GameCardDtoData? result = GameCardMapper.ToData(card);

        Assert.NotNull(result);
        Assert.Equal(5, result!.GameCardId);
        Assert.Equal(8, result.Hp);
        Assert.Equal(4, result.Attack);
        Assert.Equal(4, result.BaseAttack);
        Assert.Equal(3, result.Cost);
        Assert.Equal(3, result.BaseCost);
        Assert.Equal(card.Id.Value, result.Id);
        Assert.Equal(card.Name.Value, result.Name);
        Assert.Equal(card.Description.Value, result.Description);
        Assert.Equal(card.ImageUrl.Value, result.ImageUrl);
    }

    [Fact]
    public void ToData_MapsCardType()
    {
        GameCardDto card = MatchHelpers.MakeCard(1);
        card.CardType = CardType.GUARD;

        GameCardDtoData? result = GameCardMapper.ToData(card);

        Assert.Equal(CardType.GUARD, result!.CardType);
    }

    [Fact]
    public void ToData_MapsCardState()
    {
        GameCardDto card = MatchHelpers.MakeCard(1);

        GameCardDtoData? result = GameCardMapper.ToData(card);

        Assert.Equal(card.States.Value, result!.States.Value);
    }

    [Fact]
    public void ToData_MapsClasses()
    {
        GameCardDto card = MatchHelpers.MakeCard(1);
        card.Classes = new game.Domaine.Match.ValueObject.CardClasses(new[] { "Warrior", "Mage" });

        GameCardDtoData? result = GameCardMapper.ToData(card);

        Assert.Equal(2, result!.Classes.Value.Count);
        Assert.Contains("Warrior", result.Classes.Value);
        Assert.Contains("Mage", result.Classes.Value);
    }
}
