using game.Domaine.Matchmaking.Interface;
using game.Domaine.Interface;
using game.Domaine;
using game.Domaine.Match.ValueObject;
using Microsoft.Extensions.Logging;
using VortexTCG.DataAccess.Models;

namespace game.Domaine.Matchmaking;

public sealed class Matchmaker : IMatchmaker, IEventContainer
{
    private readonly Dictionary<UserId, DeckId> _queue = new();
    private readonly List<IEvent> _events = new();
    private ILogger? _logger;

    internal void SetLogger(ILogger logger) => _logger = logger;

    public Task JoinQueueAsync(UserId userId, DeckId deckId, CancellationToken ct = default)
    {
        List<(UserId userId, DeckId deckId)>? matchedPair = null;

        lock (_queue)
        {
            _logger?.LogDebug("[MATCHMAKER] JoinQueue — userId={UserId} deckId={DeckId} | taille queue avant: {QueueSize}", userId, deckId, _queue.Count);

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
                _logger?.LogInformation("[MATCHMAKER] Match trouvé: {UserId} (deck {DeckId}) vs {OppUserId} (deck {OppDeckId})", userId, deckId, oppUserId.Value, oppDeckId);

                _queue.Remove(oppUserId.Value);
                _queue.Remove(userId);

                matchedPair = new List<(UserId userId, DeckId deckId)>
                {
                    (oppUserId.Value, oppDeckId),
                    (userId, deckId)
                };

                _events.Add(new MatchmakerEvent(
                    MatchmakerEvent.FOUND,
                    new MatchFoundData(matchedPair)
                ));

                _logger?.LogDebug("[MATCHMAKER] Event FOUND ajouté — queue vidée, taille: {QueueSize}", _queue.Count);
            }
            else
            {
                _logger?.LogDebug("[MATCHMAKER] Joueur {UserId} en attente dans la queue — taille queue: {QueueSize}", userId, _queue.Count);
            }
        }

        return Task.CompletedTask;
    }


    public Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
    {
        lock (_queue)
        {
            bool removed = _queue.Remove(userId);
            _logger?.LogInformation("[MATCHMAKER] LeaveQueue — userId={UserId} retiré={Removed} | taille queue: {QueueSize}", userId, removed, _queue.Count);
        }

        return Task.CompletedTask;
    }

    public IReadOnlyList<IEvent> PullEvents(CancellationToken ct = default)
    {
        lock (_queue)
        {
            if (_events.Count == 0) return Array.Empty<IEvent>();
            IEvent[] batch = _events.ToArray();
            _logger?.LogDebug("[MATCHMAKER] PullEvents — {EventCount} event(s) retourné(s)", batch.Length);
            _events.Clear();
            return batch;
        }
    }

}
