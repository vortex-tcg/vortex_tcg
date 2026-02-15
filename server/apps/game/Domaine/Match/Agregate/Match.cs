using game.Domaine.Match.Agregate;

namespace game.Domaine.Match.Entity;

using System;
using System.Collections.Generic;
using game.Domaine.Interface;
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

        Random rng = Random.Shared;

        Player1.Deck.Shuffle(rng);
        Player2.Deck.Shuffle(rng);

        Player1.Champion.Gold = new ChampionGold(Player1.Champion.BaseGold.Value);
        Player2.Champion.Gold = new ChampionGold(Player2.Champion.BaseGold.Value);

        List<GameCardDto> p1Drawn = DrawCards(Player1, 6);
        List<GameCardDto> p2Drawn = DrawCards(Player2, 5);

        MatchInitData initData = new MatchInitData(
            MatchId.Value,
            (Guid)Player1.UserId,
            (Guid)Player2.UserId,
            1,
            2,
            Player1.Champion,
            Player2.Champion,
            p1Drawn,
            p2Drawn
        );

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

    private static List<GameCardDto> DrawCards(Player player, int count)
    {
        List<GameCardDto> drawn = new List<GameCardDto>(count);

        int i = 0;
        while (i < count)
        {
            GameCardDto? card = player.Deck.DrawOne();
            if (card == null)
            {
                break;
            }

            player.Hand.Add(card);
            drawn.Add(card);
            i++;
        }

        return drawn;
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
