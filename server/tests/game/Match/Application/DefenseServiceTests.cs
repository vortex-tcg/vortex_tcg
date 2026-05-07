using game.Application.Service;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;
using VCardState = game.Domaine.Match.ValueObject.CardState;

namespace game.Tests.Domain.Application;

[Collection("ApplicationTests")]
public class DefenseServiceTests : IDisposable
{
    private readonly Mock<IClientProxy> _mockProxy;

    public DefenseServiceTests()
    {
        AppServiceHelpers.ClearRoom();
        _mockProxy = AppServiceHelpers.ConfigureCallManager();
    }

    public void Dispose() => AppServiceHelpers.ClearRoom();

    private static (MatchAggregate match, UserId defenderId) BuildDefenseMatch()
    {
        UserId attackerId = new UserId(Guid.NewGuid());
        UserId defenderId = new UserId(Guid.NewGuid());

        GameCardDto attackCard = MatchHelpers.MakeCard(20, state: VCardState.Attacking);
        GameCardDto defenseCard = MatchHelpers.MakeCard(10, state: VCardState.Active);

        Player attacker = MatchHelpers.MakePlayer(userId: attackerId);
        attacker.Board.Place(1, attackCard);

        Player defender = MatchHelpers.MakePlayer(userId: defenderId);
        defender.Board.Place(2, defenseCard);

        MatchAggregate match = new MatchAggregate(attacker, defender, new DefensePhase());
        match.SetCurrentPlayerPosition(2);
        match.AttackHandler.AddAttack(1, 20);

        return (match, defenderId);
    }

    [Fact]
    public async Task ToggleDefenseCardAsync_ThrowsWhenMatchNotFound()
    {
        UserId userId = new UserId(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DefenseService.ToggleDefenseCardAsync(userId, 2, 1));
    }

    [Fact]
    public async Task ToggleDefenseCardAsync_ThrowsWhenNotCurrentPlayer()
    {
        (MatchAggregate match, _) = BuildDefenseMatch();
        UserId attackerId = match.Player1.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DefenseService.ToggleDefenseCardAsync(attackerId, 2, 1));
    }

    [Fact]
    public async Task ToggleDefenseCardAsync_SendsSignalR_WhenDefenseCardToggled()
    {
        (MatchAggregate match, UserId defenderId) = BuildDefenseMatch();
        AppServiceHelpers.AddMatchToRoom(match);

        await DefenseService.ToggleDefenseCardAsync(defenderId, 2, 1);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }
}
