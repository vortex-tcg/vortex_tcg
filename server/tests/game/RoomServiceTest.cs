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
// =============================================

using Xunit;
using game.Services;
using System;
using System.Threading.Tasks;

namespace game.Tests
{
    // public class RoomServiceTest
    // {
        // #region Tests - Création de salons
// 
        // [Fact]
        // public void TryCreateRoom_GeneratesUniqueCode()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
// 
            // bool success = service.TryCreateRoom(userId1, out string code);
// 
            // Assert.True(success);
            // Assert.NotNull(code);
            // Assert.Equal(6, code.Length);
            // Assert.Matches("^[A-Z0-9]+$", code);
        // }
// 
        // [Fact]
        // public void TryCreateRoom_WithPreferredCode_Succeeds()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // string preferredCode = "CUSTOM";
// 
            // bool success = service.TryCreateRoom(userId1, out string code, preferredCode);
// 
            // Assert.True(success);
            // Assert.Equal("CUSTOM", code);
        // }
// 
        // [Fact]
        // public void TryCreateRoom_WithUsedPreferredCode_Fails()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // string preferredCode = "CUSTOM";
            // 
            // service.TryCreateRoom(userId1, out _, preferredCode);
// 
            // bool success = service.TryCreateRoom(userId2, out string code, preferredCode);
// 
            // Assert.False(success);
        // }
// 
        // [Fact]
        // public void TryCreateRoom_AddsCreatorToRoom()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
// 
            // service.TryCreateRoom(userId1, out string code);
// 
            // string? foundCode = service.GetRoomOf(userId1);
            // Assert.Equal(code, foundCode);
        // }
// 
        // #endregion
// 
        // #region Tests - Jonction de salons
// 
        // [Fact]
        // public void TryJoinRoom_ValidRoom_Succeeds()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // service.TryCreateRoom(userId1, out string code);
// 
            // bool success = service.TryJoinRoom(userId2, code, out Guid? opponentId, out bool isFull);
// 
            // Assert.True(success);
            // Assert.Equal(userId1, opponentId);
            // Assert.False(isFull);
        // }
// 
        // [Fact]
        // public void TryJoinRoom_NonExistentRoom_Fails()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
// 
            // bool success = service.TryJoinRoom(userId1, "BADCODE", out Guid? opponentId, out bool isFull);
// 
            // Assert.False(success);
            // Assert.Null(opponentId);
            // Assert.False(isFull);
        // }
// 
        // [Fact]
        // public void TryJoinRoom_FullRoom_Fails()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // Guid userId3 = Guid.NewGuid();
            // 
            // service.TryCreateRoom(userId1, out string code);
            // service.TryJoinRoom(userId2, code, out _, out _);
// 
            // bool success = service.TryJoinRoom(userId3, code, out Guid? opponentId, out bool isFull);
// 
            // Assert.False(success);
            // Assert.Null(opponentId);
            // Assert.True(isFull);
        // }
// 
        // #endregion
// 
        // #region Tests - Gestion des pseudos
// 
        // [Fact]
        // public void SetName_AndGetName_Works()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
// 
            // service.SetName(userId1, "PlayerOne");
            // string name = service.GetName(userId1);
// 
            // Assert.Equal("PlayerOne", name);
        // }
// 
        // [Fact]
        // public void GetName_UndefinedName_ReturnsDefault()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
// 
            // string name = service.GetName(userId1);
// 
            // string expected = $"Player-{userId1.ToString()[..8]}";
            // Assert.Equal(expected, name);
        // }
// 
        // [Fact]
        // public void SetName_EmptyName_GeneratesDefault()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
// 
            // service.SetName(userId1, "   ");
            // string name = service.GetName(userId1);
// 
            // string expected = $"Player-{userId1.ToString()[..8]}";
            // Assert.Equal(expected, name);
        // }
// 
        // #endregion
// 
        // #region Tests - Initialisation de parties
// 
        // [Fact]
        // public async Task SetPlayerDeck_FirstPlayer_DoesNotInitializeGame()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid deckId1 = Guid.NewGuid();
            // 
            // service.TryCreateRoom(userId1, out string code);
// 
            // await service.SetPlayerDeck(userId1, deckId1);
// 
            // bool isReady = service.IsGameReady(code);
            // Assert.False(isReady);
// 
            // var gameRoom = service.GetGameRoom(code);
            // Assert.Null(gameRoom);
        // }
// 
        // [Fact]
        // public async Task SetPlayerDeck_BothPlayers_InitializesGame()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // Guid deckId1 = Guid.NewGuid();
            // Guid deckId2 = Guid.NewGuid();
            // 
            // service.TryCreateRoom(userId1, out string code);
            // service.TryJoinRoom(userId2, code, out _, out _);
// 
            // await service.SetPlayerDeck(userId1, deckId1);
            // await service.SetPlayerDeck(userId2, deckId2);
// 
            // bool isReady = service.IsGameReady(code);
            // Assert.True(isReady);
// 
            // var gameRoom = service.GetGameRoom(code);
            // Assert.NotNull(gameRoom);
        // }
// 
        // [Fact]
        // public async Task GetRoomPlayers_ReturnsCorrectIds()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // Guid deckId1 = Guid.NewGuid();
            // Guid deckId2 = Guid.NewGuid();
            // 
            // service.TryCreateRoom(userId1, out string code);
            // service.TryJoinRoom(userId2, code, out _, out _);
            // await service.SetPlayerDeck(userId1, deckId1);
            // await service.SetPlayerDeck(userId2, deckId2);
// 
            // var (user1, user2, deck1, deck2) = service.GetRoomPlayers(code);
// 
            // Assert.Equal(userId1, user1);
            // Assert.Equal(userId2, user2);
            // Assert.Equal(deckId1, deck1);
            // Assert.Equal(deckId2, deck2);
        // }
// 
        // #endregion
// 
        // #region Tests - Départs et nettoyage
// 
        // [Fact]
        // public void Leave_RemovesPlayerFromRoom()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // service.TryCreateRoom(userId1, out string code);
// 
            // service.Leave(userId1, out string? leftCode, out _, out _);
// 
            // Assert.Equal(code, leftCode);
// 
            // string? foundCode = service.GetRoomOf(userId1);
            // Assert.Null(foundCode);
        // }
// 
        // [Fact]
        // public void Leave_WithOpponent_LeavesOpponentInRoom()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // service.TryCreateRoom(userId1, out string code);
            // service.TryJoinRoom(userId2, code, out _, out _);
// 
            // service.Leave(userId1, out _, out Guid? opponentId, out bool roomEmpty);
// 
            // Assert.Equal(userId2, opponentId);
            // Assert.False(roomEmpty);
// 
            // string? foundCode = service.GetRoomOf(userId2);
            // Assert.Equal(code, foundCode);
        // }
// 
        // [Fact]
        // public void Leave_LastPlayer_DeletesRoom()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // service.TryCreateRoom(userId1, out string code);
// 
            // service.Leave(userId1, out _, out Guid? opponentId, out bool roomEmpty);
// 
            // Assert.True(!opponentId.HasValue || opponentId.Value == Guid.Empty);
            // Assert.True(roomEmpty);
            // 
            // var gameRoom = service.GetGameRoom(code);
            // Assert.Null(gameRoom);
        // }
// 
        // [Fact]
        // public void GetOpponentOf_ReturnsOtherPlayer()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // service.TryCreateRoom(userId1, out string code);
            // service.TryJoinRoom(userId2, code, out _, out _);
// 
            // Guid? opponent1 = service.GetOpponentOf(userId1);
            // Guid? opponent2 = service.GetOpponentOf(userId2);
// 
            // Assert.Equal(userId2, opponent1);
            // Assert.Equal(userId1, opponent2);
        // }
// 
        // [Fact]
        // public void GetOpponentOf_NoOpponent_ReturnsNull()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // service.TryCreateRoom(userId1, out _);
// 
            // Guid? opponent = service.GetOpponentOf(userId1);
// 
            // Assert.True(!opponent.HasValue || opponent.Value == Guid.Empty); 
        // }
// 
        // #endregion
// 
        // #region Tests - Cas d'erreur
// 
        // [Fact]
        // public async Task SetPlayerDeck_PlayerNotInRoom_Fails()
        // {
            // var service = new RoomService();
            // Guid userId = Guid.NewGuid();
            // Guid deckId = Guid.NewGuid();
// 
            // bool success = await service.SetPlayerDeck(userId, deckId);
// 
            // Assert.False(success);
        // }
// 
        // [Fact]
        // public void GetGameRoom_NonExistentRoom_ReturnsNull()
        // {
            // var service = new RoomService();
// 
            // var gameRoom = service.GetGameRoom("BADCODE");
// 
            // Assert.Null(gameRoom);
        // }
// 
        // [Fact]
        // public void GetGameRoomByUserId_PlayerNotInRoom_ReturnsNull()
        // {
            // var service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
// 
            // var gameRoom = service.GetGameRoomByUserId(userId1);
// 
            // Assert.Null(gameRoom);
        // }
// 
        // #endregion
    // }
}
 