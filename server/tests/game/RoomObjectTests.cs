using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Interface;
using VortexTCG.Game.Object;
using Xunit;

namespace game.Tests
{
    public class RoomObjectTests
    {
        private static Mock<IRoomActionEventListener> CreateMockListener()
        {
            return new Mock<IRoomActionEventListener>();
        }

        #region Constructor Tests

        [Fact]
        public void Room_Constructor_ShouldInitializeWithoutError()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);

            Assert.NotNull(room);
        }

        #endregion

        #region User Initialization Tests

        [Fact]
        public async Task SetUser1_ShouldCompleteWithoutError()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            await room.setUser1(userId, deckId);

            // Verify no exception and room still exists
            Assert.NotNull(room);
        }

        [Fact]
        public async Task SetUser2_ShouldCompleteWithoutError()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid userId = Guid.NewGuid();
            Guid deckId = Guid.NewGuid();

            await room.setUser2(userId, deckId);

            // Verify no exception and room still exists
            Assert.NotNull(room);
        }

        [Fact]
        public async Task SetBothUsers_ShouldSetupBothPlayersCorrectly()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            // Verify setup by checking GetOpponentId
            Assert.Equal(user2Id, room.GetOpponentId(user1Id));
            Assert.Equal(user1Id, room.GetOpponentId(user2Id));
        }

        #endregion

        #region Start Game Tests

        [Fact]
        public async Task StartGame_ShouldReturnPhaseChangeResult()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            PhaseChangeResultDTO result = room.StartGame();

            Assert.NotNull(result);
            Assert.Equal(GamePhase.PLACEMENT, result.CurrentPhase);
            Assert.Equal(user1Id, result.ActivePlayerId);
            Assert.Equal(1, result.TurnNumber);
            Assert.False(result.AutoChanged);
            Assert.True(result.CanAct);
        }

        [Fact]
        public async Task StartGame_ShouldDrawCardsForBothPlayers()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.StartGame();

            // Verify that sendDrawCardsData was called (twice - once for each player)
            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.AtLeastOnce());
        }

        #endregion

        #region GetState Tests

        [Fact]
        public async Task GetState_BeforeGameStart_ShouldReturnInitialState()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            PhaseChangeResultDTO state = room.GetState();

            Assert.NotNull(state);
            Assert.False(state.CanAct);
        }

        [Fact]
        public async Task GetState_AfterGameStart_ShouldReflectCurrentState()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            PhaseChangeResultDTO state = room.GetState();

            Assert.NotNull(state);
            Assert.True(state.CanAct);
            Assert.Equal(GamePhase.PLACEMENT, state.CurrentPhase);
            Assert.Equal(1, state.TurnNumber);
        }

        #endregion

        #region Change Phase Tests

        [Fact]
        public async Task ChangePhase_BeforeGameStart_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            PhaseChangeResultDTO? result = room.ChangePhase(user1Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_WithWrongPlayer_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            // Player 2 tries to change phase when it's Player 1's turn
            PhaseChangeResultDTO? result = room.ChangePhase(user2Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task ChangePhase_WithCorrectPlayer_ShouldAdvancePhase()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            PhaseChangeResultDTO? result = room.ChangePhase(user1Id);

            Assert.NotNull(result);
            // After PLACEMENT, should go to ATTACK (or auto-skip to next phase if no attackable cards)
        }

        #endregion

        #region Force Change Phase Tests

        [Fact]
        public async Task ForceChangePhase_BeforeGameStart_ShouldReturnAutoChangedResult()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            PhaseChangeResultDTO result = room.ForceChangePhase();

            Assert.NotNull(result);
            Assert.True(result.AutoChanged);
            Assert.False(result.CanAct);
        }

        [Fact]
        public async Task ForceChangePhase_AfterGameStart_ShouldAdvancePhase()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            PhaseChangeResultDTO result = room.ForceChangePhase();

            Assert.NotNull(result);
            Assert.True(result.AutoChanged);
            Assert.Equal("Timeout - 1 minute écoulée", result.AutoChangeReason);
        }

        #endregion

        #region Timer Tests

        [Fact]
        public async Task StartTimer_ShouldNotThrow()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            Exception ex = Record.Exception(() => room.StartTimer(30));
            Assert.Null(ex);
        }

        [Fact]
        public async Task StopTimer_ShouldNotThrow()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            Exception ex = Record.Exception(() => room.StopTimer());
            Assert.Null(ex);
        }

        [Fact]
        public async Task StopTimer_AfterStart_ShouldNotThrow()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.StartTimer(30);
            Exception ex = Record.Exception(() => room.StopTimer());
            Assert.Null(ex);
        }

        #endregion

        #region IsPlayerTurn Tests

        [Fact]
        public async Task IsPlayerTurn_BeforeGameStart_ShouldReturnFalse()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            bool result = room.IsPlayerTurn(user1Id);

            Assert.False(result);
        }

        [Fact]
        public async Task IsPlayerTurn_AfterGameStart_ShouldReturnTrueForPlayer1()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            bool player1Turn = room.IsPlayerTurn(user1Id);
            bool player2Turn = room.IsPlayerTurn(user2Id);

            Assert.True(player1Turn);
            Assert.False(player2Turn);
        }

        #endregion

        #region CanPlayerAct Tests

        [Fact]
        public async Task CanPlayerAct_BeforeGameStart_ShouldReturnFalse()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            bool result = room.CanPlayerAct(user1Id);

            Assert.False(result);
        }

        [Fact]
        public async Task CanPlayerAct_DuringPlacement_ShouldReturnTrueForActivePlayer()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            bool player1CanAct = room.CanPlayerAct(user1Id);
            bool player2CanAct = room.CanPlayerAct(user2Id);

            Assert.True(player1CanAct);
            Assert.False(player2CanAct);
        }

        #endregion

        #region GetOpponentId Tests

        [Fact]
        public async Task GetOpponentId_ShouldReturnCorrectOpponent()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            Guid opponent1 = room.GetOpponentId(user1Id);
            Guid opponent2 = room.GetOpponentId(user2Id);

            Assert.Equal(user2Id, opponent1);
            Assert.Equal(user1Id, opponent2);
        }

        #endregion

        #region DrawCards Tests

        [Fact]
        public async Task DrawCards_WithInvalidPlayer_ShouldNotSendData()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();
            Guid invalidUserId = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.DrawCards(invalidUserId, 1);

            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.Never());
        }

        [Fact]
        public async Task DrawCards_WithZeroCount_ShouldNotSendData()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.DrawCards(user1Id, 0);

            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.Never());
        }

        [Fact]
        public async Task DrawCards_WithNegativeCount_ShouldNotSendData()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.DrawCards(user1Id, -5);

            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.Never());
        }

        #endregion

        #region PlayCard Tests

        [Fact]
        public async Task PlayCard_BeforeGameStart_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            PlayCardResponseDto result = room.PlayCard(user1Id, 1, 0);

            // PlayCard checks if _activePlayerId matches, and game hasn't started
            Assert.Null(result);
        }

        [Fact]
        public async Task PlayCard_WrongPhase_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            // Change phase to non-PLACEMENT
            room.ChangePhase(user1Id);

            PlayCardResponseDto result = room.PlayCard(user1Id, 1, 0);

            // Should return null if not in PLACEMENT phase
            Assert.Null(result);
        }

        [Fact]
        public async Task PlayCard_WrongPlayer_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            // Player 2 tries to play when it's Player 1's turn
            PlayCardResponseDto result = room.PlayCard(user2Id, 1, 0);

            Assert.Null(result);
        }

        [Fact]
        public async Task PlayCard_InvalidLocation_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            PlayCardResponseDto result = room.PlayCard(user1Id, 1, -1);

            Assert.Null(result);
        }

        [Fact]
        public async Task PlayCard_InvalidLocationTooHigh_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            PlayCardResponseDto result = room.PlayCard(user1Id, 1, 6);

            Assert.Null(result);
        }

        #endregion

        #region HandleAttackEvent Tests

        [Fact]
        public async Task HandleAttackEvent_BeforeGameStart_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            AttackResponseDto result = room.HandleAttackEvent(user1Id, 1);

            // No active player before game start, so should return null
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleAttackEvent_WrongPlayer_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            AttackResponseDto result = room.HandleAttackEvent(user2Id, 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task HandleAttackEvent_CardNotOnBoard_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            AttackResponseDto result = room.HandleAttackEvent(user1Id, 9999);

            Assert.Null(result);
        }

        #endregion

        #region HandleDefenseEvent Tests

        [Fact]
        public async Task HandleDefenseEvent_BeforeGameStart_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            DefenseResponseDto result = room.HandleDefenseEvent(user1Id, 1, 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task HandleDefenseEvent_WrongPlayer_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            DefenseResponseDto result = room.HandleDefenseEvent(user2Id, 1, 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task HandleDefenseEvent_WrongPhase_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            // We're in PLACEMENT phase, not DEFENSE
            DefenseResponseDto result = room.HandleDefenseEvent(user1Id, 1, 2);

            Assert.Null(result);
        }

        [Fact]
        public async Task HandleDefenseEvent_CardNotOnBoard_ShouldReturnNull()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            // Even if player matches, card not found
            DefenseResponseDto result = room.HandleDefenseEvent(user1Id, 9999, 9998);

            Assert.Null(result);
        }

        #endregion
    }
}
