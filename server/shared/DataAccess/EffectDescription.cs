using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class EffectDescription
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public int? EffectCardId { get; set; }
        public EffectCard? EffectCards { get; set; }
    }
}