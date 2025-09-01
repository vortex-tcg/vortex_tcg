using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class CollectionCard : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; } = default!;

        public int Price { get; set; } = default!;

        public int CardId { get; set; } = default!;
        public Card Card { get; set; } = default!;

        public ICollection<Collection> Collections { get; set; } = default!;

    }
}