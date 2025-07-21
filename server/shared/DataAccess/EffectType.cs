using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class EffectType : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; }
        
        public ICollection<EffectCard> EffectCards { get; set; }
    }
}
