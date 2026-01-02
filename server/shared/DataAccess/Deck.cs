using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Deck : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Label { get; set; } = default!;

        public Guid UserId { get; set; } = Guid.Empty;
        public User Users { get; set; } = default!;

        public ICollection<DeckCard> DeckCard { get; set; } = default!;

        public Guid ChampionId { get; set; } = Guid.Empty;
        public Champion Champion { get; set; } = default!;

        public Guid FactionId { get; set; } = Guid.Empty;
        public Faction Faction { get; set; } = default!;
    }

    public class DeckCard : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public int Quantity { get; set; } = default!;

        public Guid CardId { get; set; } = Guid.Empty;
        public CollectionCard Card { get; set; } = default!;

        public Guid DeckId { get; set; } = Guid.Empty;
        public Deck Deck { get; set; } = default!;
    }
}