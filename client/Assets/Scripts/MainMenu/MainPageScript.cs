using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPageMenu : MonoBehaviour
{
    [Header("UI")]
    public Button playWithFriendsButton;
    public TMP_Text statusText; 
    [SerializeField] private NetworkRef networkRef;
    [Header("Options")]
    [Tooltip("Si aucune connexion n’existe, le menu la crée & s’identifie ici.")]
    public bool connectHereIfNeeded = true;
    public bool verboseLogs = true;

    private SignalRClient client;

    private void OnEnable()
    {
          var client = networkRef ? networkRef.Client : SignalRClient.Instance;

        if (playWithFriendsButton)
            playWithFriendsButton.onClick.AddListener(OnClickSearchOpponent);

        if (client != null)
        {
            Subscribe(client);
            SetStatus(client.IsConnected ? "Connecté, prêt." : "Connexion en cours…");
        }
        else
        {
            SetStatus("Non connecté.");
            if (connectHereIfNeeded) _ = ConnectIfNeeded();
        }
    }

    private void OnDisable()
    {
        if (playWithFriendsButton)
            playWithFriendsButton.onClick.RemoveListener(OnClickSearchOpponent);
        if (client != null) Unsubscribe(client);
    }

    private void Subscribe(SignalRClient c)
    {
        c.OnStatus       += HandleStatus;
        c.OnMatched      += HandleMatched;
        c.OnOpponentLeft += HandleOpponentLeft;
        c.OnLog          += HandleLog;
    }
    private void Unsubscribe(SignalRClient c)
    {
        c.OnStatus       -= HandleStatus;
        c.OnMatched      -= HandleMatched;
        c.OnOpponentLeft -= HandleOpponentLeft;
        c.OnLog          -= HandleLog;
    }

    // ---------- Connexion (si besoin) ----------
    private async Task<bool> ConnectIfNeeded()
    {
        // 1) Instance
        client = SignalRClient.Instance;
        if (client == null)
        {
            if (!connectHereIfNeeded) { SetStatus("Réseau non prêt. Retour via Login."); return false; }
            Log("[MainPage] Création d’un SignalRClient (menu).");
            var go = new GameObject("NetworkRoot");
            client = go.AddComponent<SignalRClient>(); // DontDestroyOnLoad dans Awake()
            Subscribe(client);
        }

        // 2) Hub URL depuis la config
        var cfg = ConfigLoader.Load();
        var hubUrl = ConfigLoader.BuildGameHubUrl(cfg);
        if (!string.IsNullOrWhiteSpace(hubUrl))
            client.hubUrl = hubUrl;

        // 3) JWT si dispo
        if (Jwt.I != null && Jwt.I.IsJwtPresent())
            client.SetAuthToken(Jwt.I.Token);

        // 4) Déjà connecté ?
        if (client.IsConnected) return true;

        // 5) Connect + identify
        string displayName = "UnityPlayer";
        if (Jwt.I != null && Jwt.I.TryGetClaim("email", out var email) && !string.IsNullOrEmpty(email))
            displayName = email.Split('@')[0];

        SetStatus("Connexion au serveur…");
        await client.ConnectAndIdentify(displayName);

        // 6) Attendre un court instant l’état connecté
        bool ok = await WaitUntilConnected(client, 6f);
        SetStatus(ok ? "Connecté, prêt." : "Pas connecté.");
        return ok;
    }

    private static async Task<bool> WaitUntilConnected(SignalRClient c, float timeoutSeconds)
    {
        float start = Time.realtimeSinceStartup;
        while (!c.IsConnected && (Time.realtimeSinceStartup - start) < timeoutSeconds)
            await Task.Yield();
        return c.IsConnected;
    }

    // ---------- Clic matchmaking ----------
    private async void OnClickSearchOpponent()
    {
        playWithFriendsButton.interactable = false;
        try
        {
            if (!await ConnectIfNeeded())
            {
                SetStatus("Connexion impossible.");
                return;
            }

            SetStatus("Recherche d’un adversaire…");
            await client.JoinQueue(); // SafeSend côté client → aucun arg fantôme
            Log("[MainPage] JoinQueue envoyé.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            SetStatus("Erreur matchmaking.");
        }
        finally
        {
            playWithFriendsButton.interactable = true;
        }
    }

    // ---------- Events du client ----------
    private void HandleStatus(string s) => SetStatus(s);
    private void HandleLog(string s)    { Log("[SignalR] " + s); }

    private void HandleMatched(string roomKey)
    {
        SetStatus("Adversaire trouvé !");
        // TODO: charger la scène de jeu ici si besoin
        // SceneManager.LoadScene("BoardScene");
    }

    private void HandleOpponentLeft()
    {
        SetStatus("L’adversaire a quitté.");
    }

    // ---------- UI utils ----------
    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
        Log("[STATUS] " + msg);
    }
    private void Log(string m) { if (verboseLogs) Debug.Log(m); }
}
