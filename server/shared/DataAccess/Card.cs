using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public enum Extension { BASIC = 0 };
    public enum CardType { GUARD = 0, SPELL = 1, EQUIPMENT = 2 };

    public class Card : AuditableEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public int Price { get; set; } = default!;  // price to craft the card

        public int? Hp { get; set; } = default!;

        public int? Attack { get; set; } = default!;

        public int Cost { get; set; } = default!;   // cost of the card to play

        public string Description { get; set; } = default!;

        public string Picture { get; set; } = default!;

        public Extension Extension { get; set; } = default!;
        public CardType CardType { get; set; } = default!;


        public ICollection<CollectionCard> Collections { get; set; } = default!;

        public ICollection<ClassCard> Class { get; set; } = default!;

        public ICollection<BoosterCard> Boosters { get; set; } = default!;

        public ICollection<FactionCard> Factions { get; set; } = default!;

        public ICollection<EffectCard> Effect { get; set; } = default!;
    }
}