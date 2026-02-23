using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using CardState = game.Domaine.Match.Entity.CardState;

namespace game.Application.Dto;
using System;
using System.Collections.Generic;
public sealed class MatchInitUserDto
{
    public Guid matchId { get; init; }

    public MatchInitSideDto self { get; init; } = new MatchInitSideDto();
    public MatchInitSideDto opponent { get; init; } = new MatchInitSideDto();
    public int opponentHandSize { get; init; }
}

public sealed class MatchInitSideDto
{
    public int position { get; init; }

    public MatchInitChampionDto champion { get; init; } = new MatchInitChampionDto();

    public int gold { get; init; }

    public string secondaryCurrencyName { get; init; } = "";
    public int secondaryCurrency { get; init; }

    public IReadOnlyList<MatchInitCardDto> drawnCards { get; init; } = Array.Empty<MatchInitCardDto>();
}
public sealed class MatchInitChampionDto
{
    public string name { get; init; } = "";
    public string description { get; init; } = "";
}
public sealed class MatchInitCardDto
{
    public int gameCardId { get; init; }
    public string name { get; init; } = "";
    public int hp { get; init; }
    public int attack { get; init; }
    public int cost { get; init; }
    public string description { get; init; } = "";

    public int cardType { get; init; }
    public IReadOnlyList<string> classes { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> states { get; init; } = Array.Empty<string>();
    public string imageUrl { get; init; } = "";
}
public static class MatchInitDtoMapper
{
    public static MatchInitChampionDto ToChampionDto(GameChampionDto champ)
    {
        return new MatchInitChampionDto
        {
            name = GetChampionName(champ),
            description = GetChampionDescription(champ)
        };
    }

    public static MatchInitCardDto ToCardDto(GameCardDto card)
    {
        List<string> classes = new List<string>();
        foreach (string c in card.Classes.Value)
        {
            classes.Add(c);
        }

        List<string> states = new List<string>
        {
            card.States.Value.ToString()
        };

        return new MatchInitCardDto
        {
            gameCardId = card.GameCardId.Value,
            name = card.Name.Value,
            hp = card.Hp.Value,
            attack = card.Attack.Value,
            cost = card.Cost.Value,
            description = card.Description.Value,
            cardType = (int)card.CardType,
            classes = classes,
            states = states,
            imageUrl = card.ImageUrl.Value == "null" ? "" : card.ImageUrl.Value
        };
    }

    private static string GetChampionName(GameChampionDto champ)
    {
        return champ.Name.Value;
    }

    private static string GetChampionDescription(GameChampionDto champ)
    {
        return champ.Description.Value;
    }
}
