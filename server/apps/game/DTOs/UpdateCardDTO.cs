using System.ComponentModel.DataAnnotations;

namespace Collection.DTOs
{
    /// <summary>
    /// Objet de transfert de données pour mettre à jour une carte existante.
    /// </summary>
    public class UpdateCardDTO
    {
        /// <summary>
        /// Nom de la carte.
        /// </summary>
        [Required(ErrorMessage = "Name est requis")]
        [MaxLength(100, ErrorMessage = "Name ne peut pas dépasser 100 caractères")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Points de vie de la carte.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Hp doit être entre 0 et 100")]
        public int Hp { get; set; }

        /// <summary>
        /// Points d'attaque de la carte.
        /// </summary>
        [Range(0, 100, ErrorMessage = "Attack doit être entre 0 et 100")]
        public int Attack { get; set; }

        /// <summary>
        /// Coût de la carte.
        /// </summary>
        [Range(0, 50, ErrorMessage = "Cost doit être entre 0 et 50")]
        public int Cost { get; set; }   

        /// <summary>
        /// Description de la carte.
        /// </summary>
        [Required(ErrorMessage = "Description est requis")]
        [MaxLength(200, ErrorMessage = "Description ne peut pas dépasser 200 caractères")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// URL de l'image associée à la carte.
        /// </summary>
        [Required(ErrorMessage = "Picture URL est requis")]
        [Url(ErrorMessage = "Picture doit être une URL valide")]
        public string Picture { get; set; } = string.Empty;

        /// <summary>
        /// Indique si un effet est actif sur la carte.
        /// </summary>
        [Required(ErrorMessage = "Effect_active est requis")]
        public int Effect_active { get; set; }

        /// <summary>
        /// Identifiant du type de carte.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "CardTypeId doit être un entier positif")]
        public int CardTypeId { get; set; }

        /// <summary>
        /// Identifiant de la rareté de la carte.
        /// </summary>
        [Range(1,int.MaxValue, ErrorMessage = "RarityId doit être un entier positif")]
        public int RarityId { get; set; }

        /// <summary>
        /// Identifiant de l'extension de la carte.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ExtensionId doit être un entier positif")]
        public int ExtensionId { get; set; }
    }
}