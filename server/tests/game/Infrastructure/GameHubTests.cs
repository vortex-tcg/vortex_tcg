using System.Security.Claims;
using game.Infrastructure;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace game.Tests.Infrastructure;

[Collection("ApplicationTests")]
public class GameHubTests : IDisposable
{
    private readonly GameHubClean _hub;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockCaller;
    private readonly Mock<HubCallerContext> _mockContext;

    public GameHubTests()
    {
        AppServiceHelpers.ClearRoom();
        AppServiceHelpers.ClearMatchmakerQueue();
        AppServiceHelpers.ConfigureCallManager();

        _hub = new GameHubClean();
        _mockClients = new Mock<IHubCallerClients>();
        _mockCaller = new Mock<ISingleClientProxy>();
        _mockContext = new Mock<HubCallerContext>();

        _mockClients.Setup(c => c.Caller).Returns(_mockCaller.Object);
        _mockCaller
            .Setup(p => p.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _hub.Clients = _mockClients.Object;
        _hub.Context = _mockContext.Object;
    }

    public void Dispose()
    {
        AppServiceHelpers.ClearRoom();
        AppServiceHelpers.ClearMatchmakerQueue();
    }

    private void SetAuthenticatedUser(Guid userId)
    {
        ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }));
        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionAborted).Returns(CancellationToken.None);
    }

    [Fact]
    public async Task OnConnectedAsync_SendsConnectedEvent_WithConnectionId()
    {
        string connectionId = "test-connection-id";
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        await _hub.OnConnectedAsync();

        _mockCaller.Verify(p => p.SendCoreAsync(
            "Connected",
            It.Is<object[]>(args => args.Length == 1 && (string)args[0] == connectionId),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task HubMethods_ThrowHubException_WhenUserNotAuthenticated()
    {
        _mockContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);
        _mockContext.Setup(c => c.ConnectionAborted).Returns(CancellationToken.None);

        await Assert.ThrowsAsync<HubException>(() => _hub.JoinQueue(Guid.NewGuid()));
    }

    [Fact]
    public async Task ToggleDefenseCard_ThrowsHubException_WhenMatchNotFound()
    {
        SetAuthenticatedUser(Guid.NewGuid());

        await Assert.ThrowsAsync<HubException>(() => _hub.ToggleDefenseCard(1, 1));
    }

    [Fact]
    public async Task ChangePhase_ThrowsInvalidOperation_WhenMatchNotFound()
    {
        SetAuthenticatedUser(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _hub.ChangePhase());
    }

    [Fact]
    public async Task Surrender_ThrowsInvalidOperation_WhenMatchNotFound()
    {
        SetAuthenticatedUser(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _hub.Surrender());
    }

    [Fact]
    public async Task JoinQueue_DoesNotThrow_WhenAuthenticated()
    {
        SetAuthenticatedUser(Guid.NewGuid());

        Exception? ex = await Record.ExceptionAsync(() => _hub.JoinQueue(Guid.NewGuid()));

        Assert.Null(ex);
    }

    [Fact]
    public async Task LeaveQueue_DoesNotThrow_WhenAuthenticated()
    {
        SetAuthenticatedUser(Guid.NewGuid());
        await _hub.JoinQueue(Guid.NewGuid());

        Exception? ex = await Record.ExceptionAsync(() => _hub.LeaveQueue());

        Assert.Null(ex);
    }

    [Fact]
    public async Task ToggleAttackCard_ThrowsInvalidOperation_WhenMatchNotFound()
    {
        SetAuthenticatedUser(Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _hub.ToggleAttackCard(0));
    }

    [Fact]
    public async Task PlayCard_ThrowsHubException_WhenMatchNotFound()
    {
        SetAuthenticatedUser(Guid.NewGuid());

        await Assert.ThrowsAsync<HubException>(() => _hub.PlayCard(0, 0));
    }
}
