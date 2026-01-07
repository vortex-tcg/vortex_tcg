using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class HudUserProfileBinder : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Auth")]
    [SerializeField] private JwtStore jwtStore;

    private VisualElement root;
    private VisualElement textContainer;
    private TextElement playerName;

    private Dictionary<string, Label> labels = new Dictionary<string, Label>();
    private Dictionary<string, string> baseText = new Dictionary<string, string>();

    [Serializable]
    private class ResultDTO<T>
    {
        [JsonPropertyName("success")] public bool success { get; set; }
        [JsonPropertyName("statusCode")] public int statusCode { get; set; }
        [JsonPropertyName("data")] public T data { get; set; }
        [JsonPropertyName("message")] public string message { get; set; }
    }

    [Serializable]
    private class UserDTO
    {
        [JsonPropertyName("id")] public string id { get; set; } = "";
        [JsonPropertyName("username")] public string username { get; set; } = "";
        [JsonPropertyName("email")] public string email { get; set; } = "";
        [JsonPropertyName("currencyQuantity")] public int currencyQuantity { get; set; }
        [JsonPropertyName("language")] public string language { get; set; } = "";
        [JsonPropertyName("rankId")] public string rankId { get; set; }
    }

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[HudUserProfileBinder] UIDocument manquant.");
            return;
        }

        root = uiDocument.rootVisualElement;

        textContainer = root.Q<VisualElement>("TextContainer");
        if (textContainer == null)
            textContainer = root;

        playerName = root.Q<TextElement>("PlayerName"); 
        Debug.Log("[HudUserProfileBinder] playerName null ? " + (playerName == null ? "YES" : "NO"));

        if (playerName == null)
            Debug.LogWarning("[HudUserProfileBinder] TextElement 'PlayerName' introuvable.");
        else
            Debug.Log("[HudUserProfileBinder] PlayerName trouvé: name=" + playerName.name + " type=" + playerName.GetType().Name + " text='" + playerName.text + "'");

        SetupUserLabels();
        CacheBaseTexts();

        Refresh();
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        StopAllCoroutines();
        StartCoroutine(FetchAndApply());
    }

    private IEnumerator FetchAndApply()
    {
        ResolveJwtStore();

        if (jwtStore == null || !jwtStore.IsJwtPresent())
        {
            Debug.LogError("[HudUserProfileBinder] JWT absent.");
            yield break;
        }
        AppConfig cfg = null;
        try
        {
            cfg = ConfigLoader.Load();
        }
        catch (Exception e)
        {
            Debug.LogError("[HudUserProfileBinder] ConfigLoader.Load() a échoué: " + e.Message);
            yield break;
        }

        if (cfg == null || string.IsNullOrWhiteSpace(cfg.baseUrl))
        {
            Debug.LogError("[HudUserProfileBinder] baseUrl introuvable dans la config (ConfigLoader.Load().baseUrl).");
            yield break;
        }

        string userId = ResolveUserIdFromJwt(jwtStore);

        Guid userGuid;
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out userGuid))
        {
            Debug.LogError("[HudUserProfileBinder] userId introuvable/invalide dans le JWT (GUID attendu).");
            yield break;
        }

        string url = CombineUrl(cfg.baseUrl, "/api/user/" + userId);

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("Accept", "application/json");
            jwtStore.AttachAuthHeader(req);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[HudUserProfileBinder] GET failed " + req.responseCode + " - " + req.error + "\n" + req.downloadHandler.text);
                yield break;
            }

            string json = req.downloadHandler.text;
            Debug.Log("[HudUserProfileBinder] JSON reçu: " + json);

            ResultDTO<UserDTO> result = null;

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.PropertyNameCaseInsensitive = true;
                result = JsonSerializer.Deserialize<ResultDTO<UserDTO>>(json, options);
            }
            catch (Exception e)
            {
                Debug.LogError("[HudUserProfileBinder] JSON parse error: " + e.Message + "\nJSON:\n" + json);
                yield break;
            }

            if (result == null || !result.success || result.data == null)
            {
                string msg = (result != null) ? result.message : "null result";
                int code = (result != null) ? result.statusCode : -1;
                Debug.LogWarning("[HudUserProfileBinder] API success=false ou data=null. statusCode=" + code + " message=" + msg);
                yield break;
            }

            Apply(result.data);
        }

    }

    private void Apply(UserDTO u)
    {
        string uname = u != null ? u.username : null;
        string current = playerName != null ? playerName.text : "(playerName null)";

        Debug.Log("[HudUserProfileBinder] Apply: username='" + uname + "' currentText='" + current + "'");

        if (playerName == null)
        {
            Debug.LogWarning("[HudUserProfileBinder] Apply: playerName est NULL.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(uname))
            {
                Debug.LogWarning("[HudUserProfileBinder] Apply: username vide.");
            }
            else
            {
                Debug.Log("[HudUserProfileBinder] Update PlayerName: '" + playerName.text + "' -> '" + uname + "'");
                playerName.text = uname;
            }
        }

        SetAppend("EmailLabel", u.email);
        SetAppend("CurrencyLabel", u.currencyQuantity);
        SetAppend("LanguageLabel", u.language);
        SetAppend("RankLabel", string.IsNullOrWhiteSpace(u.rankId) ? "-" : u.rankId);
    }

    private void SetupUserLabels()
    {
        EnsureLabel("EmailLabel");
        EnsureLabel("CurrencyLabel");
        EnsureLabel("LanguageLabel");
        EnsureLabel("RankLabel");
    }

    private void EnsureLabel(string id)
    {
        Label l = root.Q<Label>(id);
        labels[id] = l;

        if (l == null)
            Debug.LogWarning("[HudUserProfileBinder] Label introuvable dans l'UXML: " + id);
    }

    private void CacheBaseTexts()
    {
        baseText.Clear();

        foreach (KeyValuePair<string, Label> kv in labels)
        {
            string key = kv.Key;
            Label l = kv.Value;
            baseText[key] = (l != null && l.text != null) ? l.text : "";
        }
    }

    private void SetAppend(string id, object value)
    {
        if (!labels.ContainsKey(id)) return;

        Label label = labels[id];
        if (label == null) return;

        string baseTxt = baseText.ContainsKey(id) ? baseText[id] : (label.text ?? "");
        string v = (value != null) ? value.ToString() : "";

        if (string.IsNullOrWhiteSpace(v))
        {
            label.text = baseTxt;
            return;
        }

        bool needsSpace = baseTxt.Length > 0 && !char.IsWhiteSpace(baseTxt[baseTxt.Length - 1]);
        label.text = needsSpace ? (baseTxt + " " + v) : (baseTxt + v);
    }
    private void ResolveJwtStore()
    {
        if (jwtStore != null) return;
        jwtStore = Jwt.I;
    }

    private string ResolveUserIdFromJwt(JwtStore store)
    {
        string claimKey = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        string v;
        if (store.TryGetClaim(claimKey, out v) && !string.IsNullOrWhiteSpace(v))
            return v;
        string[] keys = new string[] { "id", "userId", "sub" };

        int i = 0;
        while (i < keys.Length)
        {
            string k = keys[i];
            if (store.TryGetClaim(k, out v) && !string.IsNullOrWhiteSpace(v))
                return v;
            i++;
        }

        return null;
    }

    private static string CombineUrl(string b, string r)
    {
        if (string.IsNullOrEmpty(b)) return r;
        if (string.IsNullOrEmpty(r)) return b;

        if (b.EndsWith("/")) b = b.TrimEnd('/');
        if (!r.StartsWith("/")) r = "/" + r;
        return b + r;
    }
}
