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

        if (emailField != null)
            emailField.onValueChanged.AddListener(_ => UpdateLoginButtonState());

        if (passwordField != null)
            passwordField.onValueChanged.AddListener(_ => UpdateLoginButtonState());

        if (loginButton != null)
            loginButton.onClick.AddListener(() => StartCoroutine(Login()));
    }

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email ?? string.Empty, pattern);
    }

    private void UpdateLoginButtonState()
    {
        if (loginButton == null)
            return;

        if (isSubmitting)
        {
            loginButton.interactable = false;
            return;
        }

        bool ready =
            IsValidEmail(emailField != null ? emailField.text : string.Empty) &&
            !string.IsNullOrEmpty(passwordField != null ? passwordField.text : string.Empty);

        loginButton.interactable = ready;
    }

    private IEnumerator Login()
    {
        isSubmitting = true;
        UpdateLoginButtonState();

        AppConfig cfg = ConfigLoader.Load();
        string baseUrl = (cfg != null ? cfg.apiBaseUrl : string.Empty) ?? string.Empty;
        baseUrl = baseUrl.TrimEnd('/');

        if (string.IsNullOrEmpty(baseUrl))
        {
            emailErrorText.text = "Configuration API manquante.";
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string email = emailField != null ? emailField.text : string.Empty;
        if (!IsValidEmail(email))
        {
            emailErrorText.text = "Adresse email invalide";
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string password = passwordField != null ? passwordField.text : string.Empty;

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

    [Serializable]
    private class TokenOnly { public string token; }

    [Serializable]
    private class AccessTokenOnly { public string accessToken; }

    [Serializable]
    private class LoginResponseWrapper
    {
        public LoginResponseData data;
    }

    [Serializable]
    private class LoginResponseData
    {
        public string token;
    }

    [Serializable]
    private class TokenRoot
    {
        public string token;
    }

    [Serializable]
    private class AccessTokenRoot
    {
        public string accessToken;
    }

    private string ExtractTokenFromResponse(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            LoginResponseWrapper wrapper = JsonUtility.FromJson<LoginResponseWrapper>(json);
            if (wrapper != null && wrapper.data != null && !string.IsNullOrEmpty(wrapper.data.token))
            {
                Debug.Log("[Login] Token trouvé dans data.token");
                return wrapper.data.token;
            }
        }
        catch
        {
        }
        try
        {
            TokenRoot root = JsonUtility.FromJson<TokenRoot>(json);
            if (root != null && !string.IsNullOrEmpty(root.token))
            {
                Debug.Log("[Login] Token trouvé dans token (racine)");
                return root.token;
            }
        }
        catch
        {
        }

        try
        {
            AccessTokenRoot acc = JsonUtility.FromJson<AccessTokenRoot>(json);
            if (acc != null && !string.IsNullOrEmpty(acc.accessToken))
            {
                Debug.Log("[Login] Token trouvé dans accessToken (racine)");
                return acc.accessToken;
            }
        }
        catch
        {
        }
        try
        {
            Match match = Regex.Match(
                json,
                "\"token\"\\s*:\\s*\"([^\"]+)\"",
                RegexOptions.IgnoreCase
            );
            if (match.Success)
            {
                Debug.Log("[Login] Token trouvé via regex \"token\"");
                return match.Groups[1].Value;
            }
        }
        catch
        {
        }

        Debug.LogWarning("[Login] Impossible d'extraire un token de la réponse JSON.");
        return null;
    }

    private string ExtractRawString(string json, string key)
    {
        string pattern = "\"" + key + "\"";
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
        string header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        long exp = (long)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + lifetimeSeconds);
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

    public void OnEmailChanged(string input)
    {
        if (emailErrorText != null)
            emailErrorText.text = IsValidEmail(input) ? string.Empty : "Adresse email invalide.";

        UpdateLoginButtonState();
    }
}
