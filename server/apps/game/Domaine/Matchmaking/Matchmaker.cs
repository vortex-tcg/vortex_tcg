using game.Domaine.Matchmaking.Interface;
using game.Domaine.Interface;
using game.Domaine;
namespace game.Domaine.Matchmaking;

public sealed class Matchmaker : IMatchmaker, IEventContainer
{
    private readonly object _lock = new();
    private readonly Dictionary<Guid, Guid> _queue = new(); 
    private readonly List<IEvent> _events = new();

    public Task JoinQueueAsync(Guid userId, Guid deckId, CancellationToken ct = default)
    {
        List<(Guid userId, Guid deckId)>? matchedPair = null;

        lock (_lock)
        {
            _queue[userId] = deckId;

            foreach (KeyValuePair<Guid,Guid> kv in _queue)
            {
                if (kv.Key == userId) continue;

                Guid oppUserId = kv.Key;
                Guid oppDeckId = kv.Value;
                _queue.Remove(oppUserId);
                _queue.Remove(userId);
                matchedPair = new List<(Guid userId, Guid deckId)>
                {
                    (oppUserId, oppDeckId),
                    (userId, deckId)
                };

                break;
            }
            if (matchedPair != null)
            {
                _events.Add(new MatchmakerEvent(
                    MatchmakerEvent.FOUND,
                    new MatchFoundData(matchedPair)
                ));
            }
        }
        return Task.CompletedTask;
    }

    public Task LeaveQueueAsync(Guid userId, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _queue.Remove(userId);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IEvent>> PullEventsAsync(CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (_events.Count == 0)
                return Task.FromResult<IReadOnlyList<IEvent>>(Array.Empty<IEvent>());
            IEvent[] batch = _events.ToArray();
            _events.Clear();    

            return Task.FromResult<IReadOnlyList<IEvent>>(batch);
        }
    }
}
