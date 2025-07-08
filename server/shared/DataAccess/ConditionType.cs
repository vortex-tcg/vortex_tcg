using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
<<<<<<< HEAD
    public class ConditionType : AuditableEntity
=======
    public class ConditionType
>>>>>>> da57e9f14c6f0f6a6350ee5d614b5fd04eb6296c
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; }

        public ICollection<Condition> Conditions { get; set; }
    }
}
