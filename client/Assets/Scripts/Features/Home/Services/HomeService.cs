using System;
using System.Threading.Tasks;
using UnityEngine;

public class HomeService
{
    private SignalRClient client;
    private NetworkRef networkRef;
    private bool connectHereIfNeeded;
    private bool verboseLogs;

    public event Action<string> OnStatusChanged;
    public event Action<string> OnMatched;
    public event Action OnOpponentLeft;
    public event Action<string> OnLogMessage;

    public SignalRClient Client => client;
    public bool IsConnected => client != null && client.IsConnected;

    public HomeService(NetworkRef networkRef, bool connectHereIfNeeded = true, bool verboseLogs = true)
    {
        this.networkRef = networkRef;
        this.connectHereIfNeeded = connectHereIfNeeded;
        this.verboseLogs = verboseLogs;
    }

    public void Initialize()
    {
        client = (networkRef != null && networkRef.Client != null)
            ? networkRef.Client
            : SignalRClient.Instance;

        if (client != null)
        {
            Subscribe(client);
            if (client.IsConnected)
            {
                OnStatusChanged?.Invoke("Connecté, prêt.");
            }
            else
            {
                OnStatusChanged?.Invoke("Connexion en cours…");
                if (connectHereIfNeeded)
                {
                    Task<bool> _ = ConnectIfNeeded();
                }
            }
        }
        else
        {
            OnStatusChanged?.Invoke("Non connecté.");
            if (connectHereIfNeeded)
            {
                Task<bool> _ = ConnectIfNeeded();
            }
        }
    }

    public void Cleanup()
    {
        if (client != null)
            Unsubscribe(client);
    }

    public async Task<bool> ConnectIfNeeded()
    {
        client = SignalRClient.Instance;
        if (client != null && networkRef != null)
        {
            networkRef.Bind(client);
            client.BindNetworkRef(networkRef);
        }

        if (client == null)
        {
            if (!connectHereIfNeeded)
            {
                OnStatusChanged?.Invoke("Réseau non prêt. Retour via Login.");
                return false;
            }

            Log("Création d'un SignalRClient (menu).");
            GameObject go = new GameObject("NetworkRoot");
            client = go.AddComponent<SignalRClient>();
            Subscribe(client);

            if (networkRef != null)
            {
                networkRef.Bind(client);
                client.BindNetworkRef(networkRef);
            }
            else
            {
                Debug.LogWarning("networkRef est null, Bind non effectué.");
            }
        }

        AppConfig cfg = ConfigLoader.Load();
        string hubUrl = ConfigLoader.BuildGameHubUrl(cfg);
        if (!string.IsNullOrWhiteSpace(hubUrl))
            client.hubUrl = hubUrl;

        if (Jwt.I != null && Jwt.I.IsJwtPresent())
            client.SetAuthToken(Jwt.I.Token);

        if (client.IsConnected)
            return true;

        string displayName = "UnityPlayer";
        string email;
        if (Jwt.I != null && Jwt.I.TryGetClaim("email", out email) && !string.IsNullOrEmpty(email))
            displayName = email.Split('@')[0];

        OnStatusChanged?.Invoke("Connexion au serveur…");
        await client.ConnectAndIdentify(displayName);

        bool ok = await WaitUntilConnected(client, 6f);
        OnStatusChanged?.Invoke(ok ? "Connecté, prêt." : "Pas connecté.");
        return ok;
    }

    public async Task SearchOpponent(string deckId)
    {
        try
        {
            bool connected = await ConnectIfNeeded();
            if (!connected)
            {
                OnStatusChanged?.Invoke("Connexion impossible.");
                return;
            }

            OnStatusChanged?.Invoke("Recherche d'un adversaire…");

            if (!Guid.TryParse(deckId, out Guid deckGuid))
            {
                OnStatusChanged?.Invoke("Deck ID invalide.");
                return;
            }

            await client.JoinQueue(deckGuid);
            Log("JoinQueue envoyé.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            OnStatusChanged?.Invoke("Erreur matchmaking.");
        }
    }

    private void Subscribe(SignalRClient c)
    {
        c.OnStatus += HandleStatus;
        c.OnMatched += HandleMatched;
        c.OnOpponentLeft += HandleOpponentLeft;
        c.OnLog += HandleLog;
    }

    private void Unsubscribe(SignalRClient c)
    {
        c.OnStatus -= HandleStatus;
        c.OnMatched -= HandleMatched;
        c.OnOpponentLeft -= HandleOpponentLeft;
        c.OnLog -= HandleLog;
    }

    private void HandleStatus(string s)
    {
        OnStatusChanged?.Invoke(s);
    }

    private void HandleLog(string s)
    {
        Log("[SignalR] " + s);
    }

    private void HandleMatched(string roomKey)
    {
        Log("HandleMatched reçu, salle: " + roomKey);
        OnMatched?.Invoke(roomKey);
    }

    private void HandleOpponentLeft()
    {
        OnOpponentLeft?.Invoke();
    }

    private void Log(string m)
    {
        if (verboseLogs)
            Debug.Log(m);
        OnLogMessage?.Invoke(m);
    }

    private static async Task<bool> WaitUntilConnected(SignalRClient c, float timeoutSeconds)
    {
        float start = Time.realtimeSinceStartup;
        while (!c.IsConnected && (Time.realtimeSinceStartup - start) < timeoutSeconds)
            await Task.Yield();
        return c.IsConnected;
    }
}
