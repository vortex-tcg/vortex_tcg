using game.Application.Service;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Infrastructure;
using game.Infrastructure.Manager;
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
        (MatchAggregate match, UserId p1Id) = BuildAttackMatch();
        AppServiceHelpers.AddMatchToRoom(match);

        await AttackService.ToggleAttackCardAsync(p1Id, 99);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public void ToggleAttackCard_AlwaysProducesEvent_GuardIsNeverTriggered()
    {
        (MatchAggregate match, UserId p1Id) = BuildAttackMatch();
        match.PullEvents(); 
        HandleAttackService.ToggleAttackCard(match, p1Id, 1, default);

        Assert.NotEmpty(match.PullEvents());
    }

    [Fact]
    public async Task ToggleAttackCardAsync_WhenCallerIsP2_SetsOtherToP1()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        UserId p2Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1, state: VCardState.Active);
        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        Player p2 = MatchHelpers.MakePlayer(userId: p2Id);
        p2.Board.Place(1, card);
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<AttackPhase>(p1: p1, p2: p2);
        match.SetCurrentPlayerPosition(2);
        AppServiceHelpers.AddMatchToRoom(match);

        List<string> calledUserIds = new();
        Mock<IHubContext<GameHubClean>> mockHub = new();
        Mock<IHubClients> mockClients = new();
        Mock<IClientProxy> mockProxy = new();
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients
            .Setup(c => c.User(It.IsAny<string>()))
            .Callback<string>(id => calledUserIds.Add(id))
            .Returns(mockProxy.Object);
        mockProxy
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        CallManager.Configure(mockHub.Object);

        await AttackService.ToggleAttackCardAsync(p2Id, 1);

        Assert.Contains(((Guid)p2Id).ToString(), calledUserIds);
        Assert.Contains(((Guid)p1Id).ToString(), calledUserIds);
    }
}
