using game.Application.Service;
using game.Domaine.Match.Entity;
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
public class PhaseServiceTests : IDisposable
{
    private readonly Mock<IClientProxy> _mockProxy;

    public PhaseServiceTests()
    {
        AppServiceHelpers.ClearRoom();
        _mockProxy = AppServiceHelpers.ConfigureCallManager();
    }

    public void Dispose() => AppServiceHelpers.ClearRoom();

    [Fact]
    public async Task ChangePhaseAsync_ThrowsWhenMatchNotFound()
    {
        UserId userId = new UserId(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            PhaseService.ChangePhaseAsync(userId));
    }

    [Fact]
    public async Task ChangePhaseAsync_ThrowsWhenNotCurrentPlayer()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();
        UserId p2Id = match.Player2.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            PhaseService.ChangePhaseAsync(p2Id));
    }

    [Fact]
    public async Task ChangePhaseAsync_SendsSignalR_WhenPhaseChanges()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();
        UserId p1Id = match.Player1.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await PhaseService.ChangePhaseAsync(p1Id);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ChangePhaseAsync_SendsSignalR_WhenMatchEndedByBattleResolution()
    {
        // P1 has a card in Attacking state with high attack → P2 champion (1 HP) dies → MATCH_ENDED
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto attackCard = MatchHelpers.MakeCard(1, attack: 100, state: VCardState.Attacking);
        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        p1.Board.Place(1, attackCard);

        GameChampionDto p2Champion = MatchHelpers.MakeChampion(hp: 1);
        Player p2 = MatchHelpers.MakePlayer(champion: p2Champion);

        // AttackPhase, P1 is current (position 1), no pending defense → goes to EndTurn
        MatchAggregate match = new MatchAggregate(p1, p2, new AttackPhase());
        AppServiceHelpers.AddMatchToRoom(match);

        await PhaseService.ChangePhaseAsync(p1Id);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ChangePhaseAsync_TransitionsToAttack_WhenPlayerHasActiveCard()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        GameCardDto card = MatchHelpers.MakeCard(1);

        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        p1.Board.Place(1, card);

        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>(p1: p1);
        AppServiceHelpers.AddMatchToRoom(match);

        await PhaseService.ChangePhaseAsync(p1Id);

        Assert.Equal(MatchPhaseType.Attack, match.CurrentPhase.Type);
    }

    [Fact]
    public void NextPhase_AlwaysProducesEvent_GuardIsNeverTriggered()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();
        match.PullEvents(); 

        match.NextPhase();

        Assert.NotEmpty(match.PullEvents());
    }

    [Fact]
    public async Task ChangePhaseAsync_WhenCallerIsP2_SetsOtherToP1()
    {
        UserId p1Id = new UserId(Guid.NewGuid());
        UserId p2Id = new UserId(Guid.NewGuid());
        Player p1 = MatchHelpers.MakePlayer(userId: p1Id);
        Player p2 = MatchHelpers.MakePlayer(userId: p2Id);
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>(p1: p1, p2: p2);
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

        await PhaseService.ChangePhaseAsync(p2Id);

        Assert.Contains(((Guid)p2Id).ToString(), calledUserIds);
        Assert.Contains(((Guid)p1Id).ToString(), calledUserIds);
    }
}
