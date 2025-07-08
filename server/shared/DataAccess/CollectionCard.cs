using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class CollectionCard : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }

        public int CardId { get; set; }
        public Card Card { get; set; }

        public ICollection<Collection> Collections { get; set; }

    }
}