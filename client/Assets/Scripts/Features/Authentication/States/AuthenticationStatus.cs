using System;

public class AuthenticationStatus
{
    public AuthenticationState State { get; set; } = AuthenticationState.Unauthenticated;

    public string UserId { get; set; }

    public string Email { get; set; }

    public DateTime? TokenExpiryTime { get; set; }

    public string ErrorMessage { get; set; }

    public bool IsAuthenticated => State == AuthenticationState.Authenticated;

    public bool IsAuthenticating => State == AuthenticationState.Authenticating;

    public void Reset()
    {
        State = AuthenticationState.Unauthenticated;
        UserId = null;
        Email = null;
        TokenExpiryTime = null;
        ErrorMessage = null;
    }

    public AuthenticationStatus Clone()
    {
        return new AuthenticationStatus
        {
            State = State,
            UserId = UserId,
            Email = Email,
            TokenExpiryTime = TokenExpiryTime,
            ErrorMessage = ErrorMessage
        };
    }
}
