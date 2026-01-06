using System;
using System.Reflection;
using VortexTCG.DataAccess.Models;
using VortexTCG.Game.DTO;
using GameCard = VortexTCG.Game.Object.Card;
using GameChampion = VortexTCG.Game.Object.Champion;
using Xunit;

namespace game.Tests
{
    public class ChampionObjectTests
    {
        private GameCard CreateCard(int attack)
        {
            CardDTO dto = new CardDTO
            {
                Id = Guid.NewGuid(),
                Name = "Attacker",
                Hp = 5,
                Attack = attack,
                Cost = 1,
                Description = "",
                CardType = CardType.GUARD,
                Class = new System.Collections.Generic.List<string> { "class" }
            };
            return new GameCard(dto, 1);
        }

        [Fact]
        public void InitChampion_ShouldSetDefaults()
        {
            GameChampion champion = new GameChampion();
            champion.initChampion(Guid.NewGuid());

            Assert.Equal(30, champion.GetHp());
            Assert.Equal(1, champion.GetBaseGold());
            Assert.Equal(0, champion.GetFatigue());
        }

        [Fact]
        public void ApplyDamage_ShouldReduceHpAndReturnDamage()
        {
            GameChampion champion = new GameChampion();
            champion.initChampion(Guid.NewGuid());
            GameCard source = CreateCard(7);

            int applied = champion.ApplyDamage(source);

            Assert.Equal(7, applied);
            Assert.Equal(23, champion.GetHp());
            Assert.False(champion.IsDead());
        }

        [Fact]
        public void ApplyFatigueDamage_ShouldIncreaseCounterAndReduceHp()
        {
            GameChampion champion = new GameChampion();
            champion.initChampion(Guid.NewGuid());

            MethodInfo? fatigueMethod = typeof(GameChampion).GetMethod("ApplyFatigueDamage", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(fatigueMethod);
            fatigueMethod!.Invoke(champion, null);
            Assert.Equal(29, champion.GetHp());
            Assert.Equal(1, champion.GetFatigue());

            fatigueMethod.Invoke(champion, null);
            Assert.Equal(27, champion.GetHp());
            Assert.Equal(2, champion.GetFatigue());
        }

        [Fact]
        public void GoldManagement_ShouldRespectBaseGold()
        {
            GameChampion champion = new GameChampion();
            champion.initChampion(Guid.NewGuid());

            bool canPay = champion.TryPaiedCard(1);
            Assert.True(canPay);
            champion.PayCard(1);

            canPay = champion.TryPaiedCard(1);
            Assert.False(canPay);

            champion.SetBaseGold(3);
            champion.resetGold();
            Assert.True(champion.TryPaiedCard(3));
        }

        [Fact]
        public void FormatDtos_ShouldReflectCurrentState()
        {
            GameChampion champion = new GameChampion();
            champion.initChampion(Guid.NewGuid());
            champion.SetBaseGold(2);
            champion.resetGold();

            PlayCardChampionDto playDto = champion.FormatPlayCardChampionDto();
            BattleChampionDto battleDto = champion.FormatBattleChampionDto();

            Assert.Equal(champion.GetHp(), playDto.Hp);
            Assert.Equal(champion.GetBaseGold(), playDto.Gold);
            Assert.Equal(playDto.Hp, battleDto.Hp);
            Assert.Equal(playDto.SecondaryCurrency, battleDto.SecondaryCurrency);
        }
    }
}
