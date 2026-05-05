using game.Application.Service;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Application;

[Collection("ApplicationTests")]
public class SurrenderServiceTests : IDisposable
{
    private readonly Mock<IClientProxy> _mockProxy;

    public SurrenderServiceTests()
    {
        AppServiceHelpers.ClearRoom();
        _mockProxy = AppServiceHelpers.ConfigureCallManager();
    }

    public void Dispose() => AppServiceHelpers.ClearRoom();

    [Fact]
    public async Task SurrenderAsync_ThrowsWhenMatchNotFound()
    {
        UserId userId = new UserId(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SurrenderService.SurrenderAsync(userId));
    }

    [Fact]
    public async Task SurrenderAsync_SendsSignalR_WhenPlayerSurrenders()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();
        UserId p1Id = match.Player1.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await SurrenderService.SurrenderAsync(p1Id);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task SurrenderAsync_RemovesMatchFromRoom()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();
        UserId p1Id = match.Player1.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await SurrenderService.SurrenderAsync(p1Id);

        // Match removed — next call cannot find it
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SurrenderService.SurrenderAsync(p1Id));
    }

    [Fact]
    public async Task SurrenderAsync_Player2CanSurrender()
    {
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<StandByPhase>();
        UserId p2Id = match.Player2.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await SurrenderService.SurrenderAsync(p2Id);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }
}
