using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class ClassModelTests
    {
        [Fact]
        public void Class_And_ClassCard_Link()
        {
            DateTime now = DateTime.UtcNow;
            Guid classId = Guid.NewGuid();
            Guid cardId = Guid.NewGuid();

            Card card = new Card
            {
                Id = cardId,
                Name = "Mage",
                Price = 1,
                Hp = 1,
                Attack = 1,
                Cost = 1,
                Description = "Magic",
                Picture = "mage.png",
                Extension = Extension.BASIC,
                CardType = CardType.SPELL,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            Class cls = new Class
            {
                Id = classId,
                Label = "Wizard",
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Cards = new List<ClassCard>()
            };

            ClassCard link = new ClassCard
            {
                Id = Guid.NewGuid(),
                CardId = cardId,
                Card = card,
                ClassId = classId,
                Class = cls,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            cls.Cards.Add(link);
            card.Class.Add(link);

            ClassCard stored = Assert.Single(cls.Cards);
            Assert.Equal(cardId, stored.CardId);
            Assert.Same(card, stored.Card);
            Assert.Equal(classId, stored.ClassId);
            Assert.Same(cls, stored.Class);
        }
    }
}
