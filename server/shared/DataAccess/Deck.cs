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

        public Guid UserId { get; set; } = default!;
        public User Users { get; set; } = default!;

        public ICollection<DeckCard> DeckCard { get; set; } = default!;

        public Guid ChampionId { get; set; } = default!;
        public Champion Champion { get; set; } = default!;

        public Guid FactionId { get; set; } = default!;
        public Faction Faction { get; set; } = default!;
    }

    public class DeckCard : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public int Quantity { get; set; } = default!;

        public Guid CardId { get; set; } = default!;
        public CollectionCard Card { get; set; } = default!;

        public Guid DeckId { get; set; } = default!;
        public Deck Deck { get; set; } = default!;
    }
}