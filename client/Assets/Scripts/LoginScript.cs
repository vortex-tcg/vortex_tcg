using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public class LoginScript : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public TMP_Text emailErrorText;
    public Button loginButton;

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

    private bool IsValidEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email ?? "", pattern);
    }

    private void Start()
    {
        UpdateLoginButtonState();
        emailField.onValueChanged.AddListener(_ => UpdateLoginButtonState());
        passwordField.onValueChanged.AddListener(_ => UpdateLoginButtonState());
        loginButton.onClick.AddListener(() => StartCoroutine(Login()));
    }

    private void UpdateLoginButtonState()
    {
        if (isSubmitting)
        {
            loginButton.interactable = false;
            return;
        }

        bool ready = IsValidEmail(emailField.text) && !string.IsNullOrEmpty(passwordField.text);
        loginButton.interactable = ready;
    }

    IEnumerator Login()
    {
        isSubmitting = true;
        UpdateLoginButtonState();

        string baseUrl = ConfigLoader.Load().apiBaseUrl;
        string email = emailField.text;

        if (!IsValidEmail(email))
        {
            emailErrorText.text = "Adresse email invalide";
            isSubmitting = false;
            UpdateLoginButtonState();
            yield break;
        }

        string password = passwordField.text;
        string url = baseUrl + "/api/login";

        string jsonBody = JsonUtility.ToJson(new LoginData(email, password));

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

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
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Jwt.I.SetToken(token, persist: true);
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
                Debug.LogError("Erreur de connexion : " + request.error);
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

    private string ExtractTokenFromResponse(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            var t1 = JsonUtility.FromJson<TokenOnly>("{\"token\":" + ExtractRawString(json, "token") + "}");
            if (!string.IsNullOrEmpty(t1.token)) return t1.token;
        }
        catch { /* ignore */ }

        try
        {
            var t2 = JsonUtility.FromJson<AccessTokenOnly>("{\"accessToken\":" + ExtractRawString(json, "accessToken") + "}");
            if (!string.IsNullOrEmpty(t2.accessToken)) return t2.accessToken;
        }
        catch { /* ignore */ }

        return null;

    }

    [System.Serializable] private class TokenOnly { public string token; }
    [System.Serializable] private class AccessTokenOnly { public string accessToken; }

    private string ExtractRawString(string json, string key)
    {
        string pattern = $"\"{key}\"";
        int i = json.IndexOf(pattern);
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
        return System.Convert.ToBase64String(bytes)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public void OnEmailChanged(string input)
    {
        emailErrorText.text = IsValidEmail(input) ? "" : "Adresse email invalide.";
        UpdateLoginButtonState();
    }
}
