using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Condition : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Label { get; set; } = default!;

        public string ConditionDescription { get; set; } = default!;

        public Guid ConditionTypeId { get; set; } = default!;
        public ConditionType ConditionType { get; set; } = default!;

        public ICollection<Effect> StartEffects { get; set; } = default!;

        public ICollection<Effect> EndEffects { get; set; } = default!;
    }

    public class ConditionType : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Label { get; set; } = default!;

        public ICollection<Condition> StartConditions { get; set; } = default!;

        public ICollection<Condition> EndConditions { get; set; } = default!;
    }
}