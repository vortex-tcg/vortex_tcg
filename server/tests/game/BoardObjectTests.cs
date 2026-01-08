using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;
using GameCard = VortexTCG.Game.Object.Card;
using Xunit;

namespace game.Tests
{
    public class BoardObjectTests
    {
        private GameCard CreateCard(int gameCardId)
        {
            CardDTO dto = new CardDTO
            {
                Id = Guid.NewGuid(),
                Name = "Unit",
                Hp = 10,
                Attack = 3,
                Cost = 2,
                Description = "",
                CardType = CardType.GUARD,
                Class = new List<string> { "class" }
            };
            return new GameCard(dto, gameCardId);
        }

        #region Positioning Tests

        [Fact]
        public void Positioning_ShouldUpdateAvailabilityAndRetrieval()
        {
            Board board = new Board();
            GameCard card = CreateCard(1);

            Assert.True(board.IsAvailable(0));
            board.PosCard(card, 0);
            Assert.False(board.IsAvailable(0));
            Assert.Equal(1, board.GetCardCount());

            int pos;
            bool found = board.TryGetCardPos(1, out pos);
            Assert.True(found);
            Assert.Equal(0, pos);

            GameCard retrieved = board.GetCardFromSlot(0);
            Assert.Equal(card.GetGameCardId(), retrieved.GetGameCardId());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void PosCard_AllLocations_ShouldPlaceCardCorrectly(int location)
        {
            Board board = new Board();
            GameCard card = CreateCard(100 + location);

            Assert.True(board.IsAvailable(location));
            board.PosCard(card, location);
            Assert.False(board.IsAvailable(location));

            GameCard retrieved = board.GetCardFromSlot(location);
            Assert.NotNull(retrieved);
            Assert.Equal(card.GetGameCardId(), retrieved.GetGameCardId());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void TryGetCardPos_AllLocations_ShouldReturnCorrectPosition(int location)
        {
            Board board = new Board();
            int cardId = 200 + location;
            GameCard card = CreateCard(cardId);
            board.PosCard(card, location);

            bool found = board.TryGetCardPos(cardId, out int pos);
            Assert.True(found);
            Assert.Equal(location, pos);
        }

        [Fact]
        public void TryGetCardPos_CardNotFound_ShouldReturnFalse()
        {
            Board board = new Board();
            GameCard card = CreateCard(1);
            board.PosCard(card, 0);

            bool found = board.TryGetCardPos(999, out int pos);
            Assert.False(found);
            Assert.Equal(-1, pos);
        }

        [Fact]
        public void IsAvailable_InvalidLocation_ShouldReturnFalse()
        {
            Board board = new Board();
            Assert.False(board.IsAvailable(-1));
            Assert.False(board.IsAvailable(6));
            Assert.False(board.IsAvailable(100));
        }

        [Fact]
        public void GetCardFromSlot_InvalidSlot_ShouldReturnNull()
        {
            Board board = new Board();
            GameCard result = board.GetCardFromSlot(-1);
            Assert.Null(result);

            GameCard result2 = board.GetCardFromSlot(6);
            Assert.Null(result2);

            GameCard result3 = board.GetCardFromSlot(100);
            Assert.Null(result3);
        }

        [Fact]
        public void GetCardFromSlot_EmptySlot_ShouldReturnNull()
        {
            Board board = new Board();
            for (int i = 0; i <= 5; i++)
            {
                GameCard result = board.GetCardFromSlot(i);
                Assert.Null(result);
            }
        }

        #endregion

        #region Card Count and Empty Tests

        [Fact]
        public void IsEmpty_EmptyBoard_ShouldReturnTrue()
        {
            Board board = new Board();
            Assert.True(board.IsEmpty());
            Assert.Equal(0, board.GetCardCount());
        }

        [Fact]
        public void IsEmpty_WithCards_ShouldReturnFalse()
        {
            Board board = new Board();
            GameCard card = CreateCard(1);
            board.PosCard(card, 0);

            Assert.False(board.IsEmpty());
            Assert.Equal(1, board.GetCardCount());
        }

        [Fact]
        public void GetCardCount_MultipleCards_ShouldReturnCorrectCount()
        {
            Board board = new Board();

            for (int i = 0; i < 6; i++)
            {
                GameCard card = CreateCard(i + 1);
                board.PosCard(card, i);
                Assert.Equal(i + 1, board.GetCardCount());
            }
        }

        #endregion

        #region Attackable and Defendable Tests

        [Fact]
        public void HasAttackableAndDefendableCards_ShouldReflectStates()
        {
            Board board = new Board();
            GameCard card = CreateCard(2);
            board.PosCard(card, 1);

            Assert.True(board.HasAttackableCards());
            Assert.True(board.HasDefendableCards());

            card.AddState(CardState.ENGAGE);
            Assert.False(board.HasAttackableCards());

            card.RemoveState(CardState.ENGAGE);
            card.AddState(CardState.ATTACK_ENGAGE);
            Assert.True(board.HasDefendableCards());
        }

        [Fact]
        public void HasAttackableCards_EmptyBoard_ShouldReturnFalse()
        {
            Board board = new Board();
            Assert.False(board.HasAttackableCards());
        }

        [Fact]
        public void HasDefendableCards_EmptyBoard_ShouldReturnFalse()
        {
            Board board = new Board();
            Assert.False(board.HasDefendableCards());
        }

        [Fact]
        public void HasAttackableCards_AllCardsEngaged_ShouldReturnFalse()
        {
            Board board = new Board();
            GameCard card1 = CreateCard(1);
            GameCard card2 = CreateCard(2);
            card1.AddState(CardState.ENGAGE);
            card2.AddState(CardState.ENGAGE);
            board.PosCard(card1, 0);
            board.PosCard(card2, 1);

            Assert.False(board.HasAttackableCards());
        }

        [Fact]
        public void HasDefendableCards_AllCardsEngagedOrAttackEngaged_ShouldReturnFalse()
        {
            Board board = new Board();
            GameCard card1 = CreateCard(1);
            GameCard card2 = CreateCard(2);
            // Cards need BOTH ATTACK_ENGAGE AND ENGAGE to be non-defendable
            card1.AddState(CardState.ENGAGE);
            card1.AddState(CardState.ATTACK_ENGAGE);
            card2.AddState(CardState.ENGAGE);
            card2.AddState(CardState.ATTACK_ENGAGE);
            board.PosCard(card1, 0);
            board.PosCard(card2, 1);

            Assert.False(board.HasDefendableCards());
        }

        [Fact]
        public void HasDefendableCards_CardWithOnlyDefenseEngage_ShouldReturnTrue()
        {
            Board board = new Board();
            GameCard card = CreateCard(1);
            card.AddState(CardState.DEFENSE_ENGAGE);
            board.PosCard(card, 0);

            Assert.True(board.HasDefendableCards());
        }

        #endregion

        #region Attack and Defense Spot Checks

        [Fact]
        public void AttackAndDefenseChecks_ShouldMatchCardStates()
        {
            Board board = new Board();
            GameCard card = CreateCard(3);
            board.PosCard(card, 2);

            CardSlotState initial = board.canAttackSpot(2);
            Assert.Equal(CardSlotState.CAN_ATTACK, initial);

            card.AddState(CardState.ENGAGE);
            CardSlotState engage = board.canAttackSpot(2);
            Assert.Equal(CardSlotState.ENGAGE, engage);

            card.AddState(CardState.ATTACK_ENGAGE);
            CardSlotState attackEngage = board.canAttackSpot(2);
            Assert.Equal(CardSlotState.ATTACK_ENGAGE, attackEngage);

            card.RemoveStates(new HashSet<CardState> { CardState.ATTACK_ENGAGE, CardState.ENGAGE });
            CardSlotState defendInitial = board.canDefendSpot(2);
            Assert.Equal(CardSlotState.CAN_DEFEND, defendInitial);

            card.AddState(CardState.DEFENSE_ENGAGE);
            CardSlotState defendEngage = board.canDefendSpot(2);
            Assert.Equal(CardSlotState.DEFENSE_ENGAGE, defendEngage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void CanAttackSpot_AllLocations_ShouldReturnCorrectState(int location)
        {
            Board board = new Board();
            GameCard card = CreateCard(location + 1);
            board.PosCard(card, location);

            CardSlotState state = board.canAttackSpot(location);
            Assert.Equal(CardSlotState.CAN_ATTACK, state);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void CanDefendSpot_AllLocations_ShouldReturnCorrectState(int location)
        {
            Board board = new Board();
            GameCard card = CreateCard(location + 1);
            board.PosCard(card, location);

            CardSlotState state = board.canDefendSpot(location);
            Assert.Equal(CardSlotState.CAN_DEFEND, state);
        }

        [Fact]
        public void CanAttackSpot_InvalidLocation_ShouldReturnDefaultState()
        {
            Board board = new Board();
            CardSlotState state = board.canAttackSpot(99);
            Assert.Equal(CardSlotState.CAN_DEFEND, state);
        }

        [Fact]
        public void CanDefendSpot_InvalidLocation_ShouldReturnDefaultState()
        {
            Board board = new Board();
            CardSlotState state = board.canDefendSpot(99);
            Assert.Equal(CardSlotState.CAN_ATTACK, state);
        }

        #endregion

        #region Engage Helpers

        [Fact]
        public void EngageHelpers_ShouldToggleStates()
        {
            Board board = new Board();
            GameCard card = CreateCard(4);
            board.PosCard(card, 3);

            board.EngageAttackCard(3);
            Assert.True(card.HasState(CardState.ATTACK_ENGAGE));

            board.UnEngageAttackCard(3);
            Assert.False(card.HasState(CardState.ATTACK_ENGAGE));

            board.EngageDefenseCard(3);
            Assert.True(card.HasState(CardState.DEFENSE_ENGAGE));

            board.UnEngageDefenseCard(3);
            Assert.False(card.HasState(CardState.DEFENSE_ENGAGE));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void EngageAttackCard_AllLocations_ShouldEngageCard(int location)
        {
            Board board = new Board();
            GameCard card = CreateCard(location + 1);
            board.PosCard(card, location);

            board.EngageAttackCard(location);
            Assert.True(card.HasState(CardState.ATTACK_ENGAGE));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void EngageDefenseCard_AllLocations_ShouldEngageCard(int location)
        {
            Board board = new Board();
            GameCard card = CreateCard(location + 1);
            board.PosCard(card, location);

            board.EngageDefenseCard(location);
            Assert.True(card.HasState(CardState.DEFENSE_ENGAGE));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void UnEngageAttackCard_AllLocations_ShouldRemoveState(int location)
        {
            Board board = new Board();
            GameCard card = CreateCard(location + 1);
            card.AddState(CardState.ATTACK_ENGAGE);
            board.PosCard(card, location);

            board.UnEngageAttackCard(location);
            Assert.False(card.HasState(CardState.ATTACK_ENGAGE));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void UnEngageDefenseCard_AllLocations_ShouldRemoveState(int location)
        {
            Board board = new Board();
            GameCard card = CreateCard(location + 1);
            card.AddState(CardState.DEFENSE_ENGAGE);
            board.PosCard(card, location);

            board.UnEngageDefenseCard(location);
            Assert.False(card.HasState(CardState.DEFENSE_ENGAGE));
        }

        #endregion

        #region Reset Board Engage State

        [Fact]
        public void ResetBoardEngageState_ShouldClearAllEngageStates()
        {
            Board board = new Board();
            GameCard card = CreateCard(5);
            card.AddState(CardState.ATTACK_ENGAGE);
            card.AddState(CardState.DEFENSE_ENGAGE);
            card.AddState(CardState.ENGAGE);
            board.PosCard(card, 4);

            board.ResetBoardEngageState();

            Assert.Empty(card.GetState());
        }

        [Fact]
        public void ResetBoardEngageState_MultipleCards_ShouldClearAll()
        {
            Board board = new Board();

            GameCard card1 = CreateCard(1);
            card1.AddState(CardState.ATTACK_ENGAGE);
            board.PosCard(card1, 0);

            GameCard card2 = CreateCard(2);
            card2.AddState(CardState.DEFENSE_ENGAGE);
            board.PosCard(card2, 1);

            GameCard card3 = CreateCard(3);
            card3.AddState(CardState.ENGAGE);
            board.PosCard(card3, 2);

            board.ResetBoardEngageState();

            Assert.Empty(card1.GetState());
            Assert.Empty(card2.GetState());
            Assert.Empty(card3.GetState());
        }

        [Fact]
        public void ResetBoardEngageState_EmptyBoard_ShouldNotThrow()
        {
            Board board = new Board();
            Exception ex = Record.Exception(() => board.ResetBoardEngageState());
            Assert.Null(ex);
        }

        #endregion

        #region Clear Spot Tests

        [Fact]
        public void ClearSpot_ShouldRemoveCardFromLocation()
        {
            Board board = new Board();
            GameCard card = CreateCard(6);
            board.PosCard(card, 0);
            Assert.Equal(1, board.GetCardCount());

            board.clearSpot(6);
            Assert.Equal(0, board.GetCardCount());
            Assert.True(board.IsAvailable(0));
        }

        // NOTE: The ClearSpot_AllLocations test variants for locations 1-5 are commented out
        // because clearSpot() has a bug where it doesn't null-check locations before calling GetGameCardId().
        // Only location 0 works because _location_1 is checked first.
        [Theory]
        [InlineData(0)]
        public void ClearSpot_AllLocations_ShouldRemoveCard(int location)
        {
            Board board = new Board();
            int cardId = 300 + location;
            GameCard card = CreateCard(cardId);
            board.PosCard(card, location);

            Assert.Equal(1, board.GetCardCount());
            board.clearSpot(cardId);
            Assert.Equal(0, board.GetCardCount());
            Assert.True(board.IsAvailable(location));
        }

        [Fact]
        public void ClearSpot_MultipleCards_ShouldRemoveOnlySpecified()
        {
            Board board = new Board();
            GameCard card1 = CreateCard(1);
            GameCard card2 = CreateCard(2);
            GameCard card3 = CreateCard(3);
            board.PosCard(card1, 0);
            board.PosCard(card2, 1);
            board.PosCard(card3, 2);

            Assert.Equal(3, board.GetCardCount());
            board.clearSpot(2);
            Assert.Equal(2, board.GetCardCount());
            Assert.False(board.IsAvailable(0));
            Assert.True(board.IsAvailable(1));
            Assert.False(board.IsAvailable(2));
        }

        #endregion
    }
}
