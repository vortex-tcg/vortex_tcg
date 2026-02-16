using game.Domaine.Match.Agregate;

namespace game.Domaine.Match.Entity;

using System;
using System.Collections.Generic;
using game.Domaine.Interface;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;

public sealed class Match
{
    public MatchId MatchId { get; } = new MatchId();

    public Player Player1 { get; }
    public Player Player2 { get; }

    public bool IsFinished { get; private set; }

    private readonly List<IEvent> _events = new List<IEvent>();

    public Match(Player p1, Player p2)
    {
        Player1 = p1;
        Player2 = p2;
    }

    public void InitMatch()
    {
        IsFinished = false;

        MatchInitData initData = InitMatchService.Init(this);

        _events.Add(new DomainEvent(MatchEvent.MATCH_INIT, initData));
    }

    public IReadOnlyList<IEvent> PullEvents()
    {
        if (_events.Count == 0)
        {
            return Array.Empty<IEvent>();
        }

        IEvent[] batch = _events.ToArray();
        _events.Clear();
        return batch;
    }

    public bool HasUser(UserId userId)
    {
        return Player1.UserId.Equals(userId) || Player2.UserId.Equals(userId);
    }

    public void Start()
    {
        IsFinished = false;
    }
}