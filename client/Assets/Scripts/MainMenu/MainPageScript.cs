using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainPageMenu : MonoBehaviour
{
    [Header("UI Toolkit (UXML names)")]
    [Tooltip("UIDocument qui contient MainPageHUDVisualTree. Si null, GetComponent<UIDocument>().")]
    [SerializeField] private UIDocument uiDocument;

    // D'après ton UXML (dans la hiérarchie tu vois #PlayButton et #PlayWithFriendsButton)
    [SerializeField] private string searchOpponentButtonName = "PlayButton";
    [SerializeField] private string inviteFriendButtonName = "PlayWithFriendsButton";

    // Optionnels : seulement si tu les as dans ton UXML (sinon ça reste null et le script marche quand même)
    [SerializeField] private string statusTextName = "StatusText";        // ex: <Label name="StatusText" />
    [SerializeField] private string searchingPanelName = "SearchingPanel"; // ex: <VisualElement name="SearchingPanel" />

    [SerializeField] private NetworkRef networkRef;
    [SerializeField] private string deckId = "d3b07384-d9a1-4d3b-92d8-4f5c6e7a8b9c";

    [Header("Options")]
    [Tooltip("Si aucune connexion n’existe, le menu la crée & s’identifie ici.")]
    public bool connectHereIfNeeded = true;

    public bool verboseLogs = true;

    [Header("Navigation")]
    [SerializeField] private string matchSceneName = "3DMatchScene";

    // UI refs (UI Toolkit)
    private Button searchOpponentButton;
    private Button inviteFriendButton;
    private Label statusText;                 // remplace TMP_Text
    private VisualElement searchingPanel;     // remplace GameObject

    private SignalRClient client;

    private void OnEnable()
    {
        // --- UI Toolkit bindings ---
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[MainPageMenu] UIDocument manquant sur ce GameObject (ou non assigné).");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        // ⚠️ Q() prend le name SANS le '#'
        searchOpponentButton = root.Q<Button>(searchOpponentButtonName);
        inviteFriendButton = root.Q<Button>(inviteFriendButtonName);

        // Optionnels (si présents dans l'UXML)
        statusText = root.Q<Label>(statusTextName);
        searchingPanel = root.Q<VisualElement>(searchingPanelName);

        if (searchOpponentButton != null)
            searchOpponentButton.clicked += OnClickSearchOpponent;
        else
            Debug.LogWarning($"[MainPageMenu] Bouton '{searchOpponentButtonName}' introuvable dans l'UXML.");

        if (inviteFriendButton != null)
            inviteFriendButton.clicked += OnClickInviteFriend;
        else
            Debug.LogWarning($"[MainPageMenu] Bouton '{inviteFriendButtonName}' introuvable dans l'UXML.");

        SetVisible(searchingPanel, false);

        // --- Réseau (ton code d'avant, inchangé dans l'esprit) ---
        client = (networkRef != null && networkRef.Client != null)
            ? networkRef.Client
            : SignalRClient.Instance;

        if (client != null)
        {
            Subscribe(client);
            if (client.IsConnected)
            {
                SetStatus("Connecté, prêt.");
            }
            else
            {
                SetStatus("Connexion en cours…");
                if (connectHereIfNeeded)
                {
                    Task<bool> _ = ConnectIfNeeded();
                }
            }
        }
        else
        {
            SetStatus("Non connecté.");
            if (connectHereIfNeeded)
            {
                Task<bool> _ = ConnectIfNeeded();
            }
        }
    }

    private void OnDisable()
    {
        if (searchOpponentButton != null)
            searchOpponentButton.clicked -= OnClickSearchOpponent;

        if (inviteFriendButton != null)
            inviteFriendButton.clicked -= OnClickInviteFriend;

        if (client != null)
            Unsubscribe(client);
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

    private async Task<bool> ConnectIfNeeded()
    {
        client = SignalRClient.Instance;
        if (client == null)
        {
            if (!connectHereIfNeeded)
            {
                SetStatus("Réseau non prêt. Retour via Login.");
                return false;
            }

            Log("[MainPage] Création d’un SignalRClient (menu).");
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
                Debug.LogWarning("[MainPage] networkRef est null, Bind non effectué.");
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

        SetStatus("Connexion au serveur…");
        await client.ConnectAndIdentify(displayName);

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

    private async void OnClickSearchOpponent()
    {
        SetButtonsEnabled(false);

        try
        {
            bool connected = await ConnectIfNeeded();
            if (!connected)
            {
                SetStatus("Connexion impossible.");
                return;
            }

            SetStatus("Recherche d’un adversaire…");
            SetVisible(searchingPanel, true);

            if (!Guid.TryParse(deckId, out Guid deckGuid))
            {
                SetStatus("Deck ID invalide.");
                SetVisible(searchingPanel, false);
                return;
            }

            await client.JoinQueue(deckGuid);
            Log("[MainPage] JoinQueue envoyé.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            SetStatus("Erreur matchmaking.");
            SetVisible(searchingPanel, false);
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private void OnClickInviteFriend()
    {
        Log("[MainPage] Inviter un ami (non implémenté pour l’instant).");
    }

    private void HandleStatus(string s)
    {
        SetStatus(s);
    }

    private void HandleLog(string s)
    {
        Log("[SignalR] " + s);
    }

    private void HandleMatched(string roomKey)
    {
        Log("[MAIN] HandleMatched reçu, salle: " + roomKey);
        SetVisible(searchingPanel, false);
        SceneManager.LoadScene(matchSceneName);
    }

    private void HandleOpponentLeft()
    {
        SetVisible(searchingPanel, false);
        SetStatus("L’adversaire a quitté.");
    }

    private void SetStatus(string msg)
    {
        // UI Toolkit: Label.text
        if (statusText != null)
            statusText.text = msg;

        Log("[STATUS] " + msg);
    }

    private void Log(string m)
    {
        if (verboseLogs)
            Debug.Log(m);
    }

    // ---- Helpers UI Toolkit ----

    private void SetButtonsEnabled(bool enabled)
    {
        // UI Toolkit: SetEnabled au lieu de interactable
        searchOpponentButton?.SetEnabled(enabled);
        inviteFriendButton?.SetEnabled(enabled);
    }

    private static void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
