using game.Domaine.Match.DTO;
using game.Domaine.Match.Event.Action;

namespace game.Tests.Domain.Event;

public class AttackOrderUpdatedDataTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        Guid matchId = Guid.NewGuid();
        List<EngagedCardDto> cards = [new EngagedCardDto { Position = 1, GameCardId = 10, AttackOrder = 1 }];

        AttackOrderUpdatedData data = new(matchId, cards);

        Assert.Equal(matchId, data.MatchId);
        Assert.Same(cards, data.EngagedCards);
    }

    [Fact]
    public void Constructor_WithEmptyList_SetsEmptyEngagedCards()
    {
        Guid matchId = Guid.NewGuid();

        AttackOrderUpdatedData data = new(matchId, []);

        Assert.Empty(data.EngagedCards);
    }

    [Fact]
    public void Constructor_WithMultipleCards_PreservesOrder()
    {
        Guid matchId = Guid.NewGuid();
        List<EngagedCardDto> cards =
        [
            new EngagedCardDto { Position = 1, GameCardId = 10, AttackOrder = 1 },
            new EngagedCardDto { Position = 2, GameCardId = 20, AttackOrder = 2 },
            new EngagedCardDto { Position = 3, GameCardId = 30, AttackOrder = 3 },
        ];

        AttackOrderUpdatedData data = new(matchId, cards);

        Assert.Equal(3, data.EngagedCards.Count);
        Assert.Equal(1, data.EngagedCards[0].Position);
        Assert.Equal(20, data.EngagedCards[1].GameCardId);
        Assert.Equal(3, data.EngagedCards[2].AttackOrder);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        Guid matchId = Guid.NewGuid();
        List<EngagedCardDto> cards = [];

        AttackOrderUpdatedData a = new(matchId, cards);
        AttackOrderUpdatedData b = new(matchId, cards);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentMatchId_AreNotEqual()
    {
        List<EngagedCardDto> cards = [];

        AttackOrderUpdatedData a = new(Guid.NewGuid(), cards);
        AttackOrderUpdatedData b = new(Guid.NewGuid(), cards);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equality_DifferentListReference_AreNotEqual()
    {
        Guid matchId = Guid.NewGuid();

        AttackOrderUpdatedData a = new(matchId, []);
        AttackOrderUpdatedData b = new(matchId, []);

        Assert.NotEqual(a, b);
    }
}

public class AttackEventConstantsTests
{
    [Fact]
    public void AttackOrderUpdated_HasExpectedValue()
    {
        Assert.Equal("ATTACK_ORDER_UPDATED", AttackEvent.ATTACK_ORDER_UPDATED);
    }
}
