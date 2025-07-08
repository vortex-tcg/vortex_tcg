using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
<<<<<<< HEAD
    public class EffectCard : AuditableEntity
=======
    public class EffectCard
>>>>>>> da57e9f14c6f0f6a6350ee5d614b5fd04eb6296c
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Parameter { get; set; }

        public bool IsEquipementEffect { get; set; }

        public int EffectTypeId { get; set; }
        public EffectType EffectType { get; set; }

        public int? CardId { get; set; }
        public Card? Cards { get; set; }

        public ICollection<EffectDescription> EffectDescription { get; set; }

        public ICollection<Condition> Condition { get; set; }

    }
}