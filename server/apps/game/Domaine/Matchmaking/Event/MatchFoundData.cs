using game.Domaine.Match.ValueObject;

namespace game.Domaine.Matchmaking;

public sealed class MatchFoundData
{
    public List<(UserId userId, DeckId deckId)> players { get; }

    public MatchFoundData(List<(UserId userId, DeckId deckId)> players)
    {
        this.players = players;
    }
}
