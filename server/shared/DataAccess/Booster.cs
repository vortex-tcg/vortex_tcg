using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Booster : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Label { get; set; } = default!;

        public float Price { get; set; } = default!;

        public ICollection<BoosterCard> Cards { get; set; } = default!;
    }

    public class BoosterCard : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public float Probability { get; set; } = default!;

        public Guid CardId { get; set; } = Guid.Empty;
        public Card Card { get; set; } = default!;

        public Guid BoosterId { get; set; } = Guid.Empty!;
        public Booster Booster { get; set; } = default!;
    }
}