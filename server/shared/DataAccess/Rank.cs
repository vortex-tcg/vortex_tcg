using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Rank : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Label { get; set; } = default!;

        [Required]
        public int nbVictory { get; set; } = default!;

        public ICollection<User> Users { get; set; } = default!;
    }
}
