using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class DeckCard : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; } = default!;

        public int? CollectionId { get; set; } = default!;
        public Collection? Collection { get; set; } = default!;

        public int? DeckId { get; set; } = default!;
        public Deck? Decks { get; set; } = default!;
    }
}