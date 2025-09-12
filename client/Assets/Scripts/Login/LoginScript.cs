using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System;

public class LoginScript : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_Text emailErrorText;
    public Button loginButton;
    [SerializeField] private NetworkRef networkRef;

    private bool passwordVisible = false;
    private bool isSubmitting = false;

    public void TogglePasswordVisibility()
    {
        passwordVisible = !passwordVisible;
        passwordField.contentType = passwordVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        passwordField.ForceLabelUpdate();
    }

    private void Start()
    {
        UpdateLoginButtonState();
        if (emailField)    emailField.onValueChanged.AddListener(_ => UpdateLoginButtonState());
        if (passwordField) passwordField.onValueChanged.AddListener(_ => UpdateLoginButtonState());
        if (loginButton)   loginButton.onClick.AddListener(() => StartCoroutine(Login()));
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email ?? "", pattern);
    }

    private void UpdateLoginButtonState()
    {
        if (loginButton == null) return;

        if (isSubmitting)
        {
            loginButton.interactable = false;
            return;
        }

        bool ready = IsValidEmail(emailField?.text) && !string.IsNullOrEmpty(passwordField?.text);
        loginButton.interactable = ready;
    }

    IEnumerator Login()
    {
        isSubmitting = true;
        UpdateLoginButtonState();

        var cfg = ConfigLoader.Load();
        string baseUrl = (cfg?.apiBaseUrl ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            emailErrorText.text = "Configuration API manquante.";
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string email = emailField?.text ?? "";
        if (!IsValidEmail(email))
        {
            emailErrorText.text = "Adresse email invalide";
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string password = passwordField?.text ?? "";

        string url = baseUrl.EndsWith("/api", System.StringComparison.OrdinalIgnoreCase)
            ? baseUrl + "/auth/Login"
            : baseUrl + "/api/auth/Login";

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
                    emailErrorText.text = "Jeton non disponible. Réessayez.";
                    yield break;
                }

                if (Jwt.I.IsExpired(30))
                {
                    emailErrorText.text = "Session expirée. Réessayez.";
                    Jwt.I.Clear();
                    yield break;
                }

                LoadingScreen.Load("MainPage", loadMenu: true, unloadMenu: false);
            }
            else if (request.responseCode == 401)
            {
                emailErrorText.text = "Email ou mot de passe incorrect.";
            }
            else
            {
                Debug.LogError($"[Login] Erreur HTTP {request.responseCode} : {request.error}");
                emailErrorText.text = "Connexion avec le serveur impossible.";
            }
        }
        finally
        {
            isSubmitting = false;
            UpdateLoginButtonState();
        }
    }


    [System.Serializable]
    private class LoginData
    {
        public string email;
        public string password;
        public LoginData(string email, string password) { this.email = email; this.password = password; }
    }

    [System.Serializable] private class TokenOnly { public string token; }
    [System.Serializable] private class AccessTokenOnly { public string accessToken; }

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
        catch { /* ignore */ }

        try
        {
            var raw = ExtractRawString(json, "accessToken");
            if (!string.IsNullOrEmpty(raw))
            {
                var t2 = JsonUtility.FromJson<AccessTokenOnly>("{\"accessToken\":" + raw + "}");
                if (!string.IsNullOrEmpty(t2.accessToken)) return t2.accessToken;
            }
        }
        catch { /* ignore */ }

        return null;
    }

    private string ExtractRawString(string json, string key)
    {
        string pattern = $"\"{key}\"";
        int i = json.IndexOf(pattern, StringComparison.Ordinal);
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
        long exp = (long)(System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + lifetimeSeconds);
        string payload = $"{{\"sub\":\"{email}\",\"email\":\"{email}\",\"exp\":{exp}}}";

        return Base64UrlEncode(Encoding.UTF8.GetBytes(header)) + "." +
               Base64UrlEncode(Encoding.UTF8.GetBytes(payload)) + ".";
    }

    private string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public void OnEmailChanged(string input)
    {
        if (emailErrorText)
            emailErrorText.text = IsValidEmail(input) ? "" : "Adresse email invalide.";
        UpdateLoginButtonState();
    }
}
