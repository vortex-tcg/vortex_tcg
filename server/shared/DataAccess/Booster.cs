using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Booster : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public int Price { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int? CardId { get; set; }
        public Card? Cards { get; set; }
    }
}