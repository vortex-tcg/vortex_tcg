using System;
using System.Collections.Generic;
using System.Threading;
using game.Domaine.Interface;
using game.Domaine.Match.Interface;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Agregate;

public sealed class Match
{
    public MatchId MatchId { get; } = new MatchId();

    public Entity.Player Player1 { get; }
    public Entity.Player Player2 { get; }

    public bool IsFinished { get; private set; }

    public int CurrentPlayerPosition { get; private set; } = 1;
    public IPhase CurrentPhase { get; private set; } = default!;

    private readonly List<IEvent> _events = new List<IEvent>();

    public Match(Entity.Player p1, Entity.Player p2, IPhase initialPhase)
    {
        Player1 = p1;
        Player2 = p2;
        CurrentPhase = initialPhase;
    }

    public void AddEvent(IEvent ev)
    {
        _events.Add(ev);
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

    public Entity.Player GetCurrentPlayer()
    {
        if (CurrentPlayerPosition == 1) return Player1;
        return Player2;
    }

    public Entity.Player GetOpponentPlayer()
    {
        if (CurrentPlayerPosition == 1) return Player2;
        return Player1;
    }

    public void SetCurrentPlayerPosition(int position)
    {
        if (position != 1 && position != 2)
        {
            throw new ArgumentException("position must be 1 or 2", nameof(position));
        }

        CurrentPlayerPosition = position;
    }

    public void ChangePhase(IPhase nextPhase, CancellationToken ct = default)
    {
        IPhase actualPhase = CurrentPhase;

        ChangePhaseService.ChangePhase(this, actualPhase, nextPhase, ct);

        CurrentPhase = nextPhase;
    }

    public void Start()
    {
        IsFinished = false;
    }
    public bool HasUser(UserId userId)
    {
        return Player1.UserId.Equals(userId) || Player2.UserId.Equals(userId);
    }
    public void InitMatch()
    {
        IsFinished = false;

        MatchInitData initData = InitMatchService.Init(this);

        _events.Add(new DomainEvent(MatchEvent.MATCH_INIT, initData));
    }

}
