using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class EffectChampion : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; } = default!;

        public string Description { get; set; } = default!;

        public int Cost { get; set; } = default!;

        public ICollection<Champion> Champions { get; set; } = default!;
    }
}