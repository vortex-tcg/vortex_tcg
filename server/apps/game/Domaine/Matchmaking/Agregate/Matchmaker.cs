using game.Domaine.Matchmaking.Interface;
using game.Domaine.Interface;
using game.Domaine;
using game.Domaine.Match.ValueObject;
using VortexTCG.DataAccess.Models;

namespace game.Domaine.Matchmaking;

public sealed class Matchmaker : IMatchmaker, IEventContainer
{
    private readonly Dictionary<UserId, DeckId> _queue = new();
    private readonly List<IEvent> _events = new();

    public Task JoinQueueAsync(UserId userId, DeckId deckId, CancellationToken ct = default)
    {
        lock (_queue)
        {
            _queue[userId] = deckId;

            UserId? oppUserId = null;
            DeckId oppDeckId = default;

            foreach (KeyValuePair<UserId, DeckId> kv in _queue)
            {
                if (kv.Key.Equals(userId)) continue;
                oppUserId = kv.Key;
                oppDeckId = kv.Value;
                break;
            }

            if (oppUserId != null)
            {
                _queue.Remove(oppUserId.Value);
                _queue.Remove(userId);

                List<(UserId userId, DeckId deckId)> matchedPair = new List<(UserId userId, DeckId deckId)>
                {
                    (oppUserId.Value, oppDeckId),
                    (userId, deckId)
                };

                _events.Add(new MatchmakerEvent(
                    MatchmakerEvent.FOUND,
                    new MatchFoundData(matchedPair)
                ));
            }
        }

        return Task.CompletedTask;
    }


    public Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
    {
        lock (_queue)
        {
            _queue.Remove(userId);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyList<IEvent> PullEvents(CancellationToken ct = default)
    {
        lock (_queue)
        {
            if (_events.Count == 0) return Array.Empty<IEvent>();
            IEvent[] batch = _events.ToArray();
            _events.Clear();
            return batch;
        }
    }

}