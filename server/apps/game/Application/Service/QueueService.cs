using game.Domaine.Interface;
using game.Domaine.Matchmaking;
using game.Infrastructure.Interface;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public class QueueService
{
    private readonly IRoomManager _roomManager;

    public QueueService(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public async Task JoinQueueAsync(Guid userId, Guid deckId, CancellationToken ct = default)
    {
        await _roomManager.Matchmaker.JoinQueueAsync(userId, deckId, ct);
        IReadOnlyList<IEvent> events = await _roomManager.MatchmakerEventContainer.PullEventsAsync(ct);

        for (int i = 0; i < events.Count; i++)
        {
            IEvent ev = events[i];

            if (ev.Name == MatchmakerEvent.FOUND)
            {
                MatchFoundData data = ev.GetData<MatchFoundData>();
                _roomManager.CreateMatch(data.players);
            }
        }
    }

    public Task LeaveQueueAsync(Guid userId, CancellationToken ct = default)
        => _roomManager.Matchmaker.LeaveQueueAsync(userId, ct);
}