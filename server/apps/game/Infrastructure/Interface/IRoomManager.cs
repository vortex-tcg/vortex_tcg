using game.Domaine.Interface;
using game.Domaine.Match.Entity;
using game.Domaine.Matchmaking.Interface;

namespace game.Infrastructure.Interface;

public interface IRoomManager
{
    IMatchmaker Matchmaker { get; }
    IEventContainer MatchmakerEventContainer { get; }
    Match CreateMatch(List<(Guid userId, Guid deckId)> players);
    Match? GetMatchByUserId(Guid userId);
    void RemoveFinishedMatches();
}