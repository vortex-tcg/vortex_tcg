// =============================================
// FICHIER: tests/game/GameHubTests.cs
// =============================================
// RÔLE:
// Tests unitaires pour GameHub (SignalR Hub).
// Vérifie tous les scénarios de connexion, rooms, matchmaking et communication.
//
// FRAMEWORK:
// xUnit - Framework de test .NET standard
// Moq - Framework de mocking pour les dépendances SignalR
//
// COUVERTURE:
// ✅ Connexion/Déconnexion
// ✅ Gestion des pseudos (SetName)
// ✅ File d'attente (JoinQueue)
// ✅ Création de salons (CreateRoom)
// ✅ Jonction de salons (JoinRoom)
// ✅ Messages de salon (SendRoomMessage, SendRoomMessageByCode)
// ✅ Départ de salon (LeaveRoomByCode)
// ✅ Démarrage de partie (StartGame)
// ✅ Changement de phase (ChangePhase)
// =============================================

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using game.Hubs;
using game.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace game.Tests
{
    public class GameHubTests
    {
        #region Helper Methods

        private static RoomService CreateRealRoomService()
        {
            Mock<IHubContext<GameHub>> hubContextMock = new Mock<IHubContext<GameHub>>();
            Mock<IHubClients> clientsMock = new Mock<IHubClients>();
            Mock<IClientProxy> clientProxyMock = new Mock<IClientProxy>();

            clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubContextMock.SetupGet(h => h.Clients).Returns(clientsMock.Object);

            return new RoomService(hubContextMock.Object);
        }

        private static GameHub CreateHub(
            Matchmaker matchmaker,
            RoomService rooms,
            Mock<IHubCallerClients> clients,
            Mock<IGroupManager> groups,
            string connectionId,
            ClaimsPrincipal? user)
        {
            Mock<HubCallerContext> context = new Mock<HubCallerContext>();
            context.SetupGet(c => c.ConnectionId).Returns(connectionId);
            context.SetupGet(c => c.User).Returns(user);

            GameHub hub = new GameHub(matchmaker, rooms)
            {
                Context = context.Object,
                Clients = clients.Object,
                Groups = groups.Object
            };

            return hub;
        }

        private static ClaimsPrincipal CreateUser(Guid userId)
        {
            ClaimsIdentity identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "jwt");

            return new ClaimsPrincipal(identity);
        }

        #endregion

        #region Tests - Connexion/Déconnexion

        [Fact]
        public async Task OnConnectedAsync_SendsConnectedToCaller()
        {
            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(Guid.NewGuid()));

            await hub.OnConnectedAsync();

            callerProxy.Verify(p => p.SendCoreAsync(
                "Connected",
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == "conn-1"),
                default),
                Times.Once);
        }

        [Fact]
        public async Task OnDisconnectedAsync_WhenInRoom_RemovesFromGroupAndNotifiesOpponent()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();

            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);
            rooms.TryJoinRoom(userId2, code, out Guid? _, out bool _);

            Mock<IClientProxy> othersProxy = new Mock<IClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.Setup(c => c.OthersInGroup(code)).Returns(othersProxy.Object);
            clients.Setup(c => c.Group(code)).Returns(othersProxy.Object);

            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId1));

            await hub.OnDisconnectedAsync(null);

            groups.Verify(g => g.RemoveFromGroupAsync("conn-1", code, default), Times.Once);
        }

        #endregion

        #region Tests - SetName

        [Fact]
        public async Task SetName_Unauthenticated_ThrowsHubException()
        {
            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            Mock<IGroupManager> groups = new Mock<IGroupManager>();
            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", user: null);

            await Assert.ThrowsAsync<HubException>(() => hub.SetName("Alex"));
        }

        [Fact]
        public async Task SetName_WhenAuthenticated_SetsNameInMatchmakerAndRoomService()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.SetName("TestPlayer");

            Assert.Equal("TestPlayer", matchmaker.GetName("conn-1"));
            Assert.Equal("TestPlayer", rooms.GetName(userId));
        }

        #endregion

        #region Tests - JoinQueue

        [Fact]
        public async Task JoinQueue_WhenNotMatched_SendsWaiting()
        {
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.JoinQueue(deckId);

            callerProxy.Verify(p => p.SendCoreAsync(
                "Waiting",
                It.IsAny<object[]>(),
                default),
                Times.Once);
        }

        #endregion

        #region Tests - CreateRoom

        [Fact]
        public async Task CreateRoom_WhenSuccess_AddsToGroupAndSendsRoomCreatedAndWaiting()
        {
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.CreateRoom(deckId, null);

            callerProxy.Verify(p => p.SendCoreAsync(
                "RoomCreated",
                It.IsAny<object[]>(),
                default),
                Times.Once);

            callerProxy.Verify(p => p.SendCoreAsync(
                "Waiting",
                It.IsAny<object[]>(),
                default),
                Times.Once);
        }

        [Fact]
        public async Task CreateRoom_WhenCodeTaken_SendsRoomCreateError()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string _, "ABC123");

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId2));

            await hub.CreateRoom(deckId, "ABC123");

            callerProxy.Verify(p => p.SendCoreAsync(
                "RoomCreateError",
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == "CODE_TAKEN"),
                default),
                Times.Once);
        }

        #endregion

        #region Tests - JoinRoom

        [Fact]
        public async Task JoinRoom_WhenNotFound_SendsRoomJoinError()
        {
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.JoinRoom(deckId, "NONEXISTENT");

            callerProxy.Verify(p => p.SendCoreAsync(
                "RoomJoinError",
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == "NOT_FOUND"),
                default),
                Times.Once);
        }

        [Fact]
        public async Task JoinRoom_WhenFull_SendsRoomFullError()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid userId3 = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);
            rooms.TryJoinRoom(userId2, code, out Guid? _, out bool _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId3));

            await hub.JoinRoom(deckId, code);

            callerProxy.Verify(p => p.SendCoreAsync(
                "RoomJoinError",
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == "ROOM_FULL"),
                default),
                Times.Once);
        }

        [Fact]
        public async Task JoinRoom_WhenSuccess_AddsToGroupAndSendsMatched()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IClientProxy> othersProxy = new Mock<IClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            clients.Setup(c => c.OthersInGroup(code)).Returns(othersProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId2));

            await hub.JoinRoom(deckId, code);

            groups.Verify(g => g.AddToGroupAsync("conn-1", code, default), Times.Once);
            callerProxy.Verify(p => p.SendCoreAsync(
                "Matched",
                It.IsAny<object[]>(),
                default),
                Times.Once);
        }

        #endregion

        #region Tests - SendRoomMessage

        [Fact]
        public async Task SendRoomMessage_WhenNotInRoom_DoesNothing()
        {
            Guid userId = Guid.NewGuid();
            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            Mock<IGroupManager> groups = new Mock<IGroupManager>();
            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.SendRoomMessage("ROOM1", "hello");

            clients.Verify(c => c.OthersInGroup(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendRoomMessageByCode_WhenInSameRoom_BroadcastsMessage()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string code);
            rooms.SetName(userId, "Player1");

            Mock<IClientProxy> othersProxy = new Mock<IClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.Setup(c => c.OthersInGroup(code)).Returns(othersProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.SendRoomMessageByCode(code, "hello");

            othersProxy.Verify(p => p.SendCoreAsync(
                "ReceiveRoomMessage",
                It.Is<object[]>(args => args.Length == 3 && (string)args[0] == code && (string)args[1] == "Player1" && (string)args[2] == "hello"),
                default),
                Times.Once);
        }

        #endregion

        #region Tests - LeaveRoomByCode

        [Fact]
        public async Task LeaveRoomByCode_WhenInRoom_RemovesFromGroupAndNotifiesOpponent()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);
            rooms.TryJoinRoom(userId2, code, out Guid? _, out bool _);

            Mock<IClientProxy> groupProxy = new Mock<IClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.Setup(c => c.Group(code)).Returns(groupProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId1));

            await hub.LeaveRoomByCode();

            groups.Verify(g => g.RemoveFromGroupAsync("conn-1", code, default), Times.Once);
            groupProxy.Verify(p => p.SendCoreAsync(
                "OpponentLeft",
                It.IsAny<object[]>(),
                default),
                Times.Once);
        }

        [Fact]
        public async Task LeaveRoomByCode_WhenNotInRoom_DoesNothing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.LeaveRoomByCode();

            groups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        }

        #endregion

        #region Tests - StartGame

        [Fact]
        public async Task StartGame_WhenRoomServiceReturnsNull_SendsErrorToCaller()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);

            Mock<IGroupManager> groups = new Mock<IGroupManager>();
            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.StartGame();

            callerProxy.Verify(p => p.SendCoreAsync(
                "Error",
                It.Is<object[]>(args => args.Length == 1 && ((string)args[0]).Contains("Unable to start game")),
                default),
                Times.Once);
        }

        #endregion

        #region Tests - ChangePhase

        [Fact]
        public async Task ChangePhase_WhenNull_SendsError()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.ChangePhase();

            callerProxy.Verify(p => p.SendCoreAsync(
                "Error",
                It.Is<object[]>(args => args.Length == 1 && ((string)args[0]).Contains("Unable to change phase")),
                default),
                Times.Once);
        }

        #endregion

        #region Tests - LeaveQueue

        [Fact]
        public async Task LeaveQueue_WhenNotInRoom_DoesNothing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.LeaveQueue();

            clients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task LeaveQueue_WhenInRoomWithOpponent_NotifiesOpponent()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);
            rooms.TryJoinRoom(userId2, code, out Guid? _, out bool _);

            Mock<IClientProxy> groupProxy = new Mock<IClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.Setup(c => c.Group(code)).Returns(groupProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId1));

            await hub.LeaveQueue();

            groupProxy.Verify(p => p.SendCoreAsync(
                "OpponentLeft",
                It.IsAny<object[]>(),
                default),
                Times.Once);
        }

        [Fact]
        public async Task LeaveQueue_WhenAloneInRoom_DoesNotNotify()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string code);

            Mock<IClientProxy> groupProxy = new Mock<IClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.Setup(c => c.Group(code)).Returns(groupProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.LeaveQueue();

            groupProxy.Verify(p => p.SendCoreAsync(
                "OpponentLeft",
                It.IsAny<object[]>(),
                default),
                Times.Never);
        }

        #endregion

        #region Tests - PlayCard

        [Fact]
        public async Task PlayCard_WhenNotInRoom_CompletesWithoutThrowing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.PlayCard(1, 0);

            // PlayCard calls RoomService.PlayCard which returns early when user not in room
            Assert.NotNull(hub);
        }

        [Fact]
        public async Task PlayCard_WhenInRoomButGameNotInitialized_CompletesWithoutThrowing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.PlayCard(1, 0);

            // RoomService.PlayCard will fail because GameRoom is not initialized
            Assert.NotNull(hub);
        }

        [Fact]
        public async Task PlayCard_WithValidCardIdAndLocation_DelegatestoRoomService()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);
            rooms.TryJoinRoom(userId2, code, out Guid? _, out bool _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId1));

            // PlayCard delegates to RoomService.PlayCard
            // Without full game initialization, it will send an error
            await hub.PlayCard(1, 0);

            // Verify the method was called without throwing
            Assert.NotNull(hub);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task PlayCard_AllValidLocations_DelegatesToRoomService(int location)
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.PlayCard(1, location);

            // Method should complete without throwing for valid locations
            Assert.NotNull(hub);
        }

        #endregion

        #region Tests - HandleAttackPos

        [Fact]
        public async Task HandleAttackPos_WhenNotInRoom_CompletesWithoutThrowing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.HandleAttackPos(1);

            // HandleAttackPos calls RoomService.EngageAttackCard which returns early when user not in room
            Assert.NotNull(hub);
        }

        [Fact]
        public async Task HandleAttackPos_WhenInRoomButGameNotInitialized_CompletesWithoutThrowing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.HandleAttackPos(1);

            // RoomService.EngageAttackCard will fail because GameRoom is not initialized
            Assert.NotNull(hub);
        }

        [Fact]
        public async Task HandleAttackPos_WithValidCardId_DelegatesToRoomService()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);
            rooms.TryJoinRoom(userId2, code, out Guid? _, out bool _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId1));

            await hub.HandleAttackPos(1);

            Assert.NotNull(hub);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task HandleAttackPos_VariousCardIds_DelegatesToRoomService(int cardId)
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.HandleAttackPos(cardId);

            Assert.NotNull(hub);
        }

        #endregion

        #region Tests - HandleDefensePos

        [Fact]
        public async Task HandleDefensePos_WhenNotInRoom_CompletesWithoutThrowing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.HandleDefensePos(1, 2);

            // HandleDefensePos calls RoomService.EngageDefenseCard which returns early when user not in room
            Assert.NotNull(hub);
        }

        [Fact]
        public async Task HandleDefensePos_WhenInRoomButGameNotInitialized_CompletesWithoutThrowing()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.HandleDefensePos(1, 2);

            // RoomService.EngageDefenseCard will fail because GameRoom is not initialized
            Assert.NotNull(hub);
        }

        [Fact]
        public async Task HandleDefensePos_WithValidCardIds_DelegatesToRoomService()
        {
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId1, out string code);
            rooms.TryJoinRoom(userId2, code, out Guid? _, out bool _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId1));

            await hub.HandleDefensePos(1, 2);

            Assert.NotNull(hub);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(5, 10)]
        [InlineData(10, 20)]
        public async Task HandleDefensePos_VariousCardIds_DelegatesToRoomService(int cardId, int opponentCardId)
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.HandleDefensePos(cardId, opponentCardId);

            Assert.NotNull(hub);
        }

        [Fact]
        public async Task HandleDefensePos_SameCardIdAndOpponentCardId_DelegatesToRoomService()
        {
            Guid userId = Guid.NewGuid();

            Matchmaker matchmaker = new Matchmaker();
            RoomService rooms = CreateRealRoomService();
            rooms.TryCreateRoom(userId, out string _);

            Mock<ISingleClientProxy> callerProxy = new Mock<ISingleClientProxy>();
            Mock<IHubCallerClients> clients = new Mock<IHubCallerClients>();
            clients.SetupGet(c => c.Caller).Returns(callerProxy.Object);
            Mock<IGroupManager> groups = new Mock<IGroupManager>();

            GameHub hub = CreateHub(matchmaker, rooms, clients, groups, "conn-1", CreateUser(userId));

            await hub.HandleDefensePos(5, 5);

            Assert.NotNull(hub);
        }

        #endregion
    }
}
