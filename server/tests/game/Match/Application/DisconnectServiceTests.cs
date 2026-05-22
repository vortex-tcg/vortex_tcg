using game.Application.Service;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

namespace game.Tests.Domain.Application;

[Collection("ApplicationTests")]
public class DisconnectServiceTests : IDisposable
{
    private readonly Mock<IClientProxy> _mockProxy;

    public DisconnectServiceTests()
    {
        AppServiceHelpers.ClearRoom();
        AppServiceHelpers.ClearMatchmakerQueue();
        _mockProxy = AppServiceHelpers.ConfigureCallManager();
    }

    public void Dispose()
    {
        AppServiceHelpers.ClearRoom();
        AppServiceHelpers.ClearMatchmakerQueue();
    }

    [Fact]
    public async Task HandleDisconnectAsync_ReturnsEarly_WhenMatchNotFound()
    {
        UserId userId = new UserId(Guid.NewGuid());

        await DisconnectService.HandleDisconnectAsync(userId);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task HandleDisconnectAsync_SendsMatchEnded_WhenPlayer1Disconnects()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        UserId p1Id = match.Player1.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await DisconnectService.HandleDisconnectAsync(p1Id);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task HandleDisconnectAsync_SendsMatchEnded_WhenPlayer2Disconnects()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        UserId p2Id = match.Player2.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await DisconnectService.HandleDisconnectAsync(p2Id);

        _mockProxy.Verify(p => p.SendCoreAsync(
            It.IsAny<string>(),
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task HandleDisconnectAsync_RemovesMatchFromRoom_AfterDisconnect()
    {
        MatchAggregate match = MatchHelpers.MakeMatch();
        UserId p1Id = match.Player1.UserId;
        AppServiceHelpers.AddMatchToRoom(match);

        await DisconnectService.HandleDisconnectAsync(p1Id);

        Assert.Null(RoomManager.Instance.GetMatchByUserId(p1Id));
    }
}