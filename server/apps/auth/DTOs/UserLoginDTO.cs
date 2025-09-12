namespace VortexTCG.Auth.DTOs
{
    /// <summary>
    /// DTO utilisé pour la connexion d’un utilisateur.
    /// </summary>
    /// <remarks>
    /// Contient uniquement les informations nécessaires pour s’authentifier :
    /// l’email et le mot de passe.
    /// Ce DTO est envoyé depuis le client lors d’une requête POST vers le <see cref="Controllers.LoginController"/>.
    /// </remarks>
    public class UserLoginDTO
    {
        /// <summary>
        /// Adresse email de l’utilisateur.
        /// </summary>
        /// <example>john.doe@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mot de passe de l’utilisateur.
        /// </summary>
        /// <example>Password1!</example>
        public string Password { get; set; } = string.Empty;
    }
}
