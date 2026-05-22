using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public static class PlayCardAppService
{
    public static async Task PlayCardAsync(
        Match match,
        UserId userId,
        int gameCardId,
        int boardPosition,
        CancellationToken ct = default)
    { 
        match.PlayCard(userId, gameCardId, boardPosition, ct);
        IReadOnlyList<IEvent> events = match.PullEvents();
        IEvent? playedEvent = null;
        foreach (IEvent ev in events)
        {
            if (ev.Name == PlayCardEvent.CARD_PLAYED)
            {
                playedEvent = ev;
                break;
            }
        }

        PlayCardData data = playedEvent!.GetData<PlayCardData>();

        var playerDto = new PlayCardUserDto
        {
            matchId = data.MatchId,
            self = new PlayCardSelfDto
            {
                gold = data.PlayerGold,
                position = data.BoardPosition,
                gameCardId = data.GameCardId
            },
            opponent = new PlayCardOpponentDto
            {
                gold = data.OpponentGold,
                position = data.BoardPosition,
                card = null
            }
        };
        var opponentDto = new PlayCardUserDto
        {
            matchId = data.MatchId,
            self = new PlayCardSelfDto
            {
                gold = data.OpponentGold,
                position = data.BoardPosition,
                gameCardId = data.GameCardId
            },
            opponent = new PlayCardOpponentDto
            {
                gold = data.PlayerGold,
                position = data.BoardPosition,
                card = MatchInitDtoMapper.ToCardDto(data.PlayedCard)
            }
        };

        responseDTO<PlayCardUserDto, PlayCardUserDto> payload =
            new responseDTO<PlayCardUserDto, PlayCardUserDto>
            {
                userId = data.PlayerUserId,
                opponentId = data.OpponentUserId,
                success = true,
                code = ResponseCode.SUCCESS_POSE_CARTE,
                data = playerDto,
                opponentData = opponentDto
            };

        await CallManager.Instance.CallAsync(payload, ct);
    }
}
