using game.Domaine.Interface;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking.Interface;

namespace game.Infrastructure.Interface;

public interface IRoomManager
{
    IMatchmaker Matchmaker { get; }
    IEventContainer MatchmakerEventContainer { get; }
    Match CreateMatch(List<(UserId userId, DeckId deckId)> players);
    Match? GetMatchByUserId(Guid userId);
    void RemoveFinishedMatches();
}