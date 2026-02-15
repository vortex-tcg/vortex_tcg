using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;

namespace game.Domaine.Match.Agregate;
using System;
using System.Collections.Generic;
using game.Domaine.Interface;

public static class MatchEvent
{
    public const string MATCH_INIT = "MATCH_INIT";
}

public sealed class MatchInitData
{
    public Guid MatchId { get; }
    public Guid Player1UserId { get; }
    public Guid Player2UserId { get; }

    public int Player1Position { get; }
    public int Player2Position { get; }

    public GameChampionDto Player1Champion { get; }
    public GameChampionDto Player2Champion { get; }

    public IReadOnlyList<GameCardDto> Player1DrawnCards { get; }
    public IReadOnlyList<GameCardDto> Player2DrawnCards { get; }

    public MatchInitData(
        Guid matchId,
        Guid p1UserId,
        Guid p2UserId,
        int p1Position,
        int p2Position,
        GameChampionDto p1Champion,
        GameChampionDto p2Champion,
        IReadOnlyList<GameCardDto> p1DrawnCards,
        IReadOnlyList<GameCardDto> p2DrawnCards
    )
    {
        MatchId = matchId;
        Player1UserId = p1UserId;
        Player2UserId = p2UserId;
        Player1Position = p1Position;
        Player2Position = p2Position;
        Player1Champion = p1Champion;
        Player2Champion = p2Champion;
        Player1DrawnCards = p1DrawnCards;
        Player2DrawnCards = p2DrawnCards;
    }
}

public sealed class DomainEvent : IEvent
{
    public string Name { get; }
    private readonly object _data;

    public DomainEvent(string name, object data)
    {
        Name = name;
        _data = data;
    }

    public T GetData<T>()
    {
        return (T)_data;
    }
}
