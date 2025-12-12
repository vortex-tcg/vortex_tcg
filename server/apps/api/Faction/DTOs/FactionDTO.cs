using VortexTCG.DataAccess.Models;
using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Faction.DTOs
{

    public class FactionDTO
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Condition { get; set; } = "";
    }

    public class FactionCardDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Price { get; set; }
        public int? Hp { get; set; }
        public int? Attack { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; } = "";
        public string Picture { get; set; } = "";
        public string Extension { get; set; } = "";
        public string CardType { get; set; } = "";
    }

    public class FactionChampionDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int HP { get; set; }
        public string Picture { get; set; } = "";
    }

    public class FactionWithCardsDTO
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Condition { get; set; } = "";
        public List<FactionCardDTO> Cards { get; set; } = new List<FactionCardDTO>();
    }

    public class FactionWithChampionDTO
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = "";
        public string Currency { get; set; } = "";
        public string Condition { get; set; } = "";
        public FactionChampionDTO? Champion { get; set; } // Un seul champion
    }

    public class CreateFactionDTO
    {
        [Required]
        [StringLength(100)]
        public string Label { get; set; } = "";
        
        [Required]
        [StringLength(50)]
        public string Currency { get; set; } = "";
        
        [StringLength(500)]
        public string Condition { get; set; } = "";
        
        // ✅ CORRECTION : Un seul champion par faction
        public List<Guid> CardIds { get; set; } = new List<Guid>();
        public Guid? ChampionId { get; set; } // Un seul champion optionnel
    }

    public class UpdateFactionDTO
    {
        [StringLength(100)]
        public string? Label { get; set; }
        
        [StringLength(50)]
        public string? Currency { get; set; }
        
        [StringLength(500)]
        public string? Condition { get; set; }

        // ✅ AJOUT : Gestion des cartes et champion
        public List<Guid>? CardIds { get; set; } // null = pas de changement, liste vide = supprimer toutes
        public Guid? ChampionId { get; set; } // null = pas de changement, Guid.Empty = supprimer champion
    }
}