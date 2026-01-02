using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Champion : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;

        public int HP { get; set; } = default!;

        public string Picture { get; set; } = default!;

        public Guid FactionId { get; set; } = Guid.Empty;
        public Faction Faction { get; set; } = default!;

        public Guid EffectId { get; set; } = Guid.Empty;
        public Effect Effect { get; set; } = default!;

        public ICollection<CollectionChampion> Collections { get; set; } = default!;

    }
}