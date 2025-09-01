using System.ComponentModel.DataAnnotations;

namespace Collection.DTOs
{
    public class UpdateCardDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Hp must be between 0 and 100")]
        public int Hp { get; set; }

        [Range(0, 100, ErrorMessage = "Attack must be between 0 and 100")]
        public int Attack { get; set; }

        [Range(0, 50, ErrorMessage = "Cost must be between 0 and 50")]
        public int Cost { get; set; }   

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = string.Empty;

        // [Required(ErrorMessage = "Picture URL is required")]
        [Url(ErrorMessage = "Picture must be a valid URL")]
        public string Picture { get; set; } = string.Empty;

        [Required(ErrorMessage = "Effect_active is not exist")]
        public int Effect_active { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CardTypeId must be a positive integer")]
        public int CardTypeId { get; set; }
        [Range(1,int.MaxValue, ErrorMessage = "RarityId must be a positive integer")]
        public int RarityId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "ExtensionId must be a positive integer")]
        public int ExtensionId { get; set; }
    }
}