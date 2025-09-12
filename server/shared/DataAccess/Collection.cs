using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Collection : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public int CollectionCardId { get; set; } = default!;

        public CollectionCard CollectionCard { get; set; } = default!;

        public ICollection<User> Users { get; set; } = default!;

        public ICollection<DeckCard> DeckCard { get; set; } = default!;

        public int? ChampionId { get; set; } = default!;
        public Champion? Champions { get; set; } = default!;

    }
}