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

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        emailField = root.Q<TextField>("UsernameField");
        passwordField = root.Q<TextField>("PasswordField");
        emailErrorLabel = root.Q<Label>("ErrorLabel");
        loginButton = root.Q<Button>("LoginButton");
        togglePasswordButton = root.Q<Button>("TogglePasswordButton");

        if (passwordField != null)
            passwordField.isPasswordField = true;

        if (emailField != null)
            emailField.RegisterValueChangedCallback(evt => UpdateLoginButtonState());

        if (passwordField != null)
            passwordField.RegisterValueChangedCallback(evt => UpdateLoginButtonState());

        if (loginButton != null)
            loginButton.clicked += OnLoginClicked;

        if (togglePasswordButton != null)
            togglePasswordButton.clicked += TogglePasswordVisibility;

        HideError();
        UpdateLoginButtonState();
    }

    private void OnDisable()
    {
        if (loginButton != null)
            loginButton.clicked -= OnLoginClicked;
        if (togglePasswordButton != null)
            togglePasswordButton.clicked -= TogglePasswordVisibility;
    }

    private void TogglePasswordVisibility()
    {
        passwordVisible = !passwordVisible;
        if (passwordField != null)
            passwordField.isPasswordField = !passwordVisible;
    }

    private void UpdateLoginButtonState()
    {
        if (loginButton == null) return;

        if (isSubmitting)
        {
            loginButton.SetEnabled(false);
            return;
        }

        bool ready = IsValidEmail(emailField?.value) && !string.IsNullOrEmpty(passwordField?.value);
        loginButton.SetEnabled(ready);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    private void ShowError(string message)
    {
        if (emailErrorLabel == null) return;
        emailErrorLabel.style.display = DisplayStyle.Flex;
        emailErrorLabel.text = message;
    }

    private void HideError()
    {
        if (emailErrorLabel == null) return;
        emailErrorLabel.style.display = DisplayStyle.None;
        emailErrorLabel.text = "";
    }

    private void OnLoginClicked()
    {
        StartCoroutine(LoginCoroutine());
    }

    private IEnumerator LoginCoroutine()
    {
        isSubmitting = true;
        UpdateLoginButtonState();
        HideError();

        var cfg = ConfigLoader.Load();
        string baseUrl = (cfg?.apiBaseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            ShowError("Configuration API manquante.");
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string email = emailField?.value ?? "";
        if (!IsValidEmail(email))
        {
            ShowError("Adresse email invalide");
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string password = passwordField?.value ?? "";

        string url = baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? baseUrl + "/auth/login"
            : baseUrl + "/api/auth/login";

        string jsonBody = JsonUtility.ToJson(new LoginData(email, password));

        UnityWebRequest request = new UnityWebRequest(url, "POST");
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
                string token = ExtractTokenFromResponse(response);

                if (string.IsNullOrEmpty(token))
                {
                    token = BuildMockJwt(email, 3600);
                    Debug.LogWarning("[Login] Pas de token dans la réponse — utilisation d'un JWT mock (dev).");
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Jwt.I.SetToken(token, persist: true);
                    var client = SignalRClient.Instance ?? new GameObject("NetworkRoot").AddComponent<SignalRClient>();
                    networkRef?.Bind(client);
                    client.SetAuthToken(Jwt.I.Token);
                    var displayName = (email ?? "UnityPlayer").Split('@')[0];
                    Debug.Log("[Login] Connecting to hub as " + displayName);
                    var _ = client.ConnectAndIdentify(displayName);

                    float start = Time.realtimeSinceStartup;
                    while (!client.IsConnected && Time.realtimeSinceStartup - start < 5f)
                        yield return null;
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
        public LoginData(string email, string password) { this.email = email; this.password = password; }
    }

    [Serializable] private class TokenOnly { public string token; }
    [Serializable] private class AccessTokenOnly { public string accessToken; }

    private string ExtractTokenFromResponse(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            var raw = ExtractRawString(json, "Token");
            if (!string.IsNullOrEmpty(raw))
            {
                var t1 = JsonUtility.FromJson<TokenOnly>("{\"Token\":" + raw + "}");
                if (!string.IsNullOrEmpty(t1.token)) return t1.token;
            }
        }
        catch { }

        try
        {
            var raw = ExtractRawString(json, "accessToken");
            if (!string.IsNullOrEmpty(raw))
            {
                var t2 = JsonUtility.FromJson<AccessTokenOnly>("{\"accessToken\":" + raw + "}");
                if (!string.IsNullOrEmpty(t2.accessToken)) return t2.accessToken;
            }
        }
        catch { }

        return null;
    }

    private string ExtractRawString(string json, string key)
    {
        int i = json.IndexOf($"\"{key}\"", StringComparison.Ordinal);
        if (i < 0) return null;
        int start = json.IndexOf(':', i);
        if (start < 0) return null;
        int firstQuote = json.IndexOf('"', start + 1);
        if (firstQuote < 0) return null;
        int secondQuote = json.IndexOf('"', firstQuote + 1);
        if (secondQuote < 0) return null;
        return json.Substring(firstQuote, secondQuote - firstQuote + 1);
    }

    private string BuildMockJwt(string email, int lifetimeSeconds)
    {
        var header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        long exp = (long)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + lifetimeSeconds);
        string payload = $"{{\"sub\":\"{email}\",\"email\":\"{email}\",\"exp\":{exp}}}";

        return Base64UrlEncode(Encoding.UTF8.GetBytes(header)) + "." +
               Base64UrlEncode(Encoding.UTF8.GetBytes(payload)) + ".";
    }

    private string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
