using game.Domaine.Interface;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Infrastructure.Interface;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public class QueueService
{

    private readonly RoomManager _rm = RoomManager.Instance;

    public async Task JoinQueueAsync(UserId userId, DeckId deckId, CancellationToken ct = default)
    {
        await _rm.Matchmaker.JoinQueueAsync(userId, deckId, ct);
        IReadOnlyList<IEvent> events = _rm.MatchmakerEventContainer.PullEvents(ct);

        for (int i = 0; i < events.Count; i++)
        {
            IEvent ev = events[i];

            if (ev.Name == MatchmakerEvent.FOUND)
            {
                MatchFoundData data = ev.GetData<MatchFoundData>();
                _rm.CreateMatch(data.players);
            }
        }
    }

    public Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
        => _rm.Matchmaker.LeaveQueueAsync(userId, ct);
}