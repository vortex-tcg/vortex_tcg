using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Deck : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; } = default!;

        public int? UserId { get; set; } = default!;
        public User? Users { get; set; } = default!;

        public ICollection<DeckCard> DeckCard { get; set; } = default!;
    }
}