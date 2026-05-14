using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;
using game.Tests.Helpers;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Infrastructure.Manager;

[Collection("ApplicationTests")]
public class RoomManagerTests : IDisposable
{
    public RoomManagerTests() => AppServiceHelpers.ClearRoom();
    public void Dispose() => AppServiceHelpers.ClearRoom();

    [Fact]
    public async Task CreateMatchAsync_ThrowsArgumentException_WhenNotTwoPlayers()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            RoomManager.Instance.CreateMatchAsync(new List<(UserId, DeckId)>()));
    }

    [Fact]
    public async Task CreateMatchAsync_ThrowsArgumentException_WhenOnlyOnePlayer()
    {
        var players = new List<(UserId, DeckId)>
        {
            (new UserId(Guid.NewGuid()), new DeckId(Guid.NewGuid()))
        };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            RoomManager.Instance.CreateMatchAsync(players));
    }

    [Fact]
    public void RemoveMatch_WithNull_DoesNotThrow()
    {
        Exception? ex = Record.Exception(() => RoomManager.Instance.RemoveMatch(null!));

        Assert.Null(ex);
    }

    [Fact]
    public void RemoveMatch_RemovesMatchFromRoom()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        AppServiceHelpers.AddMatchToRoom(match);

        RoomManager.Instance.RemoveMatch(match);

        MatchAggregate? found = RoomManager.Instance.GetMatchByUserId(match.Player1.UserId);
        Assert.Null(found);
    }

    [Fact]
    public void GetMatchByUserId_ReturnsNull_WhenNoMatchExists()
    {
        MatchAggregate? result = RoomManager.Instance.GetMatchByUserId(new UserId(Guid.NewGuid()));

        Assert.Null(result);
    }

    [Fact]
    public void GetMatchByUserId_ReturnsMatch_WhenUserIsInRoom()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        AppServiceHelpers.AddMatchToRoom(match);

        MatchAggregate? found = RoomManager.Instance.GetMatchByUserId(match.Player1.UserId);

        Assert.Same(match, found);
    }

    [Fact]
    public void RemoveFinishedMatches_LeavesActiveMatches()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        AppServiceHelpers.AddMatchToRoom(match);

        RoomManager.Instance.RemoveFinishedMatches();

        MatchAggregate? found = RoomManager.Instance.GetMatchByUserId(match.Player1.UserId);
        Assert.NotNull(found);
    }
}
