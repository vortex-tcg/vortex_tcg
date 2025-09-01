using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Rarity : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; } = default!;

        public ICollection<Card> Cards { get; set; } = default!;
    }
}
