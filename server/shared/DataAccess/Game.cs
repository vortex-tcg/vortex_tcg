using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Game : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Status { get; set; } = default!;

        public int TurnNumber { get; set; } = default!;

        public ICollection<User> User { get; set; } = default!;
    }
}
