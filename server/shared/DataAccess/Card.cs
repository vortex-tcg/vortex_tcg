using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Card : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public int Hp { get; set; } = default!;

        public int Attack { get; set; } = default!;

        public int Cost { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string Picture { get; set; } = default!;

        public int Effect_active { get; set; } = default!;

        public int RarityId { get; set; } = default!;
        public Rarity Rarity { get; set; } = default!;
        public int ExtensionId { get; set; } = default!;
        public Extension Extension { get; set; } = default!;
        public int CardTypeId { get; set; } = default!;
        public CardType CardType { get; set; } = default!;


        public ICollection<CollectionCard> CollectionCards { get; set; } = default!;

        public ICollection<Class> Class { get; set; } = default!;

        public ICollection<Booster> Booster { get; set; } = default!;

        public ICollection<Faction> Faction { get; set; } = default!;

        public ICollection<EffectCard> EffectCard { get; set; } = default!;
    }
}