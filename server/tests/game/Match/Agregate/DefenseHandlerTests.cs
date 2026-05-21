using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;

namespace game.Tests.Domain.Agregate;

public class DefenseHandlerTests
{
    [Fact]
    public void AddOrReplaceDefense_AddsNewDefense()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(defensePosition: 1, gameCardId: 10, attackPosition: 2);

        Assert.True(handler.IsDefenseEngaged(1));
        Assert.True(handler.HasDefenseOnAttack(2));
    }

    [Fact]
    public void AddOrReplaceDefense_ReplacesExistingDefenseOnSameAttack()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);
        handler.AddOrReplaceDefense(3, 30, 2);

        Assert.False(handler.IsDefenseEngaged(1));
        Assert.True(handler.IsDefenseEngaged(3));
        Assert.Single(handler.GetDefenders());
    }

    [Fact]
    public void IsDefenseEngagedOnAttack_ReturnsTrueForCorrectPair()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);

        Assert.True(handler.IsDefenseEngagedOnAttack(defensePosition: 1, attackPosition: 2));
        Assert.False(handler.IsDefenseEngagedOnAttack(defensePosition: 1, attackPosition: 3));
    }

    [Fact]
    public void RemoveDefenseByPosition_RemovesCorrectCard()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);
        handler.AddOrReplaceDefense(3, 30, 4);
        handler.RemoveDefenseByPosition(1);

        Assert.False(handler.IsDefenseEngaged(1));
        Assert.True(handler.IsDefenseEngaged(3));
    }

    [Fact]
    public void RemoveDefenseByGameCardId_RemovesCorrectCard()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);
        handler.AddOrReplaceDefense(3, 30, 4);
        handler.RemoveDefenseByGameCardId(10);

        Assert.False(handler.IsDefenseEngaged(1));
        Assert.True(handler.IsDefenseEngaged(3));
    }

    [Fact]
    public void RemoveDefenseByAttackPosition_RemovesCorrectCard()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);
        handler.AddOrReplaceDefense(3, 30, 4);
        handler.RemoveDefenseByAttackPosition(2);

        Assert.False(handler.HasDefenseOnAttack(2));
        Assert.True(handler.HasDefenseOnAttack(4));
    }

    [Fact]
    public void GetDefenseByDefensePosition_ReturnsCorrectCard()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);

        DefenseCard? card = handler.GetDefenseByDefensePosition(1);

        Assert.NotNull(card);
        Assert.Equal(1, card!.Position);
        Assert.Equal(10, card.GameCardId);
    }

    [Fact]
    public void GetDefenseByDefensePosition_ReturnsNullWhenNotFound()
    {
        DefenseHandler handler = new DefenseHandler();

        DefenseCard? card = handler.GetDefenseByDefensePosition(99);

        Assert.Null(card);
    }

    [Fact]
    public void GetDefenseByAttackPosition_ReturnsCorrectCard()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);

        DefenseCard? card = handler.GetDefenseByAttackPosition(2);

        Assert.NotNull(card);
        Assert.Equal(1, card!.Position);
        Assert.Equal(10, card.GameCardId);
    }

    [Fact]
    public void GetDefenseByAttackPosition_ReturnsNullWhenNotFound()
    {
        DefenseHandler handler = new DefenseHandler();

        DefenseCard? card = handler.GetDefenseByAttackPosition(99);

        Assert.Null(card);
    }

    [Fact]
    public void ResetDefenseHandler_ClearsAll()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);
        handler.AddOrReplaceDefense(3, 30, 4);
        handler.ResetDefenseHandler();

        Assert.Empty(handler.GetDefenders());
    }

    [Fact]
    public void FormatDefenseResponseDto_MapsEngagedCards()
    {
        DefenseHandler handler = new DefenseHandler();
        handler.AddOrReplaceDefense(1, 10, 2);

        DefenseUpdatedDto dto = handler.FormatDefenseResponseDto();

        Assert.Single(dto.EngagedCards);
        Assert.Equal(1, dto.EngagedCards[0].Position);
        Assert.Equal(10, dto.EngagedCards[0].GameCardId);
        Assert.Equal(2, dto.EngagedCards[0].PositionOpponentCard);
    }
}
