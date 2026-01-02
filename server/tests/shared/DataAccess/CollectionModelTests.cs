using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class CollectionModelTests
    {
        [Fact]
        public void Collection_WithCardsAndChampions()
        {
            DateTime now = DateTime.UtcNow;
            Guid collectionId = Guid.NewGuid();
            Guid cardId = Guid.NewGuid();
            Guid championId = Guid.NewGuid();

            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Username = "jdoe",
                Password = "pwd",
                Email = "j@d.com",
                Language = "fr",
                Role = Role.USER,
                Status = UserStatus.CONNECTED,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            Collection collection = new Collection
            {
                Id = collectionId,
                User = user,
                Cards = new List<CollectionCard>(),
                Champions = new List<CollectionChampion>(),
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            Card card = new Card
            {
                Id = cardId,
                Name = "Shield",
                Price = 1,
                Hp = 5,
                Attack = 0,
                Cost = 1,
                Description = "Block",
                Picture = "shield.png",
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

            Champion champion = new Champion
            {
                Id = championId,
                Name = "Knight",
                Description = "Tank",
                HP = 40,
                Picture = "knight.png",
                FactionId = Guid.NewGuid(),
                Faction = new FactionModel { Id = Guid.NewGuid(), Label = "Order", Currency = "Honor", Condition = "Brave", CreatedAtUtc = now, CreatedBy = "seed", Cards = new List<FactionCard>(), Decks = new List<Deck>(), Champions = new List<Champion>() },
                EffectId = Guid.NewGuid(),
                Effect = new Effect { Id = Guid.NewGuid(), Title = "Guard", Parameter = "1", EffectTypeId = Guid.NewGuid(), EffectType = new EffectType { Id = Guid.NewGuid(), Label = "Buff", CreatedAtUtc = now, CreatedBy = "seed", Effect = new List<Effect>() }, EffectDescriptionId = Guid.NewGuid(), EffectDescription = new EffectDescription { Id = Guid.NewGuid(), Label = "Guard", Description = "Guard", Parameter = null, CreatedAtUtc = now, CreatedBy = "seed", Effects = new List<Effect>() }, StartConditionId = Guid.NewGuid(), StartCondition = new Condition { Id = Guid.NewGuid(), Label = "Start", ConditionDescription = "Start", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "A", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() }, EndConditionId = Guid.NewGuid(), EndCondition = new Condition { Id = Guid.NewGuid(), Label = "End", ConditionDescription = "End", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "B", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() }, CreatedAtUtc = now, CreatedBy = "seed", Cards = new List<EffectCard>() },
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Collections = new List<CollectionChampion>()
            };

            CollectionCard collectionCard = new CollectionCard
            {
                Id = Guid.NewGuid(),
                Quantity = 2,
                Rarity = Rarity.RARE,
                DeckCards = new List<DeckCard>(),
                CardId = cardId,
                Card = card,
                CollectionId = collectionId,
                Collection = collection,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            CollectionChampion collectionChampion = new CollectionChampion
            {
                Id = Guid.NewGuid(),
                ChampionId = championId,
                Champion = champion,
                CollectionId = collectionId,
                Collection = collection,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            collection.Cards.Add(collectionCard);
            collection.Champions.Add(collectionChampion);

            CollectionCard storedCard = Assert.Single(collection.Cards);
            Assert.Equal(cardId, storedCard.CardId);
            Assert.Same(card, storedCard.Card);
            Assert.Equal(collectionId, storedCard.CollectionId);
            Assert.Same(collection, storedCard.Collection);
            Assert.Equal(Rarity.RARE, storedCard.Rarity);
            CollectionChampion storedChampion = Assert.Single(collection.Champions);
            Assert.Equal(championId, storedChampion.ChampionId);
            Assert.Same(champion, storedChampion.Champion);
        }
    }
}
