using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Rank : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public int nbVictory { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
