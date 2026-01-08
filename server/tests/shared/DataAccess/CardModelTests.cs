using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class CardModelTests
    {
        [Fact]
        public void Card_AllProperties_AreSet()
        {
            DateTime now = DateTime.UtcNow;
            Guid id = Guid.NewGuid();

            Card card = new Card
            {
                Id = id,
                Name = "Blade",
                Price = 5,
                Hp = 7,
                Attack = 4,
                Cost = 2,
                Description = "Sharp blade",
                Picture = "blade.png",
                Extension = Extension.BASIC,
                CardType = CardType.EQUIPMENT,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Collections = new List<CollectionCard>(),
                Class = new List<ClassCard>(),
                Boosters = new List<BoosterCard>(),
                Factions = new List<FactionCard>(),
                Effect = new List<EffectCard>()
            };

            Assert.Equal(id, card.Id);
            Assert.Equal("Blade", card.Name);
            Assert.Equal(5, card.Price);
            Assert.Equal(7, card.Hp);
            Assert.Equal(4, card.Attack);
            Assert.Equal(2, card.Cost);
            Assert.Equal("Sharp blade", card.Description);
            Assert.Equal("blade.png", card.Picture);
            Assert.Equal(Extension.BASIC, card.Extension);
            Assert.Equal(CardType.EQUIPMENT, card.CardType);
            Assert.Equal(now, card.CreatedAtUtc);
            Assert.Equal("seed", card.CreatedBy);
            Assert.Empty(card.Collections);
            Assert.Empty(card.Class);
            Assert.Empty(card.Boosters);
            Assert.Empty(card.Factions);
            Assert.Empty(card.Effect);
        }
    }
}
