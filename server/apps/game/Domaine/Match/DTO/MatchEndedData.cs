namespace game.Domaine.Match.DTO;

public sealed class MatchEndedData
{
    public Guid MatchId { get; }
    public Guid WinnerUserId { get; }
    public Guid LoserUserId { get; }
    public string Reason { get; }

    public MatchEndedData(
        Guid matchId,
        Guid winnerUserId,
        Guid loserUserId,
        string reason
    )
    {
        MatchId = matchId;
        WinnerUserId = winnerUserId;
        LoserUserId = loserUserId;
        Reason = reason;
    }
}