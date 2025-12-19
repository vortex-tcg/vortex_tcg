using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Faction : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Label { get; set; } = default!;

        public string Currency { get; set; } = default!;

        public string Condition { get; set; } = default!;

        public ICollection<FactionCard> Cards { get; set; } = default!;

        public ICollection<Deck> Decks { get; set; } = default!;

        public ICollection<Champion> Champions { get; set; } = default!;
    }

    public class FactionCard: AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CardId { get; set; } = default!;
        public Card Card { get; set; } = default!;

        public Guid FactionId { get; set; } = default!;
        public Faction Faction { get; set; } = default!;
    }
}