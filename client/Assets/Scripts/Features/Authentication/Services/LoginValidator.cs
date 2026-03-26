using System.Text.RegularExpressions;
using UnityEngine;

public class LoginValidator
{
    private EventBus eventBus = EventBus.Instance;

    public LoginValidator()
    {
        eventBus.Subscribe<ValidateLoginInputEvent>(OnValidateLoginInput);
    }

    ~LoginValidator()
    {
        eventBus.Unsubscribe<ValidateLoginInputEvent>(OnValidateLoginInput);
    }

    private void OnValidateLoginInput(ValidateLoginInputEvent evt)
    {
        if (!IsValidEmail(evt.Email))
        {
            evt.IsValid = false;
            evt.ValidationError = "Adresse email invalide.";
            return;
        }

        if (string.IsNullOrWhiteSpace(evt.Password))
        {
            evt.IsValid = false;
            evt.ValidationError = "Mot de passe requis.";
            return;
        }

        evt.IsValid = true;
    }

    public bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email ?? string.Empty, pattern);
    }
}
