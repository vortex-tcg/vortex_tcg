using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class DeckCard : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; }

        public int? CollectionId { get; set; }
        public Collection? Collection { get; set; }

        public int? DeckId { get; set; }
        public Deck? Decks { get; set; }
    }
}