using System.Security.Claims;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Infrastructure;
using game.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MatchAggregate = game.Domaine.Match.Agregate.Match;

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

        _hub = new GameHubClean(NullLogger<GameHubClean>.Instance);
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

    [Fact]
    public async Task GetAuthenticatedUserId_ThrowsHubException_WhenNoIdentifierClaim()
    {
        ClaimsPrincipal principal = new ClaimsPrincipal(new ClaimsIdentity());
        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-id");
        _mockContext.Setup(c => c.ConnectionAborted).Returns(CancellationToken.None);

        await Assert.ThrowsAsync<HubException>(() => _hub.JoinQueue(Guid.NewGuid()));
    }

    [Fact]
    public async Task OnConnectedAsync_WithAuthenticatedUser_SendsConnectedEvent()
    {
        string connectionId = "auth-conn-id";
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        SetAuthenticatedUser(Guid.NewGuid());

        await _hub.OnConnectedAsync();

        _mockCaller.Verify(p => p.SendCoreAsync(
            "Connected",
            It.Is<object[]>(args => args.Length == 1 && (string)args[0] == connectionId),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task PlayCard_FindsMatch_WhenMatchExistsForUser()
    {
        Guid userId = Guid.NewGuid();
        Player p1 = MatchHelpers.MakePlayer(userId: new UserId(userId));
        MatchAggregate match = MatchHelpers.MakeMatch(p1: p1);
        AppServiceHelpers.AddMatchToRoom(match);
        SetAuthenticatedUser(userId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _hub.PlayCard(1, 1));
    }

    [Fact]
    public async Task OnDisconnectedAsync_WhenExceptionProvided_LogsWarning()
    {
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-id");
        SetAuthenticatedUser(Guid.NewGuid());

        await _hub.OnDisconnectedAsync(new Exception("test error"));
    }

    [Fact]
    public async Task OnDisconnectedAsync_WhenNoException_CallsDisconnect()
    {
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-id");
        SetAuthenticatedUser(Guid.NewGuid());

        await _hub.OnDisconnectedAsync(null);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WhenUserNotAuthenticated_SkipsDisconnect()
    {
        _mockContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-id");

        await _hub.OnDisconnectedAsync(null);
    }

    [Fact]
    public async Task OnConnectedAsync_WhenUserHasNoIdentifierClaim_LogsUnauthenticated()
    {
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-id");
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        await _hub.OnConnectedAsync();

        _mockCaller.Verify(p => p.SendCoreAsync(
            "Connected",
            It.IsAny<object[]>(),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task OnDisconnectedAsync_WhenUserHasNoIdentifierClaim_SkipsDisconnect()
    {
        _mockContext.Setup(c => c.ConnectionId).Returns("conn-id");
        _mockContext.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        await _hub.OnDisconnectedAsync(null);
    }

    [Fact]
    public async Task ToggleDefenseCard_CompletesSuccessfully_WhenMatchIsInDefensePhase()
    {
        Guid userId = Guid.NewGuid();
        Player p2 = MatchHelpers.MakePlayer(userId: new UserId(userId));
        MatchAggregate match = MatchHelpers.MakeMatchInPhase<DefensePhase>(p2: p2);
        match.SetCurrentPlayerPosition(2);
        AppServiceHelpers.AddMatchToRoom(match);
        SetAuthenticatedUser(userId);

        Exception? ex = await Record.ExceptionAsync(() => _hub.ToggleDefenseCard(1, 1));

        Assert.Null(ex);
    }
}
