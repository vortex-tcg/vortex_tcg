namespace VortexTCG.Auth.DTOs
{
    /// <summary>
    /// DTO utilisé pour l’inscription d’un nouvel utilisateur.
    /// </summary>
    /// <remarks>
    /// Contient toutes les informations nécessaires pour créer un compte :
    /// prénom, nom, nom d’utilisateur, email et mot de passe avec confirmation.
    /// Ce DTO est envoyé depuis le client lors d’une requête POST vers le <see cref="Controllers.RegisterController"/>.
    /// </remarks>
    public class UserRegisterDTO
    {
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
        /// Mot de passe choisi par l’utilisateur.
        /// </summary>
        /// <example>Password1!</example>
        public string password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation du mot de passe pour vérification.
        /// </summary>
        /// <example>Password1!</example>
        public string password_confirmation { get; set; } = string.Empty;
    }
}
