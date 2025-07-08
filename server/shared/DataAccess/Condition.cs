using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
<<<<<<< HEAD
    public class Condition : AuditableEntity
=======
    public class Condition
>>>>>>> da57e9f14c6f0f6a6350ee5d614b5fd04eb6296c
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public string ConditionDescription { get; set; }

        public int? ConditionTypeId { get; set; }
        public ConditionType? ConditionType { get; set; }
    }
}