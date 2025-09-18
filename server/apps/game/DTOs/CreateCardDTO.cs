using System.ComponentModel.DataAnnotations;

namespace Collection.DTOs
{
    public class CreateCardDTO
    {
        [Required(ErrorMessage = "Name est requis")]
        [MaxLength(100, ErrorMessage = "Name ne peut pas dépasser 100 caractères")]
    public string name { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Hp doit être entre 0 et 100")]
    public int hp { get; set; }

        [Range(0, 100, ErrorMessage = "Attack doit être entre 0 et 100")]
    public int attack { get; set; }

        [Range(0, 50, ErrorMessage = "Cost doit être entre 0 et 50")]
    public int cost { get; set; }   

        [Required(ErrorMessage = "Description est requis")]
        [MaxLength(200, ErrorMessage = "Description ne peut pas dépasser 200 caractères")]
    public string description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Picture URL est requis")]
        [Url(ErrorMessage = "Picture doit être une URL valide")]
    public string picture { get; set; } = string.Empty;

        [Required(ErrorMessage = "Effect_active est requis")]
    public int effect_active { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CardTypeId doit être un entier positif")]
    public int card_type_id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "RarityId doit être un entier positif")]
    public int rarity_id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "ExtensionId doit être un entier positif")]
        public int extension_id { get; set; }
    }
}