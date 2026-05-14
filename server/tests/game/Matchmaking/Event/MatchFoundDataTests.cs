using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;

namespace game.Tests.Matchmaking.Event;

public class MatchFoundDataTests
{
    [Fact]
    public void Players_StoresProvidedList()
    {
        UserId userId = new UserId(Guid.NewGuid());
        DeckId deckId = new DeckId(Guid.NewGuid());
        List<(UserId, DeckId)> players = new List<(UserId, DeckId)> { (userId, deckId) };

        MatchFoundData data = new MatchFoundData(players);

        Assert.Same(players, data.players);
    }

    [Fact]
    public void Players_ContainsCorrectEntries()
    {
        UserId user1 = new UserId(Guid.NewGuid());
        UserId user2 = new UserId(Guid.NewGuid());
        DeckId deck1 = new DeckId(Guid.NewGuid());
        DeckId deck2 = new DeckId(Guid.NewGuid());

        MatchFoundData data = new MatchFoundData(new List<(UserId, DeckId)>
        {
            (user1, deck1),
            (user2, deck2)
        });

        Assert.Equal(2, data.players.Count);
        Assert.Contains(data.players, p => p.userId.Equals(user1) && p.deckId.Equals(deck1));
        Assert.Contains(data.players, p => p.userId.Equals(user2) && p.deckId.Equals(deck2));
    }

    [Fact]
    public void Players_CanBeEmpty()
    {
        MatchFoundData data = new MatchFoundData(new List<(UserId, DeckId)>());

        Assert.Empty(data.players);
    }
}
