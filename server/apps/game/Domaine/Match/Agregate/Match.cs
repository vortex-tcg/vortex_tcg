using System;
using System.Collections.Generic;
using System.Threading;
using game.Domaine.Interface;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.Interface;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;

namespace game.Domaine.Match.Agregate;

public sealed class Match
{
    public MatchId MatchId { get; } = new MatchId();

    public Player Player1 { get; }
    public Player Player2 { get; }
    public AttackHandler AttackHandler { get; } = new();

    public bool IsFinished { get; private set; }
    public int CurrentPlayerPosition { get; private set; } = 1;
    public IPhase CurrentPhase { get; private set; } = default!;
    public bool HasPlayedCardThisTurn { get; private set; } = false;
    public bool HasPendingDefense { get; private set; } 

    public void SetPendingDefense(bool value) => HasPendingDefense = value;


    private readonly List<IEvent> _events = new();

    public Match(Player p1, Player p2, IPhase initialPhase)
    {
        Player1 = p1;
        Player2 = p2;
        CurrentPhase = initialPhase;
    }

    public void AddEvent(IEvent ev) => _events.Add(ev);

    public IReadOnlyList<IEvent> PullEvents()
    {
        if (_events.Count == 0) return Array.Empty<IEvent>();
        IEvent[] batch = _events.ToArray();
        _events.Clear();
        return batch;
    }

    public Player GetCurrentPlayer() => CurrentPlayerPosition == 1 ? Player1 : Player2;
    public Player GetOpponentPlayer() => CurrentPlayerPosition == 1 ? Player2 : Player1;

    public void SetCurrentPlayerPosition(int position)
    {
        if (position != 1 && position != 2)
            throw new ArgumentException("position must be 1 or 2", nameof(position));
        CurrentPlayerPosition = position;
    }

    public void SetPhase(IPhase phase) => CurrentPhase = phase;

    public void MarkCardPlayedThisTurn() => HasPlayedCardThisTurn = true;
    public void ResetTurnFlags()
    {
        HasPlayedCardThisTurn = false;
        HasPendingDefense = false;
    }
    public void Start() => IsFinished = false;

    public bool HasUser(UserId userId)
        => Player1.UserId.Equals(userId) || Player2.UserId.Equals(userId);

    public void InitMatch(CancellationToken ct = default)
    
    {
        IsFinished = false;
        MatchInitData initData = InitMatchService.Init(this);
        _events.Add(new DomainEvent(MatchEvent.MATCH_INIT, initData));
        CurrentPhase.OnStartPhase(this, ct); 
    }
    public void PlayCard(UserId userId, int gameCardId, int boardPosition, CancellationToken ct = default)
    {
        PlayCardData data = PlayCardService.PlayCard(this, userId, gameCardId, boardPosition, ct);
        MarkCardPlayedThisTurn();

        _events.Add(new DomainEvent(PlayCardEvent.CARD_PLAYED, data));
    }
    public void NextPhase(CancellationToken ct = default)
    {
        ChangePhaseService.NextPhase(this, ct);
    }
}
