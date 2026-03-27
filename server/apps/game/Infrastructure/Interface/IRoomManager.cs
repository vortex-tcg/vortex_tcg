using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking.Interface;

namespace game.Infrastructure.Interface;

public interface IRoomManager
{
    IMatchmaker Matchmaker { get; }
    IEventContainer MatchmakerEventContainer { get; }
    Task<Match> CreateMatchAsync(List<(UserId userId, DeckId deckId)> players, CancellationToken ct = default);
    Match? GetMatchByUserId(UserId userId);
    void RemoveFinishedMatches();
}