using System;
using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Api.Champion.DTOs
{
    /// <summary>
    /// DTO Champion - Objet de transfert de données retourné par l'API.
    /// Contient toutes les informations d'un champion existant incluant son ID.
    /// </summary>
	public class ChampionDto
	{
		/// <summary>L'identifiant unique du champion.</summary>
		public Guid Id { get; set; }
		/// <summary>Le nom du champion.</summary>
		public string Name { get; set; } = default!;
		/// <summary>La description du champion.</summary>
		public string Description { get; set; } = default!;
        /// <summary>Les points de vie du champion.</summary>
        public int HP { get; set; } 
        /// <summary>L'URL ou le chemin de l'image du champion.</summary>
        public string Picture { get; set; } = default!;
        /// <summary>L'identifiant de la faction du champion.</summary>
        public Guid FactionId { get; set; }
        /// <summary>L'identifiant de l'effet spécial du champion.</summary>
        public Guid EffectId { get; set; }
	}

    /// <summary>
    /// DTO Création Champion - Objet de transfert de données utilisé pour créer ou mettre à jour un champion.
    /// Ne contient pas l'identifiant qui est généré par le serveur.
    /// </summary>
	public class ChampionCreateDto
	{
		/// <summary>Le nom du champion (obligatoire).</summary>
        [Required]
		public string Name { get; set; } = default!;
		/// <summary>La description du champion.</summary>
        [Required]
		public string Description { get; set; } = default!;
        /// <summary>Les points de vie du champion.</summary>
        [Required]
        public int? HP { get; set; }
        /// <summary>L'URL ou le chemin de l'image du champion.</summary>
        [Required]
        public string Picture { get; set; } = default!;
        /// <summary>L'identifiant de la faction du champion.</summary>
        [Required]
        public Guid? FactionId { get; set; }
        /// <summary>L'identifiant de l'effet spécial du champion.</summary>
        [Required]
        public Guid? EffectId { get; set; }
	}
}