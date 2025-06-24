using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        public string Status { get; set; }

        public int TurnNumber { get; set; }

        public ICollection<User> User { get; set; }
    }
}
