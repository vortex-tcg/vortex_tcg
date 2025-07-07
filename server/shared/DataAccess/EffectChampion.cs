using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class EffectChampion
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public int Cost { get; set; }

        public ICollection<Champion> Champions { get; set; }
    }
}