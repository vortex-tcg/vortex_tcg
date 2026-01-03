using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System;

public class LoginScript : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Network")]
    [SerializeField] private NetworkRef networkRef;

    private TextField emailField;
    private TextField passwordField;
    private Label emailErrorLabel;
    private Button loginButton;
    private Button togglePasswordButton;

    private bool passwordVisible = false;
    private bool isSubmitting = false;

    // callbacks UI Toolkit (pour pouvoir unregister proprement)
    private EventCallback<ChangeEvent<string>> emailChangedCb;
    private EventCallback<ChangeEvent<string>> passwordChangedCb;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[LoginScript] UIDocument manquant.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        emailField = root.Q<TextField>("UsernameField");
        passwordField = root.Q<TextField>("PasswordField");
        emailErrorLabel = root.Q<Label>("ErrorLabel");
        loginButton = root.Q<Button>("LoginButton");
        togglePasswordButton = root.Q<Button>("TogglePasswordButton");

        if (passwordField != null)
            passwordField.isPasswordField = true;

        // prépare callbacks (une seule fois)
        emailChangedCb ??= _ => OnEmailValueChanged();
        passwordChangedCb ??= _ => UpdateLoginButtonState();

        if (emailField != null) emailField.RegisterValueChangedCallback(emailChangedCb);
        if (passwordField != null) passwordField.RegisterValueChangedCallback(passwordChangedCb);

        if (loginButton != null)
            loginButton.clicked += OnLoginClicked;

        if (togglePasswordButton != null)
            togglePasswordButton.clicked += TogglePasswordVisibility;

        HideError();
        UpdateLoginButtonState();
    }

    private void OnDisable()
    {
        // IMPORTANT : on retire les callbacks (sinon doublons à chaque enable)
        if (emailField != null && emailChangedCb != null)
            emailField.UnregisterValueChangedCallback(emailChangedCb);

        if (passwordField != null && passwordChangedCb != null)
            passwordField.UnregisterValueChangedCallback(passwordChangedCb);

        if (loginButton != null)
            loginButton.clicked -= OnLoginClicked;

        if (togglePasswordButton != null)
            togglePasswordButton.clicked -= TogglePasswordVisibility;
    }

    private void OnLoginClicked()
    {
        if (isSubmitting) return;
        StartCoroutine(Login());
    }

    private void TogglePasswordVisibility()
    {
        passwordVisible = !passwordVisible;

        if (passwordField != null)
            passwordField.isPasswordField = !passwordVisible;

        // Optionnel : changer le texte du bouton si tu veux
        if (togglePasswordButton != null)
            togglePasswordButton.text = passwordVisible ? "Hide" : "Show";
    }

    private void OnEmailValueChanged()
    {
        // feedback direct sur l’email
        if (emailErrorLabel != null)
        {
            bool ok = IsValidEmail(emailField != null ? emailField.value : string.Empty);
            emailErrorLabel.text = ok ? string.Empty : "Adresse email invalide.";
            emailErrorLabel.style.display = ok ? DisplayStyle.None : DisplayStyle.Flex;
        }

        UpdateLoginButtonState();
    }

    private void UpdateLoginButtonState()
    {
        if (loginButton == null)
            return;

        if (isSubmitting)
        {
            loginButton.SetEnabled(false);
            return;
        }

        string email = emailField != null ? emailField.value : string.Empty;
        string password = passwordField != null ? passwordField.value : string.Empty;

        bool ready = IsValidEmail(email) && !string.IsNullOrWhiteSpace(password);
        loginButton.SetEnabled(ready);
    }

    private static bool IsValidEmail(string email)
    {
        // simple et suffisant pour UI validation
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email ?? string.Empty, pattern);
    }

    private void HideError()
    {
        if (emailErrorLabel == null) return;
        emailErrorLabel.text = string.Empty;
        emailErrorLabel.style.display = DisplayStyle.None;
    }

    private void ShowError(string message)
    {
        if (emailErrorLabel == null) return;
        emailErrorLabel.text = message ?? "Erreur inconnue.";
        emailErrorLabel.style.display = DisplayStyle.Flex;
    }

    private IEnumerator Login()
    {
        isSubmitting = true;
        UpdateLoginButtonState();
        HideError();

        AppConfig cfg = ConfigLoader.Load();
        string baseUrl = (cfg != null ? cfg.apiBaseUrl : string.Empty) ?? string.Empty;
        baseUrl = baseUrl.TrimEnd('/');

        if (string.IsNullOrEmpty(baseUrl))
        {
            ShowError("Configuration API manquante.");
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string email = emailField != null ? emailField.value : string.Empty;
        if (!IsValidEmail(email))
        {
            ShowError("Adresse email invalide.");
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string password = passwordField != null ? passwordField.value : string.Empty;

        string url = baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? baseUrl + "/auth/login"
            : baseUrl + "/api/auth/login";

        string jsonBody = JsonUtility.ToJson(new LoginData(email, password));

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("[Login] POST " + url);

        try
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log("[Login] Réponse brute du /login: " + response);

                string token = ExtractTokenFromResponse(response);

                if (string.IsNullOrEmpty(token))
                {
                    token = BuildMockJwt(email, 3600);
                    Debug.LogWarning("[Login] Pas de token dans la réponse — utilisation d'un JWT mock (dev).");
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Jwt.I.SetToken(token, persist: true);
                    Debug.Log("[Login] Jwt.I.Token = " + Jwt.I.Token);
                }

                if (!Jwt.I.IsJwtPresent())
                {
                    ShowError("Jeton non disponible. Réessayez.");
                    yield break;
                }

                if (Jwt.I.IsExpired(30))
                {
                    ShowError("Session expirée. Réessayez.");
                    Jwt.I.Clear();
                    yield break;
                }

                LoadingScreen.Load("MainPage", loadMenu: true, unloadMenu: false);
            }
            else if (request.responseCode == 401)
            {
                ShowError("Email ou mot de passe incorrect.");
            }
            else
            {
                Debug.LogError($"[Login] Erreur HTTP {request.responseCode} : {request.error}");
                ShowError("Connexion avec le serveur impossible.");
            }
        }
        finally
        {
            isSubmitting = false;
            UpdateLoginButtonState();
        }
    }

    [Serializable]
    private class LoginData
    {
        public string email;
        public string password;

        public LoginData(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }

    [Serializable] private class LoginResponseWrapper { public LoginResponseData data; }
    [Serializable] private class LoginResponseData { public string token; }
    [Serializable] private class TokenRoot { public string token; }
    [Serializable] private class AccessTokenRoot { public string accessToken; }

    private string ExtractTokenFromResponse(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            var wrapper = JsonUtility.FromJson<LoginResponseWrapper>(json);
            if (wrapper != null && wrapper.data != null && !string.IsNullOrEmpty(wrapper.data.token))
            {
                Debug.Log("[Login] Token trouvé dans data.token");
                return wrapper.data.token;
            }
        }
        catch { }

        try
        {
            var root = JsonUtility.FromJson<TokenRoot>(json);
            if (root != null && !string.IsNullOrEmpty(root.token))
            {
                Debug.Log("[Login] Token trouvé dans token (racine)");
                return root.token;
            }
        }
        catch { }

        try
        {
            var acc = JsonUtility.FromJson<AccessTokenRoot>(json);
            if (acc != null && !string.IsNullOrEmpty(acc.accessToken))
            {
                Debug.Log("[Login] Token trouvé dans accessToken (racine)");
                return acc.accessToken;
            }
        }
        catch { }

        try
        {
            Match match = Regex.Match(json, "\"token\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                Debug.Log("[Login] Token trouvé via regex \"token\"");
                return match.Groups[1].Value;
            }
        }
        catch { }

        Debug.LogWarning("[Login] Impossible d'extraire un token de la réponse JSON.");
        return null;
    }

    private string BuildMockJwt(string email, int lifetimeSeconds)
    {
        string header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        long exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + lifetimeSeconds;
        string payload = "{\"sub\":\"" + email + "\",\"email\":\"" + email + "\",\"exp\":" + exp + "}";

        return Base64UrlEncode(Encoding.UTF8.GetBytes(header)) + "." +
               Base64UrlEncode(Encoding.UTF8.GetBytes(payload)) + ".";
    }

    private string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
