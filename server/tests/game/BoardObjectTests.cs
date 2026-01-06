using System;
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
                Class = new System.Collections.Generic.List<string> { "class" }
            };
            return new GameCard(dto, gameCardId);
        }

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

            card.RemoveStates(new System.Collections.Generic.HashSet<CardState> { CardState.ATTACK_ENGAGE, CardState.ENGAGE });
            CardSlotState defendInitial = board.canDefendSpot(2);
            Assert.Equal(CardSlotState.CAN_DEFEND, defendInitial);

            card.AddState(CardState.DEFENSE_ENGAGE);
            CardSlotState defendEngage = board.canDefendSpot(2);
            Assert.Equal(CardSlotState.DEFENSE_ENGAGE, defendEngage);
        }

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
    }
}
