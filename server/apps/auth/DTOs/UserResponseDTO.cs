namespace VortexTCG.Auth.DTOs
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public string Language { get; set; } = "fr";
        public int CurrencyQuantity { get; set; } = 0;

    }
}