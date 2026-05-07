namespace game.Domaine.Match.Service;

using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.Entity;

public static class SurrenderService
{
    public static void Apply(Match match, Player leavingPlayer)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));
        if (leavingPlayer == null) throw new ArgumentNullException(nameof(leavingPlayer));

        if (match.IsFinished)
            return;

        Player current = match.GetCurrentPlayer();
        Player opponent = match.GetOpponentPlayer();

        Player winner = current.UserId.Equals(leavingPlayer.UserId)
            ? opponent
            : current;

        MatchEndedData data = new MatchEndedData(
            match.MatchId.Value,
            winner.UserId,
            leavingPlayer.UserId,
            "Surrender"
        );

        match.AddEvent(new DomainEvent(MatchEvent.MATCH_ENDED, data));
    }
}