using System.ComponentModel.DataAnnotations;

namespace VortexTCG.Auth.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string PasswordConfirmation { get; set; } = string.Empty;
    }
}
