using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using Xunit;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Agregate;

public class MatchAggregateTests
{
    [Fact]
    public void SetCurrentPlayerPosition_ThrowsOnInvalidPosition()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();

        Assert.Throws<ArgumentException>(() => match.SetCurrentPlayerPosition(3));
    }

    [Fact]
    public void Start_SetsIsFinishedFalse()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();

        match.Start();

        Assert.False(match.IsFinished);
    }

    [Fact]
    public void GetOpponentPlayer_ReturnsPlayer2_WhenCurrentIsPlayer1()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();

        Player opponent = match.GetOpponentPlayer();

        Assert.Equal(match.Player2.UserId, opponent.UserId);
    }

    [Fact]
    public void HasUser_ReturnsFalse_WhenUserNotInMatch()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();

        bool result = match.HasUser(new UserId(Guid.NewGuid()));

        Assert.False(result);
    }

    [Fact]
    public void PhaseChangedData_OpponentPlayerPosition_IsCalculatedCorrectly()
    {
        DomainEvent.PhaseChangedData data = new DomainEvent.PhaseChangedData(
            Guid.NewGuid(),
            currentPlayerPosition: 2,
            MatchPhaseType.StandBy
        );

        Assert.Equal(1, data.OpponentPlayerPosition);
    }

    [Fact]
    public void PhaseChangedData_OpponentPlayerPosition_WhenCurrentIs1()
    {
        DomainEvent.PhaseChangedData data = new DomainEvent.PhaseChangedData(
            Guid.NewGuid(),
            currentPlayerPosition: 1,
            MatchPhaseType.StandBy
        );

        Assert.Equal(2, data.OpponentPlayerPosition);
    }
}
