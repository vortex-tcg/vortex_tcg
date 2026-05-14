using game.Domaine.Match.ValueObject;
using Xunit;

namespace game.Tests.Domain.ValueObject;

public class MatchIdTests
{
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenGuidIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new MatchId(Guid.Empty));
    }

    [Fact]
    public void Constructor_StoresValue_WhenGuidIsValid()
    {
        Guid g = Guid.NewGuid();
        MatchId id = new MatchId(g);

        Assert.Equal(g, id.Value);
    }

    [Fact]
    public void DefaultConstructor_GeneratesNonEmptyGuid()
    {
        MatchId id = new MatchId();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void ImplicitCastToGuid_ReturnsValue()
    {
        Guid g = Guid.NewGuid();
        MatchId id = new MatchId(g);

        Guid cast = id;

        Assert.Equal(g, cast);
    }

    [Fact]
    public void ExplicitCastToUserId_ReturnsEquivalentUserId()
    {
        Guid g = Guid.NewGuid();
        MatchId id = new MatchId(g);

        UserId userId = (UserId)id;

        Assert.Equal(g, (Guid)userId);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        Guid g = Guid.NewGuid();
        MatchId id = new MatchId(g);

        Assert.Equal(g.ToString(), id.ToString());
    }
}
