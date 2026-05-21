using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public static class DisconnectService
{
    public static async Task HandleDisconnectAsync(UserId disconnectedUserId, CancellationToken ct = default)
    {
        RoomManager rm = RoomManager.Instance;

        await rm.Matchmaker.LeaveQueueAsync(disconnectedUserId, ct);

        Match? match = rm.GetMatchByUserId(disconnectedUserId);
        if (match == null) return;

        Guid p1         = (Guid)match.Player1.UserId;
        Guid p2         = (Guid)match.Player2.UserId;
        Guid disconnected = (Guid)disconnectedUserId;
        Guid opponent   = (disconnected == p1) ? p2 : p1;

        MatchEndedData data = new MatchEndedData(
            match.MatchId.Value,
            winnerUserId: opponent,
            loserUserId: disconnected,
            reason: "ConnectionLost"
        );

        responseDTO<MatchEndedData, MatchEndedData> payload =
            new responseDTO<MatchEndedData, MatchEndedData>
            {
                userId     = disconnected,
                opponentId = opponent,
                success    = true,
                code       = ResponseCode.SUCCESS_MATCH_ENDED,
                data       = data,
                opponentData = data
            };

        await CallManager.Instance.CallAsync(payload, ct);
        rm.RemoveMatch(match);
    }
}