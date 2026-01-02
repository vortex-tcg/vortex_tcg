using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class BoosterModelTests
    {
        [Fact]
        public void Booster_AllProperties_AreSetAndRetrieved()
        {
            DateTime created = DateTime.UtcNow;
            DateTime updated = created.AddMinutes(1);
            Guid boosterId = Guid.NewGuid();

            Booster booster = new Booster
            {
                Id = boosterId,
                Label = "Starter Pack",
                Price = 4.99f,
                CreatedAtUtc = created,
                CreatedBy = "tester",
                UpdatedAtUtc = updated,
                UpdatedBy = "editor",
                Cards = new List<BoosterCard>()
            };

            Assert.Equal(boosterId, booster.Id);
            Assert.Equal("Starter Pack", booster.Label);
            Assert.Equal(4.99f, booster.Price);
            Assert.Equal(created, booster.CreatedAtUtc);
            Assert.Equal("tester", booster.CreatedBy);
            Assert.Equal(updated, booster.UpdatedAtUtc);
            Assert.Equal("editor", booster.UpdatedBy);
            Assert.Empty(booster.Cards);
        }

        [Fact]
        public void BoosterCard_AllProperties_AreSetAndRetrieved()
        {
            DateTime now = DateTime.UtcNow;
            Guid boosterId = Guid.NewGuid();
            Guid cardId = Guid.NewGuid();

            Booster booster = new Booster
            {
                Id = boosterId,
                Label = "Mega Pack",
                Price = 9.99f,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Cards = new List<BoosterCard>()
            };

            Card card = new Card
            {
                Id = cardId,
                Name = "Guardian",
                Price = 1,
                Hp = 2,
                Attack = 3,
                Cost = 1,
                Description = "Guard the realm",
                Picture = "guardian.png",
                Extension = Extension.BASIC,
                CardType = CardType.GUARD,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            BoosterCard boosterCard = new BoosterCard
            {
                Id = Guid.NewGuid(),
                Probability = 0.42f,
                CardId = cardId,
                Card = card,
                BoosterId = boosterId,
                Booster = booster,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            booster.Cards.Add(boosterCard);
            card.Boosters.Add(boosterCard);

            BoosterCard stored = Assert.Single(booster.Cards);
            Assert.Equal(boosterCard.Id, stored.Id);
            Assert.Equal(0.42f, stored.Probability);
            Assert.Equal(cardId, stored.CardId);
            Assert.Same(card, stored.Card);
            Assert.Equal(boosterId, stored.BoosterId);
            Assert.Same(booster, stored.Booster);
            Assert.Equal(now, stored.CreatedAtUtc);
            Assert.Equal("seed", stored.CreatedBy);
        }
    }
}
