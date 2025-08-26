using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class ConditionType : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; } = default!;

        public ICollection<Condition> Conditions { get; set; } = default!;
    }
}
