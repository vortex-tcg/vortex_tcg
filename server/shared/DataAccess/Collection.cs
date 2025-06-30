using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Collection
    {
        [Key]
        public int Id { get; set; }

        public string Label { get; set; }

        public int CollectionCardId { get; set; }

        public CollectionCard CollectionCard { get; set; }

        public ICollection<User> Users { get; set; }

        public ICollection<DeckCard> DeckCard { get; set; }

        public int? ChampionId { get; set; }
        public Champion? Champions { get; set; }

    }
}