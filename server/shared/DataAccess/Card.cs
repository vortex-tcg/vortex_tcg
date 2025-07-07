using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VortexTCG.DataAccess.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Hp { get; set; }

        public int Attack { get; set; }

        public int Cost { get; set; }

        public string Description { get; set; }

        public string Picture { get; set; }

        public int Effect_active { get; set; }

        public int RarityId { get; set; }
        public Rarity Rarity { get; set; }
        public int ExtensionId { get; set; }
        public Extension Extension { get; set; }
        public int CardTypeId { get; set; }
        public CardType CardType { get; set; }


        public ICollection<CollectionCard> CollectionCards { get; set; }

        public ICollection<Class> Class { get; set; }

        public ICollection<Booster> Booster { get; set; }

        public ICollection<Faction> Faction { get; set; }

        public ICollection<EffectCard> EffectCard { get; set; }
    }
}