using System;

public class LoginRequestedEvent
{
    public string Email { get; }
    public string Password { get; }

    public LoginRequestedEvent(string email, string password)
    {
        Email = email;
        Password = password;
    }
}


public class LoginSuccessEvent
{
    public string Token { get; }
    public DateTime Timestamp { get; }

    public LoginSuccessEvent(string token)
    {
        Token = token;
        Timestamp = DateTime.UtcNow;
    }
}


public class LoginFailedEvent
{
    public string ErrorMessage { get; }
    public int? ErrorCode { get; }
    public DateTime Timestamp { get; }

    public LoginFailedEvent(string errorMessage, int? errorCode = null)
    {
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }
}

public class ValidateLoginInputEvent
{
    public string Email { get; }
    public string Password { get; }
    public bool IsValid { get; set; }
    public string ValidationError { get; set; }

    public ValidateLoginInputEvent(string email, string password)
    {
        Email = email;
        Password = password;
        IsValid = true;
        ValidationError = string.Empty;
    }
}
