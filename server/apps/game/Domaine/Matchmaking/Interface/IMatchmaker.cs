using game.Domaine.Match.ValueObject;

namespace game.Domaine.Matchmaking.Interface;

public interface IMatchmaker
{
    Task JoinQueueAsync(UserId userId, DeckId deckId, CancellationToken ct = default);
    Task LeaveQueueAsync(UserId userId, CancellationToken ct = default);
}