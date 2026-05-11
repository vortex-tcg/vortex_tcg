using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;

namespace game.Tests.Matchmaking.Event;

public class MatchmakerEventTests
{
    [Fact]
    public void FOUND_Constant_HasCorrectValue()
    {
        Assert.Equal("FOUND", MatchmakerEvent.FOUND);
    }

    [Fact]
    public void Name_IsSetFromConstructor()
    {
        MatchmakerEvent ev = new MatchmakerEvent("TEST_EVENT", new object());

        Assert.Equal("TEST_EVENT", ev.Name);
    }

    [Fact]
    public void GetData_ReturnsCastData()
    {
        string payload = "some data";
        MatchmakerEvent ev = new MatchmakerEvent(MatchmakerEvent.FOUND, payload);

        string result = ev.GetData<string>();

        Assert.Equal(payload, result);
    }

    [Fact]
    public void GetData_ReturnsMatchFoundData_WhenDataIsMatchFoundData()
    {
        UserId userId = new UserId(Guid.NewGuid());
        DeckId deckId = new DeckId(Guid.NewGuid());
        MatchFoundData data = new MatchFoundData(new List<(UserId, DeckId)> { (userId, deckId) });
        MatchmakerEvent ev = new MatchmakerEvent(MatchmakerEvent.FOUND, data);

        MatchFoundData result = ev.GetData<MatchFoundData>();

        Assert.Same(data, result);
    }
}
