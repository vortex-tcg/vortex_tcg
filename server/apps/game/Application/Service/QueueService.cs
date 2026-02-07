using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Interface;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public class QueueService
{
    public static async Task JoinQueueAsync(UserId userId, DeckId deckId, CancellationToken ct = default)
    {
        RoomManager rm = RoomManager.Instance;

        await rm.Matchmaker.JoinQueueAsync(userId, deckId, ct);
        IReadOnlyList<IEvent> events = rm.MatchmakerEventContainer.PullEvents(ct);

        for (int i = 0; i < events.Count; i++)
        {
            IEvent ev = events[i];
            if (ev.Name != MatchmakerEvent.FOUND) continue;

            MatchFoundData data = ev.GetData<MatchFoundData>();
            Match match = await rm.CreateMatchAsync(data.players, ct);
            UserId p1 = data.players[0].userId;
            UserId p2 = data.players[1].userId;

            await CallManager.Instance.CallAsync(new responseDTO<object>
            {
                userId = (Guid)p1,
                opponentId = (Guid)p2,
                success = true,
                code = ResponseCode.MATCH_FOUND,
                data = new
                {
                    matchId = match.MatchId.Value,
                    opponentId = (Guid)p2
                }
            }, ct);

            await CallManager.Instance.CallAsync(new responseDTO<object>
            {
                userId = (Guid)p2,
                opponentId = (Guid)p1,
                success = true,
                code = ResponseCode.MATCH_FOUND,
                data = new
                {
                    matchId = match.MatchId.Value,
                    opponentId = (Guid)p1
                }
            }, ct);
        }

    }

    public static Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
        => RoomManager.Instance.Matchmaker.LeaveQueueAsync(userId, ct);
}