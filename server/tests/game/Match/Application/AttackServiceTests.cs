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
public class AttackServiceTests : IDisposable
{
    private readonly Mock<IClientProxy> _mockProxy;

    public AttackServiceTests()
    {
        AppServiceHelpers.ClearRoom();
        _mockProxy = AppServiceHelpers.ConfigureCallManager();
    }

    public void Dispose() => AppServiceHelpers.ClearRoom();

    private static (MatchAggregate match, UserId p1Id) BuildAttackMatch()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1, state: VCardState.Active);

        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        p1.Board.Place(1, card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<AttackPhase>(p1: p1);
        return (match, p1Id);
    }

    [Fact]
    public async Task ToggleAttackCardAsync_ThrowsWhenMatchNotFound()
    {
        UserId userId = new UserId(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AttackService.ToggleAttackCardAsync(userId, 1));
    }

    [Fact]
    public async Task ToggleAttackCardAsync_ThrowsWhenNotCurrentPlayer()
    {
        (MatchAggregate match, _) = BuildAttackMatch();
        UserId p2Id = match.Player2.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AttackService.ToggleAttackCardAsync(p2Id, 1));
    }

    [Fact]
    public async Task ToggleAttackCardAsync_SendsSignalR_WhenCardToggled()
    {
        (MatchAggregate match, UserId p1Id) = BuildAttackMatch();
        AppServiceHelpers.AddMatchToRoom(match);

        await AttackService.ToggleAttackCardAsync(p1Id, 1);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ToggleAttackCardAsync_SendsSignalR_EvenWhenNoCardAtPosition()
    {
        // HandleAttackService always emits an ATTACK_ORDER_UPDATED event (even for empty board state)
        (MatchAggregate match, UserId p1Id) = BuildAttackMatch();
        AppServiceHelpers.AddMatchToRoom(match);

        await AttackService.ToggleAttackCardAsync(p1Id, 99);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }
}
