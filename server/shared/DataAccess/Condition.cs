using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Condition : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; } = default!;

        public string ConditionDescription { get; set; } = default!;

        public int? ConditionTypeId { get; set; } = default!;
        public ConditionType? ConditionType { get; set; } = default!;
    }
}