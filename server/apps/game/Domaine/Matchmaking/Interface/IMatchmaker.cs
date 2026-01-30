namespace game.Domaine.Matchmaking.Interface;

public interface IMatchmaker
{
    Task JoinQueueAsync(Guid userId, Guid deckId, CancellationToken ct = default);
    Task LeaveQueueAsync(Guid userId, CancellationToken ct = default);
}