using game.Domaine.Interface;
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

            if (ev.Name == MatchmakerEvent.FOUND)
            {
                MatchFoundData data = ev.GetData<MatchFoundData>();
                rm.CreateMatch(data.players);
            }
        }
    }

    public static Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
        => RoomManager.Instance.Matchmaker.LeaveQueueAsync(userId, ct);
}