using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;

namespace game.Tests.Domain.Agregate;

public class AttackHandlerTests
{
    [Fact]
    public void AddAttack_AddsCardToList()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);

        Assert.True(handler.IsEngaged(1));
        Assert.Single(handler.GetAttackers());
    }

    [Fact]
    public void AddAttack_DuplicatePosition_DoesNotAdd()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(1, 20);

        Assert.Single(handler.GetAttackers());
    }

    [Fact]
    public void AddAttack_DuplicateGameCardId_DoesNotAdd()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(2, 10);

        Assert.Single(handler.GetAttackers());
    }

    [Fact]
    public void AddAttack_AssignsAttackOrderSequentially()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(2, 20);
        handler.AddAttack(3, 30);

        IReadOnlyList<AttackCard> attackers = handler.GetAttackers();
        Assert.Equal(1, attackers[0].AttackOrder);
        Assert.Equal(2, attackers[1].AttackOrder);
        Assert.Equal(3, attackers[2].AttackOrder);
    }

    [Fact]
    public void RemoveAttackByPosition_RemovesCard()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(2, 20);
        handler.RemoveAttackByPosition(1);

        Assert.False(handler.IsEngaged(1));
        Assert.True(handler.IsEngaged(2));
    }

    [Fact]
    public void RemoveAttackByPosition_ReordersRemainingCards()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(2, 20);
        handler.AddAttack(3, 30);
        handler.RemoveAttackByPosition(1);

        IReadOnlyList<AttackCard> attackers = handler.GetAttackers();
        Assert.Equal(2, attackers.Count);
        Assert.Equal(1, attackers[0].AttackOrder);
        Assert.Equal(2, attackers[1].AttackOrder);
    }

    [Fact]
    public void RemoveAttackByGameCardId_RemovesCorrectCard()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(2, 20);
        handler.RemoveAttackByGameCardId(10);

        Assert.False(handler.IsEngaged(1));
        Assert.True(handler.IsEngaged(2));
    }

    [Fact]
    public void HasAttackAtPosition_ReturnsTrueWhenPresent()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);

        Assert.True(handler.HasAttackAtPosition(1));
        Assert.False(handler.HasAttackAtPosition(2));
    }

    [Fact]
    public void ResetAttackHandler_ClearsAllAttackers()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(2, 20);
        handler.ResetAttackHandler();

        Assert.Empty(handler.GetAttackers());
    }

    [Fact]
    public void FormatAttackResponseDto_MapsEngagedCards()
    {
        AttackHandler handler = new AttackHandler();
        handler.AddAttack(1, 10);
        handler.AddAttack(2, 20);

        AttackOrderUpdatedDto dto = handler.FormatAttackResponseDto();

        Assert.Equal(2, dto.EngagedCards.Count);
        Assert.Contains(dto.EngagedCards, c => c.Position == 1 && c.GameCardId == 10);
        Assert.Contains(dto.EngagedCards, c => c.Position == 2 && c.GameCardId == 20);
    }
}
