using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Cards.DTOs
{
    /// <summary>
    /// Objet de transfert de données pour créer une nouvelle carte.
    /// </summary>
    public class CreateCardDTO
    {
        /// <summary>
        /// Nom de la carte.
        /// </summary>
        [Required(ErrorMessage = "Name est requis")]
        [MaxLength(100, ErrorMessage = "Name ne peut pas dépasser 100 caractères")]
        public string name { get; set; } = string.Empty;

        /// <summary>
        /// Points de vie de la carte.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Hp doit être entre 0 et 100")]
        public int hp { get; set; }

        /// <summary>
        /// Points d'attaque de la carte.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Attack doit être entre 0 et 100")]
        public int attack { get; set; }

        /// <summary>
        /// Coût de la carte.
        /// </summary>
        [Range(0, 50, ErrorMessage = "Cost doit être entre 0 et 50")]
        public int cost { get; set; }

        /// <summary>
        /// Description de la carte.
        /// </summary>
        [Required(ErrorMessage = "Description est requis")]
        [MaxLength(200, ErrorMessage = "Description ne peut pas dépasser 200 caractères")]
        public string description { get; set; } = string.Empty;

        /// <summary>
        /// URL de l'image associée à la carte.
        /// </summary>
        [Required(ErrorMessage = "Picture URL est requis")]
        [Url(ErrorMessage = "Picture doit être une URL valide")]
        public string picture { get; set; } = string.Empty;

        /// <summary>
        /// Indique si un effet est actif sur la carte.
        /// </summary>
        [Required(ErrorMessage = "Effect_active est requis")]
        public int effect_active { get; set; }

        /// <summary>
        /// Identifiant du type de carte.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "CardTypeId doit être un entier positif")]
        public int card_type_id { get; set; }

        /// <summary>
        /// Identifiant de la rareté de la carte.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "RarityId doit être un entier positif")]
        public int rarity_id { get; set; }

        /// <summary>
        /// Identifiant de l'extension de la carte.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ExtensionId doit être un entier positif")]
        public int extension_id { get; set; }
    }
}