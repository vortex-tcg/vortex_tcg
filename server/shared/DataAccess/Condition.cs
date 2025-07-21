using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Condition : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public string ConditionDescription { get; set; }

        public int? ConditionTypeId { get; set; }
        public ConditionType? ConditionType { get; set; }
    }
}