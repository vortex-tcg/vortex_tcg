using game.Application.Dto;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;

namespace game.Tests.Domain.Application;

public class MatchInitDtoMapperTests
{
    [Fact]
    public void ToChampionDto_MapsName()
    {
        GameChampionDto champion = MatchHelpers.MakeChampion();

        MatchInitChampionDto dto = MatchInitDtoMapper.ToChampionDto(champion);

        Assert.Equal(champion.Name.Value, dto.name);
    }

    [Fact]
    public void ToChampionDto_MapsDescription()
    {
        GameChampionDto champion = MatchHelpers.MakeChampion();

        MatchInitChampionDto dto = MatchInitDtoMapper.ToChampionDto(champion);

        Assert.Equal(champion.Description.Value, dto.description);
    }

    [Fact]
    public void ToChampionDto_MapsHp()
    {
        GameChampionDto champion = MatchHelpers.MakeChampion(hp: 45);

        MatchInitChampionDto dto = MatchInitDtoMapper.ToChampionDto(champion);

        Assert.Equal(45, dto.hp);
    }

    [Fact]
    public void ToCardDto_MapsGameCardId()
    {
        GameCardDto card = MatchHelpers.MakeCard(gameCardId: 7);

        MatchInitCardDto dto = MatchInitDtoMapper.ToCardDto(card);

        Assert.Equal(7, dto.gameCardId);
    }

    [Fact]
    public void ToCardDto_MapsHpAttackCost()
    {
        GameCardDto card = MatchHelpers.MakeCard(gameCardId: 3, hp: 8, attack: 4, cost: 2);

        MatchInitCardDto dto = MatchInitDtoMapper.ToCardDto(card);

        Assert.Equal(8, dto.hp);
        Assert.Equal(4, dto.attack);
        Assert.Equal(2, dto.cost);
    }

    [Fact]
    public void ToCardDto_NullImageUrl_ReturnsEmpty()
    {
        GameCardDto card = MatchHelpers.MakeCard(1);
        card.ImageUrl = new CardImageUrl("null");

        MatchInitCardDto dto = MatchInitDtoMapper.ToCardDto(card);

        Assert.Equal("", dto.imageUrl);
    }

    [Fact]
    public void ToCardDto_MapsClasses()
    {
        GameCardDto card = MatchHelpers.MakeCard(1);
        card.Classes = new CardClasses(new[] { "Warrior", "Mage" });

        MatchInitCardDto dto = MatchInitDtoMapper.ToCardDto(card);

        Assert.Equal(2, dto.classes.Count);
        Assert.Contains("Warrior", dto.classes);
        Assert.Contains("Mage", dto.classes);
    }

    [Fact]
    public void ToCardDto_MapsStates()
    {
        GameCardDto card = MatchHelpers.MakeCard(1);

        MatchInitCardDto dto = MatchInitDtoMapper.ToCardDto(card);

        Assert.Single(dto.states);
    }
}
