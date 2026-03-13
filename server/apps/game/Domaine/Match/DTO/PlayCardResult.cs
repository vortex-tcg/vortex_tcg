
using GameCardDto = game.Domaine.Match.Entity.GameCardDto;

namespace game.Domaine.Match.DTO;

public sealed class PlayCardData
{
    public Guid MatchId { get; }
    public Guid PlayerUserId { get; }
    public Guid OpponentUserId { get; }

    public int PlayerGold { get; }
    public int OpponentGold { get; }

    public int BoardPosition { get; }
    public int GameCardId { get; }

    public GameCardDto PlayedCard { get; }

    public PlayCardData(
        Guid matchId,
        Guid playerUserId,
        Guid opponentUserId,
        int playerGold,
        int opponentGold,
        int boardPosition,
        int gameCardId,
        GameCardDto playedCard)
    {
        MatchId = matchId;
        PlayerUserId = playerUserId;
        OpponentUserId = opponentUserId;
        PlayerGold = playerGold;
        OpponentGold = opponentGold;
        BoardPosition = boardPosition;
        GameCardId = gameCardId;
        PlayedCard = playedCard;
    }
}
