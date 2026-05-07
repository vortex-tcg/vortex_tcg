using game.Domaine.Match.Entity;

namespace game.Application.Service;

using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;

public static class SurrenderService
{
    public static async Task SurrenderAsync(UserId userId, CancellationToken ct = default)
    {
        RoomManager rm = RoomManager.Instance;

        Match? match = rm.GetMatchByUserId(userId);
        if (match == null)
            throw new InvalidOperationException("Match not found.");

        Player player = match.Player1.UserId.Equals(userId)
            ? match.Player1
            : match.Player2;

        Domaine.Match.Service.SurrenderService.Apply(match, player);

        IReadOnlyList<IEvent> events = match.PullEvents();
        if (events.Count == 0) return;

        Guid p1 = (Guid)match.Player1.UserId;
        Guid p2 = (Guid)match.Player2.UserId;

        Guid caller = (Guid)userId;
        Guid other  = (caller == p1) ? p2 : p1;

        foreach (IEvent ev in events)
        {
            if (ev.Name == MatchEvent.MATCH_ENDED)
            {
                MatchEndedData d = ev.GetData<MatchEndedData>();

                responseDTO<MatchEndedData, MatchEndedData> payload =
                    new responseDTO<MatchEndedData, MatchEndedData>
                    {
                        userId = caller,
                        opponentId = other,
                        success = true,
                        code = ResponseCode.SUCCESS_MATCH_ENDED,
                        data = d,
                        opponentData = d
                    };

                await CallManager.Instance.CallAsync(payload, ct);
                RoomManager.Instance.RemoveMatch(match);
            }
        }
    }
}