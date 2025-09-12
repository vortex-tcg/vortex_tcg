using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Champion : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;

        public int HP { get; set; } = default!;

        public string Picture { get; set; } = default!;

        public int FactionId { get; set; } = default!;
        public Faction Faction { get; set; } = default!;

        public int EffectChampionId { get; set; } = default!;
        public EffectChampion EffectChampion { get; set; } = default!;

        public ICollection<Collection> Collections { get; set; } = default!;

    }
}