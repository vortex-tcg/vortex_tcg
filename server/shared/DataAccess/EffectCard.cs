using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class EffectCard : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; } = default!;

        public string Parameter { get; set; } = default!;

        public bool IsEquipementEffect { get; set; } = default!;

        public int EffectTypeId { get; set; } = default!;
        public EffectType EffectType { get; set; } = default!;

        public int? CardId { get; set; } = default!;
         public Card? Cards { get; set; } = default!;

        public ICollection<EffectDescription> EffectDescription { get; set; } = default!;

        public ICollection<Condition> Condition { get; set; } = default!;

    }
}