namespace game.Domaine.Match.Service;

using game.Domaine.Match.DTO;
using game.Domaine.Match.Event.Action;
using Entity;
using Agregate;

public sealed class ChampionDeathService
{
    public void CheckChampionDeath(Match match, Player damagedPlayer)
    {
        if (match == null) throw new ArgumentNullException(nameof(match));
        if (damagedPlayer == null) throw new ArgumentNullException(nameof(damagedPlayer));

        if (damagedPlayer.Champion.Hp.Value > 0)
            return;
        Player currentPlayer = match.GetCurrentPlayer();
        Player opponentPlayer = match.GetOpponentPlayer();

        Player winner = currentPlayer.UserId.Equals(damagedPlayer.UserId)
            ? opponentPlayer
            : currentPlayer;

        MatchEndedData data = new MatchEndedData(
            match.MatchId.Value,
            winner.UserId,
            damagedPlayer.UserId,
            "ChampionDead"
        );

        match.AddEvent(new DomainEvent(MatchEvent.MATCH_ENDED, data));
    }
}