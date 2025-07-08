using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
<<<<<<< HEAD
    public class Faction : AuditableEntity
=======
    public class Faction
>>>>>>> da57e9f14c6f0f6a6350ee5d614b5fd04eb6296c
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public int Currency { get; set; }

        public string Condition { get; set; }

        public int? CardId { get; set; }
        public Card? Cards { get; set; }

        public ICollection<Champion> Champions { get; set; }
    }
}