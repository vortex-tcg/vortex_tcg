using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class MainPageMenu : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    private Button playButton;
    private Button playWithFriendsButton;
    private Label statusLabel;

    [SerializeField] private NetworkRef networkRef;

    [Header("Options")]
    [Tooltip("Si aucune connexion n’existe, le menu la crée & s’identifie ici.")]
    public bool connectHereIfNeeded = true;
    public bool verboseLogs = true;

    private SignalRClient client;


    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        playButton = root.Q<Button>("PlayButton");
        playWithFriendsButton = root.Q<Button>("PlayWitchFriendsButton");
        statusLabel = root.Q<Label>("statusLabel");

        client = networkRef ? networkRef.Client : SignalRClient.Instance;

        if (playButton != null)
            playButton.clicked += OnClickPlay;

        if (playWithFriendsButton != null)
            playWithFriendsButton.clicked += OnClickSearchOpponent;

        if (client != null)
        {
            Subscribe(client);
            SetStatus(client.IsConnected ? "Connecté, prêt." : "Connexion en cours…");
        }
        else
        {
            SetStatus("Non connecté.");
            if (connectHereIfNeeded)
                _ = ConnectIfNeeded();
        }
    }

    private void OnDisable()
    {
        if (playButton != null)
            playButton.clicked -= OnClickPlay;

        if (playWithFriendsButton != null)
            playWithFriendsButton.clicked -= OnClickSearchOpponent;

        if (client != null)
            Unsubscribe(client);
    }


    private async Task<bool> ConnectIfNeeded()
    {
        client = SignalRClient.Instance;

        if (client == null)
        {
            if (!connectHereIfNeeded)
            {
                SetStatus("Réseau non prêt.");
                return false;
            }

            Log("[MainPage] Création d’un SignalRClient (menu).");
            var go = new GameObject("NetworkRoot");
            client = go.AddComponent<SignalRClient>();
            Subscribe(client);
        }

        var cfg = ConfigLoader.Load();
        var hubUrl = ConfigLoader.BuildGameHubUrl(cfg);
        if (!string.IsNullOrWhiteSpace(hubUrl))
            client.hubUrl = hubUrl;

        if (Jwt.I != null && Jwt.I.IsJwtPresent())
            client.SetAuthToken(Jwt.I.Token);

        if (client.IsConnected)
            return true;

        string displayName = "UnityPlayer";
        if (Jwt.I != null &&
            Jwt.I.TryGetClaim("email", out var email) &&
            !string.IsNullOrEmpty(email))
        {
            displayName = email.Split('@')[0];
        }

        SetStatus("Connexion au serveur…");
        await client.ConnectAndIdentify(displayName);

        bool ok = await WaitUntilConnected(client, 6f);
        SetStatus(ok ? "Connecté, prêt." : "Pas connecté.");
        return ok;
    }

    private static async Task<bool> WaitUntilConnected(SignalRClient c, float timeoutSeconds)
    {
        float start = Time.realtimeSinceStartup;
        while (!c.IsConnected && Time.realtimeSinceStartup - start < timeoutSeconds)
            await Task.Yield();

        return c.IsConnected;
    }


    private void OnClickPlay()
    {
        SetStatus("Mode solo sélectionné.");
    }

    private async void OnClickSearchOpponent()
    {
        playWithFriendsButton?.SetEnabled(false);

        try
        {
            if (!await ConnectIfNeeded())
            {
                SetStatus("Connexion impossible.");
                return;
            }

            SetStatus("Recherche d’un adversaire…");
            await client.JoinQueue();
            Log("[MainPage] JoinQueue envoyé.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            SetStatus("Erreur matchmaking.");
        }
        finally
        {
            playWithFriendsButton?.SetEnabled(true);
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

    private void HandleStatus(string s) => SetStatus(s);

    private void HandleMatched(string roomKey)
    {
        SetStatus("Adversaire trouvé !");
    }

    private void HandleOpponentLeft()
    {
        SetStatus("L’adversaire a quitté.");
    }

    private void HandleLog(string s)
    {
        Log("[SignalR] " + s);
    }

    private void SetStatus(string msg)
    {
        if (statusLabel != null)
            statusLabel.text = msg;

        Log("[STATUS] " + msg);
    }

    private void Log(string m)
    {
        if (verboseLogs)
            Debug.Log(m);
    }
}
