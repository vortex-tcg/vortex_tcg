using System;
using System.Collections.Generic;
using VortexTCG.DataAccess.Models;
using FactionModel = VortexTCG.DataAccess.Models.Faction;
using Xunit;

namespace VortexTCG.Tests.DataAccess
{
    public class DeckModelTests
    {
        [Fact]
        public void Deck_WithDeckCard()
        {
            DateTime now = DateTime.UtcNow;
            Guid deckId = Guid.NewGuid();
            Guid collectionCardId = Guid.NewGuid();

            User user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Ana",
                LastName = "Bell",
                Username = "abell",
                Password = "pwd",
                Email = "a@b.com",
                Language = "en",
                Role = Role.USER,
                Status = UserStatus.CONNECTED,
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Friends = new List<Friend>(),
                OtherFriends = new List<Friend>()
            };

            FactionModel faction = new FactionModel
            {
                Id = Guid.NewGuid(),
                Label = "Water",
                Currency = "Drop",
                Condition = "Soak",
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Cards = new List<FactionCard>(),
                Decks = new List<Deck>(),
                Champions = new List<Champion>()
            };

            Champion champion = new Champion
            {
                Id = Guid.NewGuid(),
                Name = "Wave",
                Description = "Flow",
                HP = 20,
                Picture = "wave.png",
                FactionId = faction.Id,
                Faction = faction,
                EffectId = Guid.NewGuid(),
                Effect = new Effect { Id = Guid.NewGuid(), Title = "Splash", Parameter = "1", EffectTypeId = Guid.NewGuid(), EffectType = new EffectType { Id = Guid.NewGuid(), Label = "Type", CreatedAtUtc = now, CreatedBy = "seed", Effect = new List<Effect>() }, EffectDescriptionId = Guid.NewGuid(), EffectDescription = new EffectDescription { Id = Guid.NewGuid(), Label = "Splash", Description = "Splash", Parameter = null, CreatedAtUtc = now, CreatedBy = "seed", Effects = new List<Effect>() }, StartConditionId = Guid.NewGuid(), StartCondition = new Condition { Id = Guid.NewGuid(), Label = "Start", ConditionDescription = "start", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "T", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() }, EndConditionId = Guid.NewGuid(), EndCondition = new Condition { Id = Guid.NewGuid(), Label = "End", ConditionDescription = "end", ConditionTypeId = Guid.NewGuid(), ConditionType = new ConditionType { Id = Guid.NewGuid(), Label = "T2", CreatedAtUtc = now, CreatedBy = "seed", Conditions = new List<Condition>() }, CreatedAtUtc = now, CreatedBy = "seed", StartEffects = new List<Effect>(), EndEffects = new List<Effect>() }, CreatedAtUtc = now, CreatedBy = "seed", Cards = new List<EffectCard>(), Champion = new Champion { Id = Guid.NewGuid(), Name = "Brute", Description = "", HP = 10, Picture = "", FactionId = Guid.NewGuid(), Faction = new FactionModel { Id = Guid.NewGuid(), Label = "F", Currency = "C", Condition = "Cond", CreatedAtUtc = now, CreatedBy = "seed", Cards = new List<FactionCard>(), Decks = new List<Deck>(), Champions = new List<Champion>() }, EffectId = Guid.NewGuid(), CreatedAtUtc = now, CreatedBy = "seed", Collections = new List<CollectionChampion>() } },
                CreatedAtUtc = now,
                CreatedBy = "seed",
                Collections = new List<CollectionChampion>()
            };

            CollectionCard collectionCard = new CollectionCard
            {
                Id = collectionCardId,
                Quantity = 3,
                Rarity = Rarity.EPIC,
                DeckCards = new List<DeckCard>(),
                CardId = Guid.NewGuid(),
                Card = new Card { Id = Guid.NewGuid(), Name = "WaveCard", Price = 1, Hp = 1, Attack = 1, Cost = 1, Description = "d", Picture = "p", Extension = Extension.BASIC, CardType = CardType.SPELL, CreatedAtUtc = now, CreatedBy = "seed", Collections = new List<CollectionCard>(), Class = new List<ClassCard>(), Boosters = new List<BoosterCard>(), Factions = new List<FactionCard>(), Effect = new List<EffectCard>() },
                CollectionId = Guid.NewGuid(),
                Collection = new Collection { Id = Guid.NewGuid(), User = user, Cards = new List<CollectionCard>(), Champions = new List<CollectionChampion>(), CreatedAtUtc = now, CreatedBy = "seed" },
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            Deck deck = new Deck
            {
                Id = deckId,
                Label = "WaterDeck",
                UserId = user.Id,
                Users = user,
                ChampionId = champion.Id,
                Champion = champion,
                FactionId = faction.Id,
                Faction = faction,
                DeckCard = new List<DeckCard>(),
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            DeckCard deckCard = new DeckCard
            {
                Id = Guid.NewGuid(),
                Quantity = 2,
                CardId = collectionCardId,
                Card = collectionCard,
                DeckId = deckId,
                Deck = deck,
                CreatedAtUtc = now,
                CreatedBy = "seed"
            };

            deck.DeckCard.Add(deckCard);
            collectionCard.DeckCards.Add(deckCard);

            DeckCard stored = Assert.Single(deck.DeckCard);
            Assert.Equal(collectionCardId, stored.CardId);
            Assert.Same(collectionCard, stored.Card);
            Assert.Equal(deckId, stored.DeckId);
            Assert.Same(deck, stored.Deck);
            Assert.Equal(2, stored.Quantity);
        }
    }
}
