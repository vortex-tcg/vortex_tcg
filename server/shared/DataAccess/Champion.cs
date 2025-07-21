using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Champion : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int HP { get; set; }

        public string Picture { get; set; }

        public int FactionId { get; set; }
        public Faction Faction { get; set; }

        public int EffectChampionId { get; set; }
        public EffectChampion EffectChampion { get; set; }

        public ICollection<Collection> Collections { get; set; }

    }
}