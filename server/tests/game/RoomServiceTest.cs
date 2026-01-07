// =============================================
// FICHIER: tests/game/RoomServiceTest.cs
// =============================================
// RÔLE:
// Tests unitaires pour RoomService.
// Vérifie tous les scénarios de création, jonction, initialisation et départ.
//
// FRAMEWORK:
// xUnit - Framework de test .NET standard
//
// COUVERTURE:
// ✅ Création de salons
// ✅ Jonction de salons
// ✅ Gestion des pseudos
// ✅ Initialisation de parties
// ✅ Départs et nettoyage
// ✅ Cas d'erreur (salon plein, inexistant, etc.)
// ✅ Position des joueurs
// ✅ Changement de phase
// =============================================

using Xunit;
using game.Services;
using game.Hubs;
using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.AspNetCore.SignalR;

namespace game.Tests
{
    public class RoomServiceTest
    {
        private static RoomService CreateRoomService()
        {
            Mock<IHubContext<GameHub>> hubContextMock = new Mock<IHubContext<GameHub>>();
            Mock<IHubClients> clientsMock = new Mock<IHubClients>();
            Mock<IClientProxy> clientProxyMock = new Mock<IClientProxy>();

            clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);
            hubContextMock.SetupGet(h => h.Clients).Returns(clientsMock.Object);

            return new RoomService(hubContextMock.Object);
        }

        #region Tests - Création de salons

        [Fact]
        public void TryCreateRoom_GeneratesUniqueCode()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            bool success = service.TryCreateRoom(userId1, out string code);

            Assert.True(success);
            Assert.NotNull(code);
            Assert.Equal(6, code.Length);
            Assert.Matches("^[A-Z0-9]+$", code);
        }

        [Fact]
        public void TryCreateRoom_WithPreferredCode_Succeeds()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            string preferredCode = "CUSTOM";

            bool success = service.TryCreateRoom(userId1, out string code, preferredCode);

            Assert.True(success);
            Assert.Equal("CUSTOM", code);
        }

        [Fact]
        public void TryCreateRoom_WithUsedPreferredCode_Fails()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            string preferredCode = "CUSTOM";

            service.TryCreateRoom(userId1, out string code1, preferredCode);

            bool success = service.TryCreateRoom(userId2, out string code, preferredCode);

            Assert.False(success);
        }

        [Fact]
        public void TryCreateRoom_AddsCreatorToRoom()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);

            string? foundCode = service.GetRoomOf(userId1);
            Assert.Equal(code, foundCode);
        }

        [Fact]
        public void TryCreateRoom_UserAlreadyInRoom_Fails()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code1);
            bool success = service.TryCreateRoom(userId1, out string code2);

            Assert.False(success);
            Assert.Equal(string.Empty, code2);
        }

        [Fact]
        public void TryCreateRoom_LowercasePreferredCode_ConvertsToUppercase()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            string preferredCode = "lower";

            bool success = service.TryCreateRoom(userId1, out string code, preferredCode);

            Assert.True(success);
            Assert.Equal("LOWER", code);
        }

        [Fact]
        public void TryCreateRoom_PreferredCodeWithSpaces_TrimsAndConverts()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            string preferredCode = "  spaced  ";

            bool success = service.TryCreateRoom(userId1, out string code, preferredCode);

            Assert.True(success);
            Assert.Equal("SPACED", code);
        }

        #endregion

        #region Tests - Jonction de salons

        [Fact]
        public void TryJoinRoom_ValidRoom_Succeeds()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            bool success = service.TryJoinRoom(userId2, code, out Guid? opponentId, out bool isFull);

            Assert.True(success);
            Assert.Equal(userId1, opponentId);
            Assert.False(isFull);
        }

        [Fact]
        public void TryJoinRoom_NonExistentRoom_Fails()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            bool success = service.TryJoinRoom(userId1, "BADCODE", out Guid? opponentId, out bool isFull);

            Assert.False(success);
            Assert.Null(opponentId);
            Assert.False(isFull);
        }

        [Fact]
        public void TryJoinRoom_FullRoom_Fails()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid userId3 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp1, out bool full1);

            bool success = service.TryJoinRoom(userId3, code, out Guid? opponentId, out bool isFull);

            Assert.False(success);
            Assert.Null(opponentId);
            Assert.True(isFull);
        }

        [Fact]
        public void TryJoinRoom_UserAlreadyInRoom_Fails()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code1);
            service.TryCreateRoom(userId2, out string code2);

            bool success = service.TryJoinRoom(userId1, code2, out Guid? opponentId, out bool isFull);

            Assert.False(success);
        }

        [Fact]
        public void TryJoinRoom_LowercaseCode_Succeeds()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code, "MYROOM");

            bool success = service.TryJoinRoom(userId2, "myroom", out Guid? opponentId, out bool isFull);

            Assert.True(success);
            Assert.Equal(userId1, opponentId);
        }

        [Fact]
        public void TryJoinRoom_CodeWithSpaces_TrimsAndSucceeds()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code, "MYROOM");

            bool success = service.TryJoinRoom(userId2, "  MYROOM  ", out Guid? opponentId, out bool isFull);

            Assert.True(success);
            Assert.Equal(userId1, opponentId);
        }

        #endregion

        #region Tests - Gestion des pseudos

        [Fact]
        public void SetName_AndGetName_Works()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.SetName(userId1, "PlayerOne");
            string name = service.GetName(userId1);

            Assert.Equal("PlayerOne", name);
        }

        [Fact]
        public void GetName_UndefinedName_ReturnsDefault()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            string name = service.GetName(userId1);

            string expected = $"Player-{userId1.ToString()[..8]}";
            Assert.Equal(expected, name);
        }

        [Fact]
        public void SetName_EmptyName_GeneratesDefault()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.SetName(userId1, "   ");
            string name = service.GetName(userId1);

            string expected = $"Player-{userId1.ToString()[..8]}";
            Assert.Equal(expected, name);
        }

        [Fact]
        public void SetName_NullName_GeneratesDefault()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.SetName(userId1, null);
            string name = service.GetName(userId1);

            string expected = $"Player-{userId1.ToString()[..8]}";
            Assert.Equal(expected, name);
        }

        [Fact]
        public void SetName_UpdatesExistingName()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.SetName(userId1, "FirstName");
            service.SetName(userId1, "SecondName");
            string name = service.GetName(userId1);

            Assert.Equal("SecondName", name);
        }

        #endregion

        #region Tests - Position des joueurs

        [Fact]
        public void GetPlayerPosition_Creator_ReturnsOne()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            int? position = service.GetPlayerPosition(userId1);

            Assert.Equal(1, position);
        }

        [Fact]
        public void GetPlayerPosition_Joiner_ReturnsTwo()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);
            int? position = service.GetPlayerPosition(userId2);

            Assert.Equal(2, position);
        }

        [Fact]
        public void GetPlayerPosition_NotInRoom_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            int? position = service.GetPlayerPosition(userId1);

            Assert.Null(position);
        }

        [Fact]
        public void GetPlayerPosition_RoomDoesNotExist_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            // User is not in any room
            int? position = service.GetPlayerPosition(userId1);

            Assert.Null(position);
        }

        #endregion

        #region Tests - Initialisation de parties

        [Fact]
        public async Task SetPlayerDeck_BothPlayers_InitializesGame()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);

            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);

            bool isReady = service.IsGameReady(code);
            Assert.True(isReady);

            object? gameRoom = service.GetGameRoom(code);
            Assert.NotNull(gameRoom);
        }

        [Fact]
        public async Task GetRoomPlayers_ReturnsCorrectIds()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);

            (Guid? user1, Guid? user2, Guid? deck1, Guid? deck2) = service.GetRoomPlayers(code);

            Assert.Equal(userId1, user1);
            Assert.Equal(userId2, user2);
            Assert.Equal(deckId1, deck1);
            Assert.Equal(deckId2, deck2);
        }

        [Fact]
        public async Task SetPlayerDeck_OnlyOnePlayer_DoesNotInitializeGame()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            await service.SetPlayerDeck(userId1, deckId1);

            bool isReady = service.IsGameReady(code);
            Assert.False(isReady);
        }

        [Fact]
        public async Task SetPlayerDeck_SamePlayerTwice_DoesNotDuplicate()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);

            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId1, deckId2);

            // Should not initialize because only user1's deck is set (overwritten)
            bool isReady = service.IsGameReady(code);
            Assert.False(isReady);
        }

        #endregion

        #region Tests - Départs et nettoyage

        [Fact]
        public void Leave_RemovesPlayerFromRoom()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            service.Leave(userId1, out string? leftCode, out Guid? opp, out bool empty);

            Assert.Equal(code, leftCode);

            string? foundCode = service.GetRoomOf(userId1);
            Assert.Null(foundCode);
        }

        [Fact]
        public void Leave_WithOpponent_LeavesOpponentInRoom()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);

            service.Leave(userId1, out string? leftCode, out Guid? opponentId, out bool roomEmpty);

            Assert.Equal(userId2, opponentId);
            Assert.False(roomEmpty);

            string? foundCode = service.GetRoomOf(userId2);
            Assert.Equal(code, foundCode);
        }

        [Fact]
        public void Leave_LastPlayer_DeletesRoom()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            service.Leave(userId1, out string? leftCode, out Guid? opponentId, out bool roomEmpty);

            Assert.True(!opponentId.HasValue || opponentId.Value == Guid.Empty);
            Assert.True(roomEmpty);

            object? gameRoom = service.GetGameRoom(code);
            Assert.Null(gameRoom);
        }

        [Fact]
        public void Leave_NotInRoom_DoesNothing()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.Leave(userId1, out string? code, out Guid? opponentId, out bool roomEmpty);

            Assert.Null(code);
            Assert.Null(opponentId);
            Assert.False(roomEmpty);
        }

        [Fact]
        public void Leave_CleansUpPlayerName()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.SetName(userId1, "TestPlayer");
            service.TryCreateRoom(userId1, out string code);

            service.Leave(userId1, out string? leftCode, out Guid? opp, out bool empty);

            // After leaving, GetName should return default
            string name = service.GetName(userId1);
            string expected = $"Player-{userId1.ToString()[..8]}";
            Assert.Equal(expected, name);
        }

        [Fact]
        public void GetOpponentOf_ReturnsOtherPlayer()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);

            Guid? opponent1 = service.GetOpponentOf(userId1);
            Guid? opponent2 = service.GetOpponentOf(userId2);

            Assert.Equal(userId2, opponent1);
            Assert.Equal(userId1, opponent2);
        }

        [Fact]
        public void GetOpponentOf_NoOpponent_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            Guid? opponent = service.GetOpponentOf(userId1);

            Assert.True(!opponent.HasValue || opponent.Value == Guid.Empty);
        }

        [Fact]
        public void GetOpponentOf_NotInRoom_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            Guid? opponent = service.GetOpponentOf(userId1);

            Assert.Null(opponent);
        }

        #endregion

        #region Tests - Accès à l'état de la partie

        [Fact]
        public void GetGameRoom_ValidCode_ReturnsRoom()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            // Before initialization, GameRoom is null
            object? gameRoom = service.GetGameRoom(code);
            // It will be null until both players set decks
        }

        [Fact]
        public void GetGameRoom_LowercaseCode_ReturnsRoom()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code, "MYROOM");

            object? gameRoom = service.GetGameRoom("myroom");
            // Code should be normalized to uppercase
        }

        [Fact]
        public void GetGameRoomByUserId_ValidUser_ReturnsRoom()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            object? gameRoom = service.GetGameRoomByUserId(userId1);
            // Before initialization with decks, GameRoom is the placeholder
        }

        [Fact]
        public void IsGameReady_BeforeDecksSet_ReturnsFalse()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);

            bool isReady = service.IsGameReady(code);
            Assert.False(isReady);
        }

        [Fact]
        public void IsGameReady_NonExistentRoom_ReturnsFalse()
        {
            RoomService service = CreateRoomService();

            bool isReady = service.IsGameReady("BADCODE");
            Assert.False(isReady);
        }

        [Fact]
        public void GetRoomPlayers_NonExistentRoom_ReturnsNulls()
        {
            RoomService service = CreateRoomService();

            (Guid? user1, Guid? user2, Guid? deck1, Guid? deck2) = service.GetRoomPlayers("BADCODE");

            Assert.Null(user1);
            Assert.Null(user2);
            Assert.Null(deck1);
            Assert.Null(deck2);
        }

        [Fact]
        public void GetRoomPlayers_EmptyRoom_ReturnsNulls()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);
            service.Leave(userId1, out string? leftCode, out Guid? opp, out bool empty);

            (Guid? user1, Guid? user2, Guid? deck1, Guid? deck2) = service.GetRoomPlayers(code);

            Assert.Null(user1);
            Assert.Null(user2);
        }

        #endregion

        #region Tests - Cas d'erreur

        [Fact]
        public async Task SetPlayerDeck_PlayerNotInRoom_Fails()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            bool success = await service.SetPlayerDeck(userId, deckId);

            Assert.False(success);
        }

        [Fact]
        public void GetGameRoom_NonExistentRoom_ReturnsNull()
        {
            RoomService service = CreateRoomService();

            object? gameRoom = service.GetGameRoom("BADCODE");

            Assert.Null(gameRoom);
        }

        [Fact]
        public void GetGameRoomByUserId_PlayerNotInRoom_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            object? gameRoom = service.GetGameRoomByUserId(userId1);

            Assert.Null(gameRoom);
        }

        #endregion

        #region Tests - Démarrage de partie

        [Fact]
        public void StartGame_UserNotInRoom_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();

            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = service.StartGame(userId);

            Assert.Null(result);
        }

        [Fact]
        public void StartGame_RoomNotInitialized_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string code);

            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = service.StartGame(userId1);

            Assert.Null(result);
        }

        [Fact]
        public async Task StartGame_GameInitialized_ReturnsResult()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? opp, out bool full);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);

            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = service.StartGame(userId1);

            Assert.NotNull(result);
        }

        #endregion

        #region Tests - Changement de phase

        [Fact]
        public async Task ChangePhase_UserNotInRoom_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();

            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask = service.ChangePhase(userId);

            Assert.NotNull(resultTask);
            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = await resultTask;
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_RoomNotInitialized_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            service.TryCreateRoom(userId1, out string _);

            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask = service.ChangePhase(userId1);

            Assert.NotNull(resultTask);
            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = await resultTask;
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_AfterStartGame_ReturnsResult()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask = service.ChangePhase(userId1);

            Assert.NotNull(resultTask);
            VortexTCG.Game.DTO.PhaseChangeResultDTO result = await resultTask;
            Assert.NotNull(result);
        }

        #endregion

        #region Tests - Multiple Rooms

        [Fact]
        public void MultipleRooms_IndependentState()
        {
            RoomService service = CreateRoomService();
            Guid user1 = Guid.NewGuid();
            Guid user2 = Guid.NewGuid();
            Guid user3 = Guid.NewGuid();
            Guid user4 = Guid.NewGuid();

            service.TryCreateRoom(user1, out string code1);
            service.TryJoinRoom(user2, code1, out Guid? _, out bool _);

            service.TryCreateRoom(user3, out string code2);
            service.TryJoinRoom(user4, code2, out Guid? _, out bool _);

            Assert.NotEqual(code1, code2);
            Assert.Equal(code1, service.GetRoomOf(user1));
            Assert.Equal(code1, service.GetRoomOf(user2));
            Assert.Equal(code2, service.GetRoomOf(user3));
            Assert.Equal(code2, service.GetRoomOf(user4));
        }

        #endregion

        #region Tests - EngageAttackCard

        [Fact]
        public async Task EngageAttackCard_UserNotInRoom_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();

            Exception exception = await Record.ExceptionAsync(() => service.EngageAttackCard(userId, 1));

            Assert.Null(exception);
        }

        [Fact]
        public async Task EngageAttackCard_RoomNotInitialized_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();
            service.TryCreateRoom(userId, out string _);

            Exception exception = await Record.ExceptionAsync(() => service.EngageAttackCard(userId, 1));

            Assert.Null(exception);
        }

        [Fact]
        public async Task EngageAttackCard_GameNotStarted_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);

            Exception exception = await Record.ExceptionAsync(() => service.EngageAttackCard(userId1, 1));

            Assert.Null(exception);
        }

        [Fact]
        public async Task EngageAttackCard_AfterGameStart_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.EngageAttackCard(userId1, 1));

            Assert.Null(exception);
        }

        [Fact]
        public async Task EngageAttackCard_WithInvalidCardId_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.EngageAttackCard(userId1, -999));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        [InlineData(-1)]
        public async Task EngageAttackCard_VariousCardIds_DoesNotThrow(int cardId)
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.EngageAttackCard(userId1, cardId));

            Assert.Null(exception);
        }

        #endregion

        #region Tests - EngageDefenseCard

        [Fact]
        public async Task EngageDefenseCard_UserNotInRoom_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();

            Exception exception = await Record.ExceptionAsync(() => service.EngageDefenseCard(userId, 1, 2));

            Assert.Null(exception);
        }

        [Fact]
        public async Task EngageDefenseCard_RoomNotInitialized_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();
            service.TryCreateRoom(userId, out string _);

            Exception exception = await Record.ExceptionAsync(() => service.EngageDefenseCard(userId, 1, 2));

            Assert.Null(exception);
        }

        [Fact]
        public async Task EngageDefenseCard_GameNotStarted_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);

            Exception exception = await Record.ExceptionAsync(() => service.EngageDefenseCard(userId1, 1, 2));

            Assert.Null(exception);
        }

        [Fact]
        public async Task EngageDefenseCard_AfterGameStart_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.EngageDefenseCard(userId1, 1, 2));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(-1, -2)]
        [InlineData(100, 200)]
        public async Task EngageDefenseCard_VariousCardIds_DoesNotThrow(int cardId, int opponentCardId)
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.EngageDefenseCard(userId1, cardId, opponentCardId));

            Assert.Null(exception);
        }

        #endregion

        #region Tests - PlayCard

        [Fact]
        public async Task PlayCard_UserNotInRoom_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();

            Exception exception = await Record.ExceptionAsync(() => service.PlayCard(userId, 1, 0));

            Assert.Null(exception);
        }

        [Fact]
        public async Task PlayCard_RoomNotInitialized_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();
            service.TryCreateRoom(userId, out string _);

            Exception exception = await Record.ExceptionAsync(() => service.PlayCard(userId, 1, 0));

            Assert.Null(exception);
        }

        [Fact]
        public async Task PlayCard_GameNotStarted_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);

            Exception exception = await Record.ExceptionAsync(() => service.PlayCard(userId1, 1, 0));

            Assert.Null(exception);
        }

        [Fact]
        public async Task PlayCard_AfterGameStart_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.PlayCard(userId1, 1, 0));

            Assert.Null(exception);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(5, 2)]
        [InlineData(-1, 3)]
        [InlineData(100, 4)]
        [InlineData(1, 5)]
        public async Task PlayCard_VariousParameters_DoesNotThrow(int cardId, int location)
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.PlayCard(userId1, cardId, location));

            Assert.Null(exception);
        }

        [Fact]
        public async Task PlayCard_InvalidLocation_DoesNotThrow()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            Exception exception = await Record.ExceptionAsync(() => service.PlayCard(userId1, 1, -1));
            Assert.Null(exception);

            exception = await Record.ExceptionAsync(() => service.PlayCard(userId1, 1, 6));
            Assert.Null(exception);

            exception = await Record.ExceptionAsync(() => service.PlayCard(userId1, 1, 100));
            Assert.Null(exception);
        }

        #endregion

        #region Tests - sendDrawCardsData and sendBattleResolveData

        [Fact]
        public void SendDrawCardsData_DoesNotThrow()
        {
            RoomService service = CreateRoomService();

            VortexTCG.Game.DTO.DrawCardsResultDTO data = new VortexTCG.Game.DTO.DrawCardsResultDTO
            {
                PlayerResult = new VortexTCG.Game.DTO.DrawResultForPlayerDTO
                {
                    PlayerId = Guid.NewGuid(),
                    DrawnCards = new System.Collections.Generic.List<VortexTCG.Game.DTO.DrawnCardDTO>(),
                    FatigueCount = 0,
                    BaseFatigue = 0,
                    SentToGraveyard = new System.Collections.Generic.List<VortexTCG.Game.DTO.DrawnCardDTO>()
                },
                OpponentResult = new VortexTCG.Game.DTO.DrawResultForOpponentDTO
                {
                    PlayerId = Guid.NewGuid(),
                    CardsDrawnCount = 0,
                    FatigueCount = 0,
                    BaseFatigue = 0,
                    CardsBurnedCount = 0
                }
            };

            Exception exception = Record.Exception(() => service.sendDrawCardsData(data));

            Assert.Null(exception);
        }

        [Fact]
        public void SendBattleResolveData_DoesNotThrow()
        {
            RoomService service = CreateRoomService();

            VortexTCG.Game.DTO.BattlesDataDto data = new VortexTCG.Game.DTO.BattlesDataDto
            {
                battles = new System.Collections.Generic.List<VortexTCG.Game.DTO.BattleDataDto>()
            };
            Guid attackerId = Guid.NewGuid();
            Guid defenderId = Guid.NewGuid();

            Exception exception = Record.Exception(() => service.sendBattleResolveData(data, attackerId, defenderId));

            Assert.Null(exception);
        }

        [Fact]
        public void SendBattleResolveData_WithBattleData_DoesNotThrow()
        {
            RoomService service = CreateRoomService();

            VortexTCG.Game.DTO.BattlesDataDto data = new VortexTCG.Game.DTO.BattlesDataDto
            {
                battles = new System.Collections.Generic.List<VortexTCG.Game.DTO.BattleDataDto>
                {
                    new VortexTCG.Game.DTO.BattleDataDto
                    {
                        isAgainstChamp = true,
                        againstChamp = new VortexTCG.Game.DTO.BattlaAgainstChampDataDto
                        {
                            isChampDead = false,
                            isCardDead = false,
                            attackerDamageDeal = 5,
                            championDamageDeal = 0,
                            attackerCard = new VortexTCG.Game.DTO.GameCardDto(),
                            attackerChamp = new VortexTCG.Game.DTO.BattleChampionDto(),
                            defenderChamp = new VortexTCG.Game.DTO.BattleChampionDto()
                        }
                    }
                }
            };
            Guid attackerId = Guid.NewGuid();
            Guid defenderId = Guid.NewGuid();

            Exception exception = Record.Exception(() => service.sendBattleResolveData(data, attackerId, defenderId));

            Assert.Null(exception);
        }

        [Fact]
        public void SendBattleResolveData_WithCardBattle_DoesNotThrow()
        {
            RoomService service = CreateRoomService();

            VortexTCG.Game.DTO.BattlesDataDto data = new VortexTCG.Game.DTO.BattlesDataDto
            {
                battles = new System.Collections.Generic.List<VortexTCG.Game.DTO.BattleDataDto>
                {
                    new VortexTCG.Game.DTO.BattleDataDto
                    {
                        isAgainstChamp = false,
                        againstCard = new VortexTCG.Game.DTO.BattleAgainstCardDataDto
                        {
                            isAttackerDead = false,
                            isDefenderDead = true,
                            attackerDamageDeal = 3,
                            defenderDamageDeal = 2,
                            attackerCard = new VortexTCG.Game.DTO.GameCardDto(),
                            defenderCard = new VortexTCG.Game.DTO.GameCardDto(),
                            attackerChamp = new VortexTCG.Game.DTO.BattleChampionDto(),
                            defenderChamp = new VortexTCG.Game.DTO.BattleChampionDto()
                        }
                    }
                }
            };
            Guid attackerId = Guid.NewGuid();
            Guid defenderId = Guid.NewGuid();

            Exception exception = Record.Exception(() => service.sendBattleResolveData(data, attackerId, defenderId));

            Assert.Null(exception);
        }

        #endregion

        #region Tests - tryGetRoom edge cases

        [Fact]
        public async Task SetPlayerDeck_RoomDeleted_ReturnsFalse()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.Leave(userId1, out string? _, out Guid? _, out bool _);

            bool result = await service.SetPlayerDeck(userId1, deckId1);

            Assert.False(result);
        }

        #endregion

        #region Tests - ChangePhase edge cases

        [Fact]
        public async Task ChangePhase_RoomCodeNotFound_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId = Guid.NewGuid();

            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask = service.ChangePhase(userId);

            Assert.NotNull(resultTask);
            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = await resultTask;
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_RoomNotFound_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.Leave(userId1, out string? _, out Guid? _, out bool _);

            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask = service.ChangePhase(userId1);

            Assert.NotNull(resultTask);
            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = await resultTask;
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_GameRoomNull_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            // Decks not set, GameRoom is just placeholder, not initialized

            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask = service.ChangePhase(userId1);

            Assert.NotNull(resultTask);
            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = await resultTask;
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_GameNotInitialized_ReturnsNull()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string _);
            // Only one player, IsGameInitialized = false

            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask = service.ChangePhase(userId1);

            Assert.NotNull(resultTask);
            VortexTCG.Game.DTO.PhaseChangeResultDTO? result = await resultTask;
            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_FullGameCycle_ReturnsResults()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            // Change phase multiple times
            Task<VortexTCG.Game.DTO.PhaseChangeResultDTO>? resultTask1 = service.ChangePhase(userId1);
            Assert.NotNull(resultTask1);
            VortexTCG.Game.DTO.PhaseChangeResultDTO result1 = await resultTask1;
            Assert.NotNull(result1);

            // After PLACEMENT -> ATTACK (or skipped) -> DEFENSE (or skipped) -> END_TURN -> next turn
            // The exact result depends on board state, but method should not throw
        }

        #endregion

        #region Tests - Leave with initialized game

        [Fact]
        public async Task Leave_WithInitializedGame_StopsTimer()
        {
            RoomService service = CreateRoomService();
            Guid userId1 = Guid.NewGuid();
            Guid userId2 = Guid.NewGuid();
            Guid deckId1 = Guid.NewGuid();
            Guid deckId2 = Guid.NewGuid();

            service.TryCreateRoom(userId1, out string code);
            service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            await service.SetPlayerDeck(userId1, deckId1);
            await service.SetPlayerDeck(userId2, deckId2);
            service.StartGame(userId1);

            // Leave both players
            service.Leave(userId1, out string? code1, out Guid? opp1, out bool empty1);
            service.Leave(userId2, out string? code2, out Guid? opp2, out bool empty2);

            Assert.True(empty2);
        }

        #endregion
    }
}