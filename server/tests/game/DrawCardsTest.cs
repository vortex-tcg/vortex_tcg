using Xunit;
using game.Services;
using VortexTCG.Game.DTO;
using System;
using System.Threading.Tasks;

namespace game.Tests
{
    // public class DrawCardsTest
    // {
        // private async Task<(RoomService service, Guid userId1, Guid userId2, string code)> SetupGameAsync()
        // {
            // RoomService service = new RoomService();
            // Guid userId1 = Guid.NewGuid();
            // Guid userId2 = Guid.NewGuid();
            // Guid deckId1 = Guid.NewGuid();
            // Guid deckId2 = Guid.NewGuid();
// 
            // service.TryCreateRoom(userId1, out string code);
            // service.TryJoinRoom(userId2, code, out Guid? _, out bool _);
            // await service.SetPlayerDeck(userId1, deckId1);
            // await service.SetPlayerDeck(userId2, deckId2);
// 
            // return (service, userId1, userId2, code);
        // }
// 
        // [Fact]
        // public async Task DrawCards_OneCard_ReturnsCardForPlayer()
        // {
            // (RoomService service, Guid userId1, Guid _, string code) = await SetupGameAsync();
            // VortexTCG.Game.Object.Room? gameRoom = service.GetGameRoom(code);
// 
            // DrawCardsResultDTO? result = gameRoom?.DrawCards(userId1, 1);
// 
            // Assert.NotNull(result);
            // Assert.Single(result.PlayerResult.DrawnCards);
            // Assert.Equal(0, result.PlayerResult.FatigueCount);
        // }
// 
        // [Fact]
        // public async Task DrawCards_MultipleCards_ReturnsAllCards()
        // {
            // (RoomService service, Guid userId1, Guid _, string code) = await SetupGameAsync();
            // VortexTCG.Game.Object.Room? gameRoom = service.GetGameRoom(code);
// 
            // DrawCardsResultDTO? result = gameRoom?.DrawCards(userId1, 5);
// 
            // Assert.NotNull(result);
            // Assert.Equal(5, result.PlayerResult.DrawnCards.Count);
        // }
// 
        // [Fact]
        // public async Task DrawCards_OpponentReceivesCountOnly()
        // {
            // (RoomService service, Guid userId1, Guid _, string code) = await SetupGameAsync();
            // VortexTCG.Game.Object.Room? gameRoom = service.GetGameRoom(code);
// 
            // DrawCardsResultDTO? result = gameRoom?.DrawCards(userId1, 3);
// 
            // Assert.NotNull(result);
            // Assert.Equal(userId1, result.OpponentResult.PlayerId);
            // Assert.Equal(3, result.OpponentResult.CardsDrawnCount);
        // }
// 
        // [Fact]
        // public async Task DrawCards_EmptyDeck_CausesFatigue()
        // {
            // (RoomService service, Guid userId1, Guid _, string code) = await SetupGameAsync();
            // VortexTCG.Game.Object.Room? gameRoom = service.GetGameRoom(code);
// 
            // gameRoom?.DrawCards(userId1, 30);
            // DrawCardsResultDTO? result = gameRoom?.DrawCards(userId1, 1);
// 
            // Assert.NotNull(result);
            // Assert.Empty(result.PlayerResult.DrawnCards);
            // Assert.Equal(1, result.PlayerResult.FatigueCount);
            // Assert.Equal(1, result.OpponentResult.FatigueCount);
        // }
// 
        // [Fact]
        // public async Task DrawCards_PartialDeck_DrawsAndFatigue()
        // {
            // (RoomService service, Guid userId1, Guid _, string code) = await SetupGameAsync();
            // VortexTCG.Game.Object.Room? gameRoom = service.GetGameRoom(code);
// 
            // gameRoom?.DrawCards(userId1, 28);
            // DrawCardsResultDTO? result = gameRoom?.DrawCards(userId1, 5);
// 
            // Assert.NotNull(result);
            // Assert.Equal(2, result.PlayerResult.DrawnCards.Count);
            // Assert.Equal(3, result.PlayerResult.FatigueCount);
        // }
// 
        // [Fact]
        // public async Task DrawCards_InvalidPlayer_ReturnsNull()
        // {
            // (RoomService service, Guid _, Guid _, string code) = await SetupGameAsync();
            // VortexTCG.Game.Object.Room? gameRoom = service.GetGameRoom(code);
            // Guid unknownUser = Guid.NewGuid();
// 
            // DrawCardsResultDTO? result = gameRoom?.DrawCards(unknownUser, 1);
// 
            // Assert.Null(result);
        // }
// 
        // [Fact]
        // public async Task DrawCards_ZeroCards_ReturnsNull()
        // {
            // (RoomService service, Guid userId1, Guid _, string code) = await SetupGameAsync();
            // VortexTCG.Game.Object.Room? gameRoom = service.GetGameRoom(code);
// 
            // DrawCardsResultDTO? result = gameRoom?.DrawCards(userId1, 0);
// 
            // Assert.Null(result);
        // }
    // }
}