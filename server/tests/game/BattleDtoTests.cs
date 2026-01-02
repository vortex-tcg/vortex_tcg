using System;
using System.Collections.Generic;
using VortexTCG.Game.DTO;
using VortexTCG.Game.Object;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace game.Tests
{
    public class BattleDtoTests
    {
        private GameCardDto BuildGameCardDto(int gameCardId)
        {
            GameCardDto dto = new GameCardDto
            {
                Id = Guid.NewGuid(),
                GameCardId = gameCardId,
                Name = "Card" + gameCardId,
                Hp = 10,
                Attack = 5,
                Cost = 2,
                Description = "desc",
                CardType = CardType.GUARD,
                Class = new List<string> { "class" },
                State = new List<CardState> { CardState.ENGAGE }
            };

            return dto;
        }

        private BattleChampionDto BuildBattleChampionDto(int hp)
        {
            BattleChampionDto dto = new BattleChampionDto
            {
                Hp = hp,
                Gold = 3,
                SecondaryCurrency = 1
            };

            return dto;
        }

        [Fact]
        public void AttackResponseDto_ShouldStoreValues()
        {
            List<int> attackIds = new List<int> { 1, 2, 3 };
            Guid playerId = Guid.NewGuid();
            Guid opponentId = Guid.NewGuid();

            AttackResponseDto dto = new AttackResponseDto
            {
                AttackCardsId = attackIds,
                PlayerId = playerId,
                OpponentId = opponentId
            };

            Assert.Equal(attackIds, dto.AttackCardsId);
            Assert.Equal(playerId, dto.PlayerId);
            Assert.Equal(opponentId, dto.OpponentId);
        }

        [Fact]
        public void DefenseResponseDto_ShouldStoreNestedValues()
        {
            List<int> attackIds = new List<int> { 4, 5 };
            List<DefenseCardDataDto> defenseCards = new List<DefenseCardDataDto>
            {
                new DefenseCardDataDto { cardId = 10, opponentCardId = 20 },
                new DefenseCardDataDto { cardId = 11, opponentCardId = 21 }
            };
            Guid playerId = Guid.NewGuid();
            Guid opponentId = Guid.NewGuid();

            DefenseResponseDto dto = new DefenseResponseDto
            {
                PlayerId = playerId,
                OpponentId = opponentId,
                data = new DefenseDataResponseDto
                {
                    AttackCardsId = attackIds,
                    DefenseCards = defenseCards
                }
            };

            Assert.Equal(playerId, dto.PlayerId);
            Assert.Equal(opponentId, dto.OpponentId);
            Assert.Equal(attackIds, dto.data.AttackCardsId);
            Assert.Equal(defenseCards, dto.data.DefenseCards);
        }

        [Fact]
        public void BattleAgainstCardDataDto_ShouldStoreCardAndChampionData()
        {
            GameCardDto attacker = BuildGameCardDto(50);
            GameCardDto defender = BuildGameCardDto(60);
            BattleChampionDto attackerChamp = BuildBattleChampionDto(15);
            BattleChampionDto defenderChamp = BuildBattleChampionDto(20);

            BattleAgainstCardDataDto dto = new BattleAgainstCardDataDto
            {
                isAttackerDead = true,
                isDefenderDead = false,
                attackerDamageDeal = 7,
                defenderDamageDeal = 3,
                attackerCard = attacker,
                defenderCard = defender,
                attackerChamp = attackerChamp,
                defenderChamp = defenderChamp
            };

            Assert.True(dto.isAttackerDead);
            Assert.False(dto.isDefenderDead);
            Assert.Equal(7, dto.attackerDamageDeal);
            Assert.Equal(3, dto.defenderDamageDeal);
            Assert.Same(attacker, dto.attackerCard);
            Assert.Same(defender, dto.defenderCard);
            Assert.Same(attackerChamp, dto.attackerChamp);
            Assert.Same(defenderChamp, dto.defenderChamp);
        }

        [Fact]
        public void BattlaAgainstChampDataDto_ShouldStoreChampionData()
        {
            GameCardDto attacker = BuildGameCardDto(70);
            BattleChampionDto attackerChamp = BuildBattleChampionDto(12);
            BattleChampionDto defenderChamp = BuildBattleChampionDto(18);

            BattlaAgainstChampDataDto dto = new BattlaAgainstChampDataDto
            {
                isChampDead = false,
                isCardDead = true,
                attackerDamageDeal = 6,
                championDamageDeal = 4,
                attackerCard = attacker,
                attackerChamp = attackerChamp,
                defenderChamp = defenderChamp
            };

            Assert.False(dto.isChampDead);
            Assert.True(dto.isCardDead);
            Assert.Equal(6, dto.attackerDamageDeal);
            Assert.Equal(4, dto.championDamageDeal);
            Assert.Same(attacker, dto.attackerCard);
            Assert.Same(attackerChamp, dto.attackerChamp);
            Assert.Same(defenderChamp, dto.defenderChamp);
        }

        [Fact]
        public void BattleResponseDto_ShouldStoreBattles()
        {
            BattleAgainstCardDataDto battleAgainstCard = new BattleAgainstCardDataDto
            {
                isAttackerDead = false,
                isDefenderDead = false,
                attackerDamageDeal = 2,
                defenderDamageDeal = 1,
                attackerCard = BuildGameCardDto(80),
                defenderCard = BuildGameCardDto(81),
                attackerChamp = BuildBattleChampionDto(10),
                defenderChamp = BuildBattleChampionDto(11)
            };

            BattleDataDto battleData = new BattleDataDto
            {
                isAgainstChamp = false,
                againstCard = battleAgainstCard,
                againstChamp = null
            };

            BattlesDataDto battlesData = new BattlesDataDto
            {
                battles = new List<BattleDataDto> { battleData }
            };

            Guid player1 = Guid.NewGuid();
            Guid player2 = Guid.NewGuid();

            BattleResponseDto response = new BattleResponseDto
            {
                data = battlesData,
                Player1Id = player1,
                Player2Id = player2
            };

            Assert.Equal(player1, response.Player1Id);
            Assert.Equal(player2, response.Player2Id);
            Assert.Single(response.data.battles);
            Assert.Same(battleData, response.data.battles[0]);
            Assert.Same(battleAgainstCard, response.data.battles[0].againstCard);
            Assert.Null(response.data.battles[0].againstChamp);
        }
    }
}