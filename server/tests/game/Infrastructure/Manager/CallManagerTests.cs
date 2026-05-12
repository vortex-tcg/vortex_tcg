using game.Application.Dto;
using game.Application.Enum;
using game.Infrastructure;
using game.Infrastructure.Manager;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace game.Tests.Infrastructure.Manager;

[Collection("ApplicationTests")]
public class CallManagerTests
{
    private static (Mock<IHubContext<GameHubClean>>, Mock<IHubClients>, Mock<IClientProxy>) ConfigureHub(
        string? userId = null, string? opponentId = null)
    {
        Mock<IHubContext<GameHubClean>> hub = new Mock<IHubContext<GameHubClean>>();
        Mock<IHubClients> clients = new Mock<IHubClients>();
        Mock<IClientProxy> proxy = new Mock<IClientProxy>();

        proxy.Setup(p => p.SendCoreAsync(
            It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        hub.Setup(h => h.Clients).Returns(clients.Object);
        clients.Setup(c => c.User(It.IsAny<string>())).Returns(proxy.Object);

        CallManager.Configure(hub.Object);
        return (hub, clients, proxy);
    }

    [Fact]
    public async Task CallAsync_DoesNothing_WhenResponseIsNull()
    {
        ConfigureHub();

        Exception? ex = await Record.ExceptionAsync(() =>
            CallManager.Instance.CallAsync<string, string>(null!));

        Assert.Null(ex);
    }

    [Fact]
    public async Task CallAsync_SendsErrorToPlayer_WhenSuccessIsFalse()
    {
        Guid userId = Guid.NewGuid();
        (_, _, Mock<IClientProxy> proxy) = ConfigureHub();

        responseDTO<string, string> response = new responseDTO<string, string>
        {
            userId = userId,
            opponentId = Guid.Empty,
            success = false,
            code = ResponseCode.NOT_FOUND
        };

        await CallManager.Instance.CallAsync(response);

        proxy.Verify(p => p.SendCoreAsync(
            "Error",
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task CallAsync_SendsToPlayerAndOpponent_WhenSuccessIsTrue()
    {
        Guid userId = Guid.NewGuid();
        Guid opponentId = Guid.NewGuid();

        Mock<IClientProxy> playerProxy = new Mock<IClientProxy>();
        Mock<IClientProxy> opponentProxy = new Mock<IClientProxy>();
        playerProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        opponentProxy.Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Mock<IHubClients> clients = new Mock<IHubClients>();
        clients.Setup(c => c.User(userId.ToString())).Returns(playerProxy.Object);
        clients.Setup(c => c.User(opponentId.ToString())).Returns(opponentProxy.Object);

        Mock<IHubContext<GameHubClean>> hub = new Mock<IHubContext<GameHubClean>>();
        hub.Setup(h => h.Clients).Returns(clients.Object);
        CallManager.Configure(hub.Object);

        responseDTO<string, string> response = new responseDTO<string, string>
        {
            userId = userId,
            opponentId = opponentId,
            success = true,
            code = ResponseCode.SUCCESS_PHASE_CHANGED,
            data = "player-data",
            opponentData = "opponent-data"
        };

        await CallManager.Instance.CallAsync(response);

        playerProxy.Verify(p => p.SendCoreAsync(
            "successPhaseChanged", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once());
        opponentProxy.Verify(p => p.SendCoreAsync(
            "opponentPhaseChanged", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task CallAsync_SkipsPlayer_WhenUserIdIsEmpty()
    {
        (_, Mock<IHubClients> clients, _) = ConfigureHub();

        responseDTO<string, string> response = new responseDTO<string, string>
        {
            userId = Guid.Empty,
            opponentId = Guid.Empty,
            success = false,
            code = ResponseCode.NOT_FOUND
        };

        await CallManager.Instance.CallAsync(response);

        clients.Verify(c => c.User(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task CallAsync_SkipsOpponent_WhenOpponentIdIsEmpty()
    {
        Guid userId = Guid.NewGuid();
        (_, Mock<IHubClients> clients, Mock<IClientProxy> proxy) = ConfigureHub();

        responseDTO<string, string> response = new responseDTO<string, string>
        {
            userId = userId,
            opponentId = Guid.Empty,
            success = true,
            code = ResponseCode.SUCCESS_PHASE_CHANGED,
            data = "data",
            opponentData = null
        };

        await CallManager.Instance.CallAsync(response);

        clients.Verify(c => c.User(It.IsAny<string>()), Times.Once());
        proxy.Verify(p => p.SendCoreAsync(
            "successPhaseChanged", It.IsAny<object[]>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
