using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Gamelog : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; } = default!;

        public int? UserId { get; set; } = default!;
        public User? Users { get; set; } = default!;
        public int ActionTypeId { get; set; } = default!;
        public ActionType ActionType { get; set; } = default!;
    }
}