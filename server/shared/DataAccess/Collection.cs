using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public enum Rarity { NORMAL = 0, RARE = 1, EPIC = 2, LEGENDARY = 3 };

    public class Collection : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public User User { get; set; } = default!;

        public ICollection<CollectionCard> Cards { get; set; } = default!;
        public ICollection<CollectionChampion> Champions { get; set; } = default!;

    }

    public class CollectionCard : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public int Quantity { get; set; } = default!;

        public Rarity Rarity { get; set; } = default!;

        public ICollection<DeckCard> DeckCards { get; set; } = default!;

        public Guid CardId { get; set; } = Guid.Empty;
        public Card Card { get; set; } = default!;

        public Guid CollectionId { get; set; } = Guid.Empty;
        public Collection Collection { get; set; } = default!;

    }

    public class CollectionChampion : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ChampionId { get; set; } = Guid.Empty;
        public Champion Champion { get; set; } = default!;

        public Guid CollectionId { get; set; } = Guid.Empty;
        public Collection Collection { get; set; } = default!;
    }
}