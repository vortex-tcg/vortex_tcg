using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
<<<<<<< HEAD
    public class EffectChampion : AuditableEntity
=======
    public class EffectChampion
>>>>>>> da57e9f14c6f0f6a6350ee5d614b5fd04eb6296c
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public string Description { get; set; }

        public int Cost { get; set; }

        public ICollection<Champion> Champions { get; set; }
    }
}