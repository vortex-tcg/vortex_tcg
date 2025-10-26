using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Auth.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string first_name { get; set; } = string.Empty;

        [Required]
        public string last_name { get; set; } = string.Empty;

        [Required]
        public string username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        [Required]
        [Compare("password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string password_confirmation { get; set; } = string.Empty;
    }
}
