namespace VortexTCG.Auth.DTOs
{
    /// <summary>
    /// DTO utilisé pour renvoyer les informations d’un utilisateur après l’inscription ou la connexion.
    /// </summary>
    /// <remarks>
    /// Ce DTO contient les informations publiques de l’utilisateur,
    /// ainsi que le token JWT pour l’authentification et son rôle.
    /// Il est renvoyé par le <see cref="Controllers.RegisterController"/> et le <see cref="Controllers.LoginController"/>.
    /// </remarks>
    public class UserResponseDTO
    {
        /// <summary>
        /// Identifiant unique de l’utilisateur en base.
        /// </summary>
        /// <example>1</example>
        public int id { get; set; }

        /// <summary>
        /// Nom d’utilisateur choisi par l’utilisateur.
        /// </summary>
        /// <example>johndoe</example>
        public string username { get; set; } = string.Empty;

        /// <summary>
        /// Adresse email de l’utilisateur.
        /// </summary>
        /// <example>john.doe@example.com</example>
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// Prénom de l’utilisateur.
        /// </summary>
        /// <example>John</example>
        public string first_name { get; set; } = string.Empty;

        /// <summary>
        /// Nom de famille de l’utilisateur.
        /// </summary>
        /// <example>Doe</example>
        public string last_name { get; set; } = string.Empty;

        /// <summary>
        /// Token JWT permettant l’authentification de l’utilisateur.
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        public string token { get; set; } = string.Empty;

        /// <summary>
        /// Rôle de l’utilisateur dans le système (ex: User, Admin).
        /// </summary>
        /// <example>User</example>
        public string role { get; set; } = string.Empty;

        /// <summary>
        /// Langue préférée de l’utilisateur.
        /// </summary>
        /// <example>fr</example>
        public string language { get; set; } = "fr";

        /// <summary>
        /// Quantité de monnaie ou points virtuels possédés par l’utilisateur.
        /// </summary>
        /// <example>0</example>
        public int currency_quantity { get; set; } = 0;
    }
}
