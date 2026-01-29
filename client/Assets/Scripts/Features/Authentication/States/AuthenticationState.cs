/// <summary>
/// Énumération des états d'authentification possibles
/// </summary>
public enum AuthenticationState
{
    Unauthenticated,
    Authenticating,
    Authenticated,
    TokenExpired,
    AuthenticationFailed
}
