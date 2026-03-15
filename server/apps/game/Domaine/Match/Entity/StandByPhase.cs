using game.Domaine.Match.DTO;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.Interface;

namespace game.Domaine.Match.Entity;

using System.Threading;
using Agregate;
using ValueObject;


public sealed class StandByPhase : IPhase
{
    public MatchPhaseType Type => MatchPhaseType.StandBy;

    public void OnStartPhase(Match match, CancellationToken ct = default)
    {
        Player current = match.GetCurrentPlayer();
        Player opponent = match.GetOpponentPlayer();
        current.Champion.BaseGold = new ChampionBaseGold(Math.Min(current.Champion.BaseGold.Value + 1, 10));
        current.Champion.Gold = new ChampionGold(current.Champion.BaseGold.Value);       
        GameCardDto? drawn = current.Deck.DrawOne();
        if (drawn != null)
        {
            current.Hand.Add(drawn);
        }

        GameCardDtoData? drawnCardData = GameCardMapper.ToData(drawn);

        StandByPhaseData data = new StandByPhaseData(
            match.MatchId.Value,
            current.UserId,
            current.Champion.Gold.Value,
            opponent.Champion.Gold.Value,
            current.Hand.Count,
            opponent.Hand.Count,
            drawnCardData
        );

        match.AddEvent(new DomainEvent(PhaseEvent.STANDBY_STARTED, data));
    }

    public void OnEndPhase(Match match, CancellationToken ct = default)
    {
        //TODO
    }
}
