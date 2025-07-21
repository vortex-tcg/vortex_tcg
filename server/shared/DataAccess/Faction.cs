using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Faction : AuditableEntity
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