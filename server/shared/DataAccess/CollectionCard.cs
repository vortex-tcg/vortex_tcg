using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
<<<<<<< HEAD
    public class CollectionCard : AuditableEntity
=======
    public class CollectionCard
>>>>>>> da57e9f14c6f0f6a6350ee5d614b5fd04eb6296c
    {
        [Key]
        public int Id { get; set; }

<<<<<<< HEAD
=======
        public string Label { get; set; }

>>>>>>> da57e9f14c6f0f6a6350ee5d614b5fd04eb6296c
        public int Quantity { get; set; }

        public int Price { get; set; }

        public int CardId { get; set; }
        public Card Card { get; set; }

        public ICollection<Collection> Collections { get; set; }

    }
}