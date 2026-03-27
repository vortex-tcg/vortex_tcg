namespace game.Domaine.Match.DTO;
using System;
using game.Domaine.Match.Entity;
public sealed class StandByPhaseData
{
    public Guid MatchId { get; }
    public Guid CurrentPlayerUserId { get; }

    public int PlayerGold { get; }
    public int OpponentGold { get; }

    public int PlayerHandCount { get; }
    public int OpponentHandCount { get; }

    public GameCardDtoData? DrawnCard { get; }

    public StandByPhaseData(
        Guid matchId,
        Guid currentPlayerUserId,
        int playerGold,
        int opponentGold,
        int playerHandCount,
        int opponentHandCount,
        GameCardDtoData? drawnCard
    )
    {
        MatchId = matchId;
        CurrentPlayerUserId = currentPlayerUserId;
        PlayerGold = playerGold;
        OpponentGold = opponentGold;
        PlayerHandCount = playerHandCount;
        OpponentHandCount = opponentHandCount;
        DrawnCard = drawnCard;
    }
}
