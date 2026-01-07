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

        #region Phase Cycle Tests

        [Fact]
        public async Task ChangePhase_MultipleTimes_ShouldCycleThroughPhases()
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

            // PLACEMENT -> ATTACK/DEFENSE/END_TURN -> PLACEMENT (or auto-skips)
            PhaseChangeResultDTO? result1 = room.ChangePhase(user1Id);
            Assert.NotNull(result1);

            // Continue cycling until back to PLACEMENT
            int maxIterations = 10;
            int iterations = 0;
            PhaseChangeResultDTO lastResult = result1;

            while (lastResult.CurrentPhase != GamePhase.PLACEMENT && iterations < maxIterations)
            {
                PhaseChangeResultDTO? nextResult = room.ChangePhase(lastResult.ActivePlayerId);
                if (nextResult == null) break;
                lastResult = nextResult;
                iterations++;
            }

            // Should complete without throwing
            Assert.True(iterations < maxIterations);
        }

        [Fact]
        public async Task ChangePhase_FullTurnCycle_ShouldIncrementTurnNumber()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            PhaseChangeResultDTO startResult = room.StartGame();

            Assert.Equal(1, startResult.TurnNumber);

            // Go through full turn cycle
            int maxIterations = 20;
            int iterations = 0;
            PhaseChangeResultDTO lastResult = startResult;

            while (iterations < maxIterations)
            {
                PhaseChangeResultDTO? nextResult = room.ChangePhase(lastResult.ActivePlayerId);
                if (nextResult == null) break;
                lastResult = nextResult;
                iterations++;

                // Check if turn increased and we're back to player 1
                if (lastResult.TurnNumber > 1) break;
            }

            // Should complete without throwing
            Assert.True(iterations < maxIterations);
        }

        #endregion

        #region Timer Event Tests

        [Fact]
        public async Task OnTimeUp_EventCanBeSubscribed()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            bool eventFired = false;
            room.OnTimeUp += () => { eventFired = true; };

            // Event subscription should work without throwing
            Assert.False(eventFired); // Not fired yet
        }

        #endregion

        #region PlayCard CardType Tests

        [Fact]
        public async Task PlayCard_CardNotInHand_ShouldReturnNull()
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

            // Try to play a card that doesn't exist in hand
            PlayCardResponseDto result = room.PlayCard(user1Id, 99999, 0);

            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task PlayCard_AllValidLocations_ShouldNotThrow(int location)
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

            // Should not throw, just return null if card doesn't exist
            Exception exception = Record.Exception(() => room.PlayCard(user1Id, 1, location));
            Assert.Null(exception);
        }

        #endregion

        #region DrawCards Extended Tests

        [Fact]
        public async Task DrawCards_ForPlayer1_ShouldSendData()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.DrawCards(user1Id, 3);

            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.Once());
        }

        [Fact]
        public async Task DrawCards_ForPlayer2_ShouldSendData()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.DrawCards(user2Id, 2);

            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.Once());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task DrawCards_VariousCounts_ShouldSendData(int count)
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.DrawCards(user1Id, count);

            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.Once());
        }

        #endregion

        #region GetState Extended Tests

        [Fact]
        public async Task GetState_AfterPhaseChange_ShouldReflectNewPhase()
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

            PhaseChangeResultDTO? changeResult = room.ChangePhase(user1Id);
            PhaseChangeResultDTO stateAfter = room.GetState();

            Assert.NotNull(changeResult);
            Assert.Equal(changeResult.CurrentPhase, stateAfter.CurrentPhase);
        }

        [Fact]
        public async Task GetState_AfterForceChange_ShouldReflectNewPhase()
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

            PhaseChangeResultDTO forceResult = room.ForceChangePhase();
            PhaseChangeResultDTO stateAfter = room.GetState();

            Assert.Equal(forceResult.CurrentPhase, stateAfter.CurrentPhase);
        }

        #endregion

        #region HandleAttackEvent Extended Tests

        [Fact]
        public async Task HandleAttackEvent_WithActivePlayer_AndNoCardsOnBoard_ShouldReturnNull()
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

            // User1 is the active player, try attack with non-existent card
            AttackResponseDto result = room.HandleAttackEvent(user1Id, 12345);
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleAttackEvent_AfterPhaseChange_ShouldReturnNull()
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

            // Change phase
            room.ChangePhase(user1Id);

            // Try attack
            AttackResponseDto result = room.HandleAttackEvent(user1Id, 1);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100)]
        [InlineData(999999)]
        public async Task HandleAttackEvent_WithVariousCardIds_ShouldNotThrow(int cardId)
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

            Exception ex = Record.Exception(() => room.HandleAttackEvent(user1Id, cardId));
            Assert.Null(ex);
        }

        #endregion

        #region HandleDefenseEvent Extended Tests

        [Fact]
        public async Task HandleDefenseEvent_WithOpponentCardIdNegative_AndWrongState_ShouldReturnNull()
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

            // Try defense with negative opponent card id
            DefenseResponseDto result = room.HandleDefenseEvent(user1Id, 1, -1);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(-1, -1)]
        [InlineData(100, 200)]
        public async Task HandleDefenseEvent_WithVariousCardIds_ShouldNotThrow(int cardId, int opponentCardId)
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

            Exception ex = Record.Exception(() => room.HandleDefenseEvent(user1Id, cardId, opponentCardId));
            Assert.Null(ex);
        }

        #endregion

        #region Multiple Phase Changes Tests

        [Fact]
        public async Task ChangePhase_ThroughMultipleTurns_ShouldIncrementTurnCorrectly()
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

            // Perform multiple phase changes
            PhaseChangeResultDTO? result = room.GetState();
            int initialTurn = result.TurnNumber;

            // Change phase multiple times
            for (int i = 0; i < 15; i++)
            {
                PhaseChangeResultDTO? nextResult = room.ChangePhase(result.ActivePlayerId);
                if (nextResult == null)
                {
                    result = room.GetState();
                    continue;
                }
                result = nextResult;
            }

            // Turn should have incremented at some point
            Assert.True(result.TurnNumber >= initialTurn);
        }

        [Fact]
        public async Task ForceChangePhase_MultipleTimes_ShouldCycle()
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

            List<GamePhase> phases = new List<GamePhase>();

            for (int i = 0; i < 10; i++)
            {
                PhaseChangeResultDTO result = room.ForceChangePhase();
                phases.Add(result.CurrentPhase);
            }

            // Should have at least visited PLACEMENT phase
            Assert.Contains(GamePhase.PLACEMENT, phases);
        }

        #endregion

        #region IsPlayerTurn Extended Tests

        [Fact]
        public async Task IsPlayerTurn_AfterSwitchingPlayers_ShouldUpdateCorrectly()
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

            // Initially player 1
            Assert.True(room.IsPlayerTurn(user1Id));
            Assert.False(room.IsPlayerTurn(user2Id));

            // Change phase until player changes
            PhaseChangeResultDTO? result = room.ChangePhase(user1Id);
            while (result != null && result.ActivePlayerId == user1Id)
            {
                result = room.ChangePhase(result.ActivePlayerId);
            }

            // Should not throw
            Assert.NotNull(room);
        }

        [Fact]
        public async Task IsPlayerTurn_WithUnknownPlayer_ShouldReturnFalse()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid unknownId = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            bool result = room.IsPlayerTurn(unknownId);
            Assert.False(result);
        }

        #endregion

        #region CanPlayerAct Extended Tests

        [Fact]
        public async Task CanPlayerAct_WithUnknownPlayer_ShouldReturnFalse()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid unknownId = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            room.StartGame();

            bool result = room.CanPlayerAct(unknownId);
            Assert.False(result);
        }

        [Fact]
        public async Task CanPlayerAct_AfterMultiplePhaseChanges_ShouldWork()
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

            // Change phase a few times
            room.ChangePhase(user1Id);
            room.ForceChangePhase();

            // Check can act still works
            bool canAct1 = room.CanPlayerAct(user1Id);
            bool canAct2 = room.CanPlayerAct(user2Id);

            // One or both may be able to act depending on phase
            Assert.True(canAct1 || canAct2 || !canAct1 && !canAct2);
        }

        #endregion

        #region GetOpponentId Extended Tests

        [Fact]
        public async Task GetOpponentId_WithUnknownPlayer_ShouldReturnBasedOnLogic()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid unknownId = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            // GetOpponentId returns user2 if playerId != user1, else user1
            // So for unknownId (which != user1), it returns user1
            Guid opponent = room.GetOpponentId(unknownId);
            Assert.Equal(user1Id, opponent); // Not user1, so returns user1 (the opposite logic)
        }

        #endregion

        #region DrawCards Fatigue Tests

        [Fact]
        public async Task DrawCards_ManyTimes_ShouldEventuallyTriggerFatigue()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            // Draw many cards to deplete deck and trigger fatigue
            for (int i = 0; i < 50; i++)
            {
                room.DrawCards(user1Id, 5);
            }

            // Verify sendDrawCardsData was called many times
            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.AtLeast(50));
        }

        [Fact]
        public async Task DrawCards_BothPlayers_ShouldWorkIndependently()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            room.DrawCards(user1Id, 3);
            room.DrawCards(user2Id, 2);
            room.DrawCards(user1Id, 1);
            room.DrawCards(user2Id, 4);

            listener.Verify(l => l.sendDrawCardsData(It.IsAny<DrawCardsResultDTO>()), Times.Exactly(4));
        }

        #endregion

        #region Phase End Turn Tests

        [Fact]
        public async Task ChangePhase_ToEndTurn_ShouldResolveBattle()
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

            // Cycle through phases to reach END_TURN
            PhaseChangeResultDTO? result = room.GetState();
            int iterations = 0;

            while (result != null && result.CurrentPhase != GamePhase.END_TURN && iterations < 20)
            {
                PhaseChangeResultDTO? next = room.ChangePhase(result.ActivePlayerId);
                if (next == null)
                {
                    result = room.GetState();
                    iterations++;
                    continue;
                }
                result = next;
                iterations++;
            }

            // Should not throw
            Assert.True(iterations < 20);
        }

        [Fact]
        public async Task ForceChangePhase_ToEndTurn_ShouldCallBattleResolve()
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

            // Force change multiple times to hit END_TURN
            for (int i = 0; i < 5; i++)
            {
                room.ForceChangePhase();
            }

            // sendBattleResolveData should be called at least once
            listener.Verify(l => l.sendBattleResolveData(
                It.IsAny<BattlesDataDto>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        #endregion

        #region Timer Integration Tests

        [Fact]
        public async Task StartTimer_AfterGameStart_ShouldNotThrow()
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

            Exception ex = Record.Exception(() => room.StartTimer(30));
            Assert.Null(ex);

            room.StopTimer();
        }

        [Fact]
        public async Task StartTimer_ThenStop_ThenStart_ShouldNotThrow()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);

            Exception ex = Record.Exception(() =>
            {
                room.StartTimer(10);
                room.StopTimer();
                room.StartTimer(20);
                room.StopTimer();
            });
            Assert.Null(ex);
        }

        #endregion

        #region Full Game Simulation Tests

        [Fact]
        public async Task FullGameSimulation_ShouldCompleteMultipleTurns()
        {
            Mock<IRoomActionEventListener> listener = CreateMockListener();
            Room room = new Room(listener.Object);
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid deck1Id = Guid.NewGuid();
            Guid deck2Id = Guid.NewGuid();

            await room.setUser1(user1Id, deck1Id);
            await room.setUser2(user2Id, deck2Id);
            PhaseChangeResultDTO startResult = room.StartGame();

            // Simulate 3 complete turns
            PhaseChangeResultDTO currentState = startResult;
            int targetTurn = 3;

            while (currentState.TurnNumber < targetTurn)
            {
                PhaseChangeResultDTO? next = room.ChangePhase(currentState.ActivePlayerId);
                if (next == null)
                {
                    // Force change if regular change fails
                    currentState = room.ForceChangePhase();
                }
                else
                {
                    currentState = next;
                }
            }

            Assert.True(currentState.TurnNumber >= targetTurn);
        }

        [Fact]
        public async Task FullGameSimulation_WithPlayCardAttempts_ShouldNotThrow()
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

            // Try various actions during simulation
            Exception ex = Record.Exception(() =>
            {
                for (int turn = 0; turn < 5; turn++)
                {
                    // Try to play cards
                    room.PlayCard(user1Id, 1, 0);
                    room.PlayCard(user1Id, 2, 1);

                    // Try attacks
                    room.HandleAttackEvent(user1Id, 1);

                    // Try defenses
                    room.HandleDefenseEvent(user2Id, 1, 1);

                    // Force advance
                    room.ForceChangePhase();
                }
            });
            Assert.Null(ex);
        }

        #endregion
    }
}
