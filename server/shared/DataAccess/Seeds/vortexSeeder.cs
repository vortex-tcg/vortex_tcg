using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;
using VortexClass = VortexTCG.DataAccess.Models.Class;

namespace VortexTCG.DataAccess.Seeds
{
    public sealed class VortexSeeder
    {
        private readonly VortexDbContext _db;
        private readonly string _actor;
        private readonly DateTime _utcNow;

        public VortexSeeder(VortexDbContext db, string actor = "Seeder", DateTime? utcNow = null)
        {
            _db = db;
            _actor = actor;
            _utcNow = utcNow ?? DateTime.UtcNow;
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        private void StampNew(AuditableEntity entity)
        {
            entity.CreatedAtUtc = _utcNow;
            entity.CreatedBy = _actor;
            entity.UpdatedAtUtc = _utcNow;
            entity.UpdatedBy = _actor;
        }

        private static string Key(Guid a, Guid b)
        {
            return $"{a:N}:{b:N}";
        }

        public Guid EnsureFaction(Guid preferredId, string label, string currency, string condition)
        {
            Faction existing = _db.Set<Faction>()
                .AsNoTracking()
                .FirstOrDefault(f => f.Label == label);

            if (existing != null)
            {
                return existing.Id;
            }

            Faction entity = new Faction
            {
                Id = preferredId,
                Label = label,
                Currency = currency,
                Condition = condition
            };

            StampNew(entity);
            _db.Set<Faction>().Add(entity);

            return preferredId;
        }
        public void SeedDeckForUser(
            VortexDbContext db,
            string email,
            string deckName,
            Guid factionId,
            Guid championId,
            Guid[] cardIds,
            DateTime utcDate,
            string actor
        )
        {
            User? user = db.Users.SingleOrDefault(u => u.Email == email);
            if (user == null) return;
            if (user.CollectionId == null) return;

            Guid collectionId = user.CollectionId.Value;

            Deck deck = EnsureDeck(db, user.Id, deckName, factionId, championId, utcDate, actor);

            foreach (Guid cardId in cardIds)
            {
                CollectionCard? cc = db.Set<CollectionCard>()
                    .SingleOrDefault(x =>
                        x.CollectionId == collectionId &&
                        x.CardId == cardId &&
                        x.Rarity == Rarity.NORMAL
                    );

                if (cc == null) continue; // normalement jamais si la starter collection a été seed

                EnsureDeckCard(db, deck.Id, cc.Id, 1, utcDate, actor);
            }
        }
      private static Deck EnsureDeck(
        VortexDbContext db,
        Guid userId,
        string deckName,
        Guid factionId,
        Guid championId,
        DateTime utcDate,
        string actor
    )
    {
        Deck? existing = db.Set<Deck>()
            .SingleOrDefault(d => d.UserId == userId && d.Label == deckName);

        if (existing != null)
        {
            bool changed = false;

            if (existing.FactionId != factionId)
            {
                existing.FactionId = factionId;
                changed = true;
            }

            if (existing.ChampionId != championId)
            {
                existing.ChampionId = championId;
                changed = true;
            }

            if (changed)
            {
                existing.UpdatedAtUtc = utcDate;
                existing.UpdatedBy = actor;
            }

            return existing;
        }

        Deck created = new Deck
        {
            Id = Guid.NewGuid(),
            Label = deckName,
            UserId = userId,
            FactionId = factionId,
            ChampionId = championId,
            CreatedAtUtc = utcDate,
            CreatedBy = actor,
            UpdatedAtUtc = utcDate,
            UpdatedBy = actor
        };

        db.Set<Deck>().Add(created);
        return created;
    }

    private static void EnsureDeckCard(
        VortexDbContext db,
        Guid deckId,
        Guid collectionCardId,
        int quantity,
        DateTime utcDate,
        string actor
    )
    {
        DeckCard? existing = db.Set<DeckCard>()
            .SingleOrDefault(dc => dc.DeckId == deckId && dc.CardId == collectionCardId);

        if (existing != null)
        {
            if (existing.Quantity != quantity)
            {
                existing.Quantity = quantity;
                existing.UpdatedAtUtc = utcDate;
                existing.UpdatedBy = actor;
            }
            return;
        }

        DeckCard created = new DeckCard
        {
            Id = Guid.NewGuid(),
            DeckId = deckId,
            CardId = collectionCardId,
            Quantity = quantity,
            CreatedAtUtc = utcDate,
            CreatedBy = actor,
            UpdatedAtUtc = utcDate,
            UpdatedBy = actor
        };

        db.Set<DeckCard>().Add(created);
    }
      
        public Guid EnsureCard(
            Guid preferredId,
            string name,
            int price,
            int? hp,
            int? attack,
            int cost,
            string description,
            string picture,
            Extension extension,
            CardType cardType
        )
        {
            Card? existing = _db.Set<Card>()
                .AsNoTracking()
                .FirstOrDefault(c => c.Name == name);

            if (existing != null)
            {
                return existing.Id;
            }

            Card entity = new Card
            {
                Id = preferredId,
                Name = name,
                Price = price,
                Hp = hp,
                Attack = attack,
                Cost = cost,
                Description = description,
                Picture = picture,
                Extension = extension,
                CardType = cardType
            };

            StampNew(entity);
            _db.Set<Card>().Add(entity);

            return preferredId;
        }
   public void SeedStarterCollectionForUser(
        VortexDbContext db,
        string email,
        Guid[] championIds,
        Guid[] cardIds,
        DateTime utcDate,
        string actor
    )
    {
        User? user = db.Users.SingleOrDefault(u => u.Email == email);
        if (user == null) return;

        Collection collection = GetOrCreateCollection(db, user, utcDate, actor);

        int i;
        for (i = 0; i < championIds.Length; i++)
        {
            EnsureCollectionChampion(db, collection.Id, championIds[i], utcDate, actor);
        }

        for (i = 0; i < cardIds.Length; i++)
        {
            EnsureCollectionCard(db, collection.Id, cardIds[i], Rarity.NORMAL, 3, utcDate, actor);
        }
    }

    private static Collection GetOrCreateCollection(VortexDbContext db, User user, DateTime utcDate, string actor)
    {
        if (user.CollectionId != null)
        {
            Collection? existing = db.Set<Collection>().SingleOrDefault(c => c.Id == user.CollectionId.Value);
            if (existing != null) return existing;
        }

        Collection created = new Collection
        {
            Id = Guid.NewGuid(),
            User = user,
            Cards = new List<CollectionCard>(),
            Champions = new List<CollectionChampion>(),
            CreatedAtUtc = utcDate,
            CreatedBy = actor,
            UpdatedAtUtc = utcDate,
            UpdatedBy = actor
        };

        user.CollectionId = created.Id;

        db.Set<Collection>().Add(created);
        return created;
    }

    private static void EnsureCollectionChampion(VortexDbContext db, Guid collectionId, Guid championId, DateTime utcDate, string actor)
    {
        CollectionChampion? existing = db.Set<CollectionChampion>()
            .SingleOrDefault(x => x.CollectionId == collectionId && x.ChampionId == championId);

        if (existing != null) return;

        CollectionChampion created = new CollectionChampion
        {
            Id = Guid.NewGuid(),
            CollectionId = collectionId,
            ChampionId = championId,
            CreatedAtUtc = utcDate,
            CreatedBy = actor,
            UpdatedAtUtc = utcDate,
            UpdatedBy = actor
        };

        db.Set<CollectionChampion>().Add(created);
    }

    private static void EnsureCollectionCard(
        VortexDbContext db,
        Guid collectionId,
        Guid cardId,
        Rarity rarity,
        int quantity,
        DateTime utcDate,
        string actor
    )
    {
        CollectionCard? existing = db.Set<CollectionCard>()
            .SingleOrDefault(x => x.CollectionId == collectionId && x.CardId == cardId && x.Rarity == rarity);

        if (existing == null)
        {
            CollectionCard created = new CollectionCard
            {
                Id = Guid.NewGuid(),
                CollectionId = collectionId,
                CardId = cardId,
                Rarity = rarity,
                Quantity = quantity,
                DeckCards = new List<DeckCard>(),
                CreatedAtUtc = utcDate,
                CreatedBy = actor,
                UpdatedAtUtc = utcDate,
                UpdatedBy = actor
            };

            db.Set<CollectionCard>().Add(created);
            return;
        }

        if (existing.Quantity != quantity)
        {
            existing.Quantity = quantity;
            existing.UpdatedAtUtc = utcDate;
            existing.UpdatedBy = actor;
        }
    }
        public Guid EnsureFactionCard(Guid preferredId, Guid cardId, Guid factionId)
        {
            FactionCard? existing = _db.Set<FactionCard>()
                .AsNoTracking()
                .FirstOrDefault(fc => fc.CardId == cardId && fc.FactionId == factionId);

            if (existing != null)
            {
                return existing.Id;
            }

            FactionCard entity = new FactionCard
            {
                Id = preferredId,
                CardId = cardId,
                FactionId = factionId
            };

            StampNew(entity);
            _db.Set<FactionCard>().Add(entity);

            return preferredId;
        }

        public Guid EnsureClassCard(Guid preferredId, Guid cardId, Guid classId)
        {
            ClassCard? existing = _db.Set<ClassCard>()
                .AsNoTracking()
                .FirstOrDefault(cc => cc.CardId == cardId && cc.ClassId == classId);

            if (existing != null)
            {
                return existing.Id;
            }

            ClassCard entity = new ClassCard
            {
                Id = preferredId,
                CardId = cardId,
                ClassId = classId
            };

            StampNew(entity);
            _db.Set<ClassCard>().Add(entity);

            return preferredId;
        }

        public Guid EnsureClass(Guid preferredId, string label)
        {
            VortexClass? existing = _db.Set<VortexClass>()
                .AsNoTracking()
                .FirstOrDefault(c => c.Label == label);

            if (existing != null)
            {
                return existing.Id;
            }

            VortexClass entity = new VortexClass
            {
                Id = preferredId,
                Label = label
            };

            StampNew(entity);
            _db.Set<VortexClass>().Add(entity);

            return preferredId;
        }
        
        public Guid EnsureChampion(
            Guid preferredId,
            string name,
            string description,
            int hp,
            string picture,
            Guid factionId,
            Guid? effectId
        )
        {
            Champion? existing = _db.Set<Champion>()
                .AsNoTracking()
                .FirstOrDefault(c => c.Name == name);

            if (existing != null)
            {
                return existing.Id;
            }

            Champion entity = new Champion
            {
                Id = preferredId,
                Name = name,
                Description = description,
                HP = hp,
                Picture = picture,
                FactionId = factionId,
                EffectId = effectId
            };

            StampNew(entity);
            _db.Set<Champion>().Add(entity);

            return preferredId;
        }
    }
    public sealed class CardSeedInput
    {
        public string Name { get; }
        public int Price { get; }
        public int? Hp { get; }
        public int? Attack { get; }
        public int Cost { get; }
        public string Description { get; }
        public string Picture { get; }
        public Extension Extension { get; }
        public CardType CardType { get; }

        public CardSeedInput(
            string name,
            int price,
            int? hp,
            int? attack,
            int cost,
            string description,
            string picture,
            Extension extension,
            CardType cardType
        )
        {
            Name = name;
            Price = price;
            Hp = hp;
            Attack = attack;
            Cost = cost;
            Description = description;
            Picture = picture;
            Extension = extension;
            CardType = cardType;
        }
    }

    public sealed class FactionCardSeedInput
    {
        public string CardName { get; }
        public string FactionLabel { get; }

        public FactionCardSeedInput(string cardName, string factionLabel)
        {
            CardName = cardName;
            FactionLabel = factionLabel;
        }
    }
    public sealed class ClassCardSeedInput
    {
        public Guid CardId { get; }
        public Guid ClassId { get; }

        public ClassCardSeedInput(Guid cardId, Guid classId)
        {
            CardId = cardId;
            ClassId = classId;
        }
    }
}
