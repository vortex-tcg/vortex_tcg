using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class EffectModelTests
    {
        [Fact]
        public void Effect_WithTypeDescriptionConditions()
        {
            DateTime now = DateTime.UtcNow;
            EffectType type = new EffectType { Id = Guid.NewGuid(), Label = "Damage", CreatedAtUtc = now, CreatedBy = "seed", Effect = new List<Effect>() };
            EffectDescription desc = new EffectDescription { Id = Guid.NewGuid(), Label = "Hit", Description = "Deal", Parameter = "X", CreatedAtUtc = now, CreatedBy = "seed", Effects = new List<Effect>() };
            Condition start = new Condition { Id = Guid.NewGuid(), Label = "OnPlay", ConditionDescription = "start", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "Type", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() };
            Condition end = new Condition { Id = Guid.NewGuid(), Label = "After", ConditionDescription = "end", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "Type2", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() };

            Effect effect = new Effect
            {
                Id = Guid.NewGuid(),
                Title = "Smash",
                Parameter = "3",
                EffectTypeId = type.Id,
                EffectType = type,
                EffectDescriptionId = desc.Id,
                EffectDescription = desc,
                StartConditionId = start.Id,
                StartCondition = start,
                EndConditionId = end.Id,
                EndCondition = end,
                Champion = new Champion { Id = Guid.NewGuid(), Name = "Brute", Description = "", HP = 10, Picture = "", FactionId = Guid.NewGuid(), Faction = new FactionModel { Id = Guid.NewGuid(), Label = "F", Currency = "C", Condition = "Cond", CreatedAtUtc = now, CreatedBy = "seed", Cards = new List<FactionCard>(), Decks = new List<Deck>(), Champions = new List<Champion>() }, EffectId = Guid.NewGuid(), CreatedAtUtc = now, CreatedBy = "seed", Collections = new List<CollectionChampion>(), Effect = null! },
                Cards = new List<EffectCard>(),
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            type.Effect.Add(effect);
            desc.Effects.Add(effect);
            start.StartEffects.Add(effect);
            end.EndEffects.Add(effect);

            Assert.Equal("Smash", effect.Title);
            Assert.Equal("3", effect.Parameter);
            Assert.Same(type, effect.EffectType);
            Assert.Same(desc, effect.EffectDescription);
            Assert.Same(start, effect.StartCondition);
            Assert.Same(end, effect.EndCondition);
            Assert.Contains(effect, type.Effect);
            Assert.Contains(effect, desc.Effects);
            Assert.Contains(effect, start.StartEffects);
            Assert.Contains(effect, end.EndEffects);
        }

        [Fact]
        public void EffectCard_Links_Effect_And_Card()
        {
            DateTime now = DateTime.UtcNow;
            Card card = new Card { Id = Guid.NewGuid(), Name = "Bolt", Price = 1, Hp = 1, Attack = 1, Cost = 1, Description = "", Picture = "", Extension = Extension.BASIC, CardType = CardType.SPELL, CreatedAtUtc = now, CreatedBy = "seed", Collections = new List<CollectionCard>(), Class = new List<ClassCard>(), Boosters = new List<BoosterCard>(), Factions = new List<FactionCard>(), Effect = new List<EffectCard>() };
            Effect effect = new Effect { Id = Guid.NewGuid(), Title = "Zap", Parameter = "1", EffectTypeId = Guid.NewGuid(), EffectType = new EffectType { Id = Guid.NewGuid(), Label = "Type", CreatedAtUtc = now, CreatedBy = "seed", Effect = new List<Effect>() }, EffectDescriptionId = Guid.NewGuid(), EffectDescription = new EffectDescription { Id = Guid.NewGuid(), Label = "Label", Description = "", Parameter = null, CreatedAtUtc = now, CreatedBy = "seed", Effects = new List<Effect>() }, StartConditionId = Guid.NewGuid(), StartCondition = new Condition { Id = Guid.NewGuid(), Label = "Start", ConditionDescription = "", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "CT", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() }, EndConditionId = Guid.NewGuid(), EndCondition = new Condition { Id = Guid.NewGuid(), Label = "End", ConditionDescription = "", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "CT2", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() }, Cards = new List<EffectCard>(), CreatedAtUtc = now, CreatedBy = "seed" };

            EffectCard link = new EffectCard
            {
                Id = Guid.NewGuid(),
                EffectId = effect.Id,
                Effect = effect,
                CardId = card.Id,
                Card = card,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            effect.Cards.Add(link);
            card.Effect.Add(link);

            EffectCard stored = Assert.Single(effect.Cards);
            Assert.Equal(card.Id, stored.CardId);
            Assert.Same(card, stored.Card);
            Assert.Equal(effect.Id, stored.EffectId);
            Assert.Same(effect, stored.Effect);
        }
    }
}
