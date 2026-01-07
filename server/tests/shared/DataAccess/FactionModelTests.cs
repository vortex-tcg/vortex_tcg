using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class FactionModelTests
    {
        [Fact]
        public void Faction_WithCardLink()
        {
            DateTime now = DateTime.UtcNow;
            Guid factionId = Guid.NewGuid();

            FactionModel faction = new FactionModel
            {
                Id = factionId,
                Label = "Shadow",
                Currency = "Dark",
                Condition = "Stealth",
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Cards = new List<FactionCard>(),
                Decks = new List<Deck>(),
                Champions = new List<Champion>()
            };

            Card card = new Card { Id = Guid.NewGuid(), Name = "Shade", Price = 1, Hp = 2, Attack = 3, Cost = 1, Description = "", Picture = "", Extension = Extension.BASIC, CardType = CardType.GUARD, CreatedAtUtc = now, CreatedBy = "seed", Collections = new List<CollectionCard>(), Class = new List<ClassCard>(), Boosters = new List<BoosterCard>(), Factions = new List<FactionCard>(), Effect = new List<EffectCard>() };

            FactionCard link = new FactionCard
            {
                Id = Guid.NewGuid(),
                CardId = card.Id,
                Card = card,
                FactionId = factionId,
                Faction = faction,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            faction.Cards.Add(link);
            card.Factions.Add(link);

            FactionCard stored = Assert.Single(faction.Cards);
            Assert.Equal(card.Id, stored.CardId);
            Assert.Same(card, stored.Card);
            Assert.Equal(factionId, stored.FactionId);
            Assert.Same(faction, stored.Faction);
        }
    }
}
