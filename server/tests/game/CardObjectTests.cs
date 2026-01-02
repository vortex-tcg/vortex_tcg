using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;
using GameCard = VortexTCG.Game.Object.Card;
using Xunit;

namespace game.Tests
{
    public class CardObjectTests
    {
        private CardDTO BuildCardDto(string name, int hp = 10, int attack = 4, int cost = 2)
        {
            CardDTO dto = new CardDTO
            {
                Id = Guid.NewGuid(),
                Name = name,
                Hp = hp,
                Attack = attack,
                Cost = cost,
                Description = "desc",
                CardType = CardType.GUARD,
                Class = new List<string> { "class" }
            };

            return dto;
        }

        [Fact]
        public void Constructor_ShouldInitializeFields()
        {
            CardDTO dto = BuildCardDto("Alpha", 8, 3, 5);
            GameCard card = new GameCard(dto, 1);

            Assert.Equal(dto.Id, card.GetCardId());
            Assert.Equal(1, card.GetGameCardId());
            Assert.Equal("Alpha", card.GetName());
            Assert.Equal(8, card.GetHp());
            Assert.Equal(3, card.GetAttack());
            Assert.Equal(5, card.GetCost());
            Assert.Equal("desc", card.GetDescription());
            Assert.Equal(CardType.GUARD, card.GetCardType());
            Assert.Equal(new List<string> { "class" }, card.GetClasses());
            Assert.Empty(card.GetState());
        }

        [Fact]
        public void StateManagement_ShouldAddRemoveAndQueryStates()
        {
            GameCard card = new GameCard(BuildCardDto("Beta"), 2);

            card.AddState(CardState.ENGAGE);
            card.AddState(CardState.ATTACK_ENGAGE);
            card.AddState(CardState.ATTACK_ENGAGE); // duplicate ignored

            List<CardState> states = card.GetState();
            Assert.Equal(2, states.Count);
            Assert.Contains(CardState.ENGAGE, states);
            Assert.Contains(CardState.ATTACK_ENGAGE, states);

            bool hasEngage = card.HasState(CardState.ENGAGE);
            bool hasAll = card.HasStates(new List<CardState> { CardState.ENGAGE, CardState.ATTACK_ENGAGE });
            bool hasOne = card.HasOneState(new List<CardState> { CardState.DEFENSE_ENGAGE, CardState.ATTACK_ENGAGE });

            Assert.True(hasEngage);
            Assert.True(hasAll);
            Assert.True(hasOne);

            HashSet<CardState> removeSet = new HashSet<CardState> { CardState.ENGAGE };
            card.RemoveStates(removeSet);
            card.RemoveState(CardState.ATTACK_ENGAGE);

            Assert.Empty(card.GetState());
        }

        [Fact]
        public void CheckCanDefend_ShouldReturnExpectedStates()
        {
            GameCard card = new GameCard(BuildCardDto("Gamma"), 3);

            CardSlotState initial = card.checkCanDefend();
            Assert.Equal(CardSlotState.CAN_DEFEND, initial);

            card.AddState(CardState.ENGAGE);
            CardSlotState withEngage = card.checkCanDefend();
            Assert.Equal(CardSlotState.ENGAGE, withEngage);

            card.AddState(CardState.DEFENSE_ENGAGE);
            CardSlotState withDefenseEngage = card.checkCanDefend();
            Assert.Equal(CardSlotState.DEFENSE_ENGAGE, withDefenseEngage);
        }

        [Fact]
        public void CheckCanAttack_ShouldReturnExpectedStates()
        {
            GameCard card = new GameCard(BuildCardDto("Delta"), 4);

            CardSlotState initial = card.checkCanAttack();
            Assert.Equal(CardSlotState.CAN_ATTACK, initial);

            card.AddState(CardState.ENGAGE);
            CardSlotState withEngage = card.checkCanAttack();
            Assert.Equal(CardSlotState.ENGAGE, withEngage);

            card.AddState(CardState.ATTACK_ENGAGE);
            CardSlotState withAttackEngage = card.checkCanAttack();
            Assert.Equal(CardSlotState.ATTACK_ENGAGE, withAttackEngage);
        }

        [Fact]
        public void ApplyDamage_ShouldReduceHpAndReturnDamage()
        {
            GameCard source = new GameCard(BuildCardDto("Src", hp: 10, attack: 6), 5);
            GameCard target = new GameCard(BuildCardDto("Tgt", hp: 12), 6);

            int damage = target.ApplyDamage(source);

            Assert.Equal(6, damage);
            Assert.Equal(6, target.GetHp());
            Assert.False(target.IsDead());

            target.ApplyDamage(source);
            target.ApplyDamage(source);

            Assert.True(target.IsDead());
        }

        [Fact]
        public void FormatGameCardDto_ShouldMirrorCurrentState()
        {
            GameCard card = new GameCard(BuildCardDto("Echo", hp: 5, attack: 2, cost: 1), 7);
            card.AddState(CardState.DEFENSE_ENGAGE);

            GameCardDto dto = card.FormatGameCardDto();

            Assert.Equal(card.GetCardId(), dto.Id);
            Assert.Equal(card.GetGameCardId(), dto.GameCardId);
            Assert.Equal(card.GetName(), dto.Name);
            Assert.Equal(card.GetHp(), dto.Hp);
            Assert.Equal(card.GetAttack(), dto.Attack);
            Assert.Equal(card.GetCost(), dto.Cost);
            Assert.Equal(card.GetDescription(), dto.Description);
            Assert.Equal(card.GetCardType(), dto.CardType);
            Assert.Equal(card.GetClasses(), dto.Class);
            Assert.Equal(card.GetState(), dto.State);
        }
    }
}