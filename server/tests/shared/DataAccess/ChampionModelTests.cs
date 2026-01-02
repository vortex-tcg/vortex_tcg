using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class ChampionModelTests
    {
        [Fact]
        public void Champion_LinksFactionAndEffect()
        {
            DateTime now = DateTime.UtcNow;
            Guid champId = Guid.NewGuid();
            Guid factionId = Guid.NewGuid();
            Guid effectId = Guid.NewGuid();

            FactionModel faction = new FactionModel
            {
                Id = factionId,
                Label = "Fire",
                Currency = "Flame",
                Condition = "Burn",
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Cards = new List<FactionCard>(),
                Decks = new List<Deck>(),
                Champions = new List<Champion>()
            };

            Effect effect = new Effect
            {
                Id = effectId,
                Title = "Ignite",
                Parameter = "5",
                CreatedAtUtc = now,
                CreatedBy = "seed",
                EffectTypeId = Guid.NewGuid(),
                EffectType = new EffectType { Id = Guid.NewGuid(), Label = "Buff", CreatedAtUtc = now, CreatedBy = "seed", Effect = new List<Effect>() },
                EffectDescriptionId = Guid.NewGuid(),
                EffectDescription = new EffectDescription { Id = Guid.NewGuid(), Label = "Burn", Description = "Adds burn", Parameter = null, CreatedAtUtc = now, CreatedBy = "seed", Effects = new List<Effect>() },
                StartConditionId = Guid.NewGuid(),
                StartCondition = new Condition { Id = Guid.NewGuid(), Label = "OnStart", ConditionDescription = "start", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "TypeA", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() },
                EndConditionId = Guid.NewGuid(),
                EndCondition = new Condition { Id = Guid.NewGuid(), Label = "OnEnd", ConditionDescription = "end", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "TypeB", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() },
                Cards = new List<EffectCard>()
            };

            Champion champion = new Champion
            {
                Id = champId,
                Name = "Pyro",
                Description = "Fire master",
                HP = 30,
                Picture = "pyro.png",
                FactionId = factionId,
                Faction = faction,
                EffectId = effectId,
                Effect = effect,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Collections = new List<CollectionChampion>()
            };

            Assert.Equal(champId, champion.Id);
            Assert.Equal("Pyro", champion.Name);
            Assert.Equal("Fire master", champion.Description);
            Assert.Equal(30, champion.HP);
            Assert.Equal("pyro.png", champion.Picture);
            Assert.Equal(factionId, champion.FactionId);
            Assert.Same(faction, champion.Faction);
            Assert.Equal(effectId, champion.EffectId);
            Assert.Same(effect, champion.Effect);
            Assert.Equal(now, champion.CreatedAtUtc);
            Assert.Equal("seed", champion.CreatedBy);
            Assert.Empty(champion.Collections);
        }
    }
}
