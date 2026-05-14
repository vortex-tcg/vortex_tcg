using game.Domaine.Match.DTO;
using Xunit;

namespace game.Tests.Domain.Service;

public class MatchEndedDataTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        Guid matchId = Guid.NewGuid();
        Guid winner = Guid.NewGuid();
        Guid loser = Guid.NewGuid();

        MatchEndedData data = new MatchEndedData(matchId, winner, loser, "Surrender");

        Assert.Equal(matchId, data.MatchId);
        Assert.Equal(winner, data.WinnerUserId);
        Assert.Equal(loser, data.LoserUserId);
        Assert.Equal("Surrender", data.Reason);
    }
}

public class StandByPhaseDataTests
{
    [Fact]
    public void Constructor_SetsAllProperties_IncludingDrawnCard()
    {
        Guid matchId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();

        StandByPhaseData data = new StandByPhaseData(matchId, userId, 3, 2, 4, 5, null);

        Assert.Equal(matchId, data.MatchId);
        Assert.Equal(userId, data.CurrentPlayerUserId);
        Assert.Equal(3, data.PlayerGold);
        Assert.Equal(2, data.OpponentGold);
        Assert.Equal(4, data.PlayerHandCount);
        Assert.Equal(5, data.OpponentHandCount);
        Assert.Null(data.DrawnCard);
    }
}
