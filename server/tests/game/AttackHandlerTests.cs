using System;
using System.Collections.Generic;
using System.Linq;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;
using GameCard = VortexTCG.Game.Object.Card;
using GameDefenseCard = VortexTCG.Game.Object.DefenseCard;
using Xunit;

namespace game.Tests
{
    public class AttackHandlerTests
    {
        private GameCard BuildCard(int gameCardId, int attack = 3, int hp = 5)
        {
            CardDTO dto = new CardDTO
            {
                Id = Guid.NewGuid(),
                Name = "Card" + gameCardId,
                Hp = hp,
                Attack = attack,
                Cost = 1,
                Description = "desc",
                CardType = CardType.GUARD,
                Class = new List<string> { "class" }
            };

            GameCard card = new GameCard(dto, gameCardId);
            return card;
        }

        [Fact]
        public void AddAttack_ShouldTrackAndFormatAttackers()
        {
            AttackHandler handler = new AttackHandler();
            GameCard firstAttacker = BuildCard(1);
            GameCard secondAttacker = BuildCard(2);
            Guid playerId = Guid.NewGuid();
            Guid opponentId = Guid.NewGuid();

            handler.AddAttack(firstAttacker);
            handler.AddAttack(secondAttacker);

            List<GameCard> attackers = handler.GetAttacker();
            Assert.Equal(2, attackers.Count);
            Assert.Same(firstAttacker, attackers[0]);
            Assert.Same(secondAttacker, attackers[1]);

            AttackResponseDto dto = handler.FormatAttackResponseDto(playerId, opponentId);
            Assert.Equal(new List<int> { 1, 2 }, dto.AttackCardsId);
            Assert.Equal(playerId, dto.PlayerId);
            Assert.Equal(opponentId, dto.OpponentId);
        }

        [Fact]
        public void RemoveAttack_ShouldRemoveSpecificAttacker()
        {
            AttackHandler handler = new AttackHandler();
            GameCard firstAttacker = BuildCard(3);
            GameCard secondAttacker = BuildCard(4);

            handler.AddAttack(firstAttacker);
            handler.AddAttack(secondAttacker);

            handler.RemoveAttack(firstAttacker);

            List<GameCard> attackers = handler.GetAttacker();
            Assert.Single(attackers);
            Assert.Same(secondAttacker, attackers[0]);
        }

        [Fact]
        public void AddDefense_ShouldReplaceExistingForSameOpponent()
        {
            AttackHandler handler = new AttackHandler();
            GameCard defenderOne = BuildCard(5);
            GameCard defenderTwo = BuildCard(6);
            GameCard opponentCard = BuildCard(20);

            handler.AddDefense(defenderOne, opponentCard);
            handler.AddDefense(defenderTwo, opponentCard);

            List<GameDefenseCard> defenders = handler.GetDefender();
            Assert.Single(defenders);
            Assert.Same(defenderTwo, defenders[0].card);
            Assert.Equal(opponentCard.GetGameCardId(), defenders[0].oppositeCardId);
        }

        [Fact]
        public void RemoveDefense_ShouldRemoveMatchingDefender()
        {
            AttackHandler handler = new AttackHandler();
            GameCard defenderOne = BuildCard(7);
            GameCard defenderTwo = BuildCard(8);
            GameCard opponentCardOne = BuildCard(30);
            GameCard opponentCardTwo = BuildCard(31);

            handler.AddDefense(defenderOne, opponentCardOne);
            handler.AddDefense(defenderTwo, opponentCardTwo);

            handler.RemoveDefense(defenderOne);

            List<GameDefenseCard> defenders = handler.GetDefender();
            Assert.Single(defenders);
            Assert.Same(defenderTwo, defenders[0].card);
            Assert.Equal(opponentCardTwo.GetGameCardId(), defenders[0].oppositeCardId);
        }

        [Fact]
        public void ResetAttackHandler_ShouldClearAttackAndDefense()
        {
            AttackHandler handler = new AttackHandler();
            GameCard attacker = BuildCard(9);
            GameCard defender = BuildCard(10);
            GameCard opponent = BuildCard(40);

            handler.AddAttack(attacker);
            handler.AddDefense(defender, opponent);

            handler.ResetAttackHandler();

            Assert.Empty(handler.GetAttacker());
            Assert.Empty(handler.GetDefender());

            DefenseResponseDto defenseResponse = handler.FormatDefenseResponseDto(Guid.NewGuid(), Guid.NewGuid());
            Assert.Empty(defenseResponse.data.AttackCardsId);
            Assert.Empty(defenseResponse.data.DefenseCards);

            AttackResponseDto attackResponse = handler.FormatAttackResponseDto(Guid.NewGuid(), Guid.NewGuid());
            Assert.Empty(attackResponse.AttackCardsId);
        }

        [Fact]
        public void FormatDefenseResponseDto_ShouldIncludeAttackAndDefenseData()
        {
            AttackHandler handler = new AttackHandler();
            GameCard attackerOne = BuildCard(11);
            GameCard attackerTwo = BuildCard(12);
            GameCard defenderOne = BuildCard(13);
            GameCard defenderTwo = BuildCard(14);
            GameCard opponentOne = BuildCard(50);
            GameCard opponentTwo = BuildCard(51);
            Guid playerId = Guid.NewGuid();
            Guid opponentId = Guid.NewGuid();

            handler.AddAttack(attackerOne);
            handler.AddAttack(attackerTwo);
            handler.AddDefense(defenderOne, opponentOne);
            handler.AddDefense(defenderTwo, opponentTwo);

            DefenseResponseDto response = handler.FormatDefenseResponseDto(playerId, opponentId);

            Assert.Equal(playerId, response.PlayerId);
            Assert.Equal(opponentId, response.OpponentId);
            Assert.Equal(new List<int> { 11, 12 }, response.data.AttackCardsId);

            bool hasFirst = response.data.DefenseCards.Any(item => item.cardId == defenderOne.GetGameCardId() && item.opponentCardId == opponentOne.GetGameCardId());
            bool hasSecond = response.data.DefenseCards.Any(item => item.cardId == defenderTwo.GetGameCardId() && item.opponentCardId == opponentTwo.GetGameCardId());
            Assert.True(hasFirst);
            Assert.True(hasSecond);
        }

        [Fact]
        public void GetSpecificDefender_ShouldReturnMatchingDefender()
        {
            AttackHandler handler = new AttackHandler();
            GameCard defenderOne = BuildCard(15);
            GameCard defenderTwo = BuildCard(16);
            GameCard opponentOne = BuildCard(60);
            GameCard opponentTwo = BuildCard(61);

            handler.AddDefense(defenderOne, opponentOne);
            handler.AddDefense(defenderTwo, opponentTwo);

            GameDefenseCard result = handler.GetSpecificDefender(opponentTwo.GetGameCardId());

            Assert.NotNull(result);
            Assert.Same(defenderTwo, result.card);
            Assert.Equal(opponentTwo.GetGameCardId(), result.oppositeCardId);
        }
    }
}
