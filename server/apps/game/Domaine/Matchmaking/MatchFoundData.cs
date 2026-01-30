namespace game.Domaine.Matchmaking;

public sealed class MatchFoundData
{
    public List<(Guid userId, Guid deckId)> players { get; }

    public MatchFoundData(List<(Guid userId, Guid deckId)> players)
    {
        this.players = players;
    }
}
