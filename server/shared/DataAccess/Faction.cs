using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Faction : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; } = default!;

        public int Currency { get; set; } = default!;

        public string Condition { get; set; } = default!;

        public int? CardId { get; set; } = default!;
        public Card? Cards { get; set; } = default!;

        public ICollection<Champion> Champions { get; set; } = default!;
    }
}