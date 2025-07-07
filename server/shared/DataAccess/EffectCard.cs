using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class EffectCard
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