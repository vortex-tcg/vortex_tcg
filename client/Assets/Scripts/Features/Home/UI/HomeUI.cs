using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string searchOpponentButtonName = "PlayButton";
    [SerializeField] private string inviteFriendButtonName = "PlayWithFriendsButton";
    [SerializeField] private string collectionButtonName = "CollectionButton";
    [SerializeField] private string statusTextName = "StatusText";
    [SerializeField] private string searchingPanelName = "SearchingPanel";

    [SerializeField] private NetworkRef networkRef;
    [SerializeField] private string deckId = "d3b07384-d9a1-4d3b-92d8-4f5c6e7a8b9c";
    [SerializeField] private bool connectHereIfNeeded = true;
    [SerializeField] private bool verboseLogs = true;
    [SerializeField] private string matchSceneName = "MatchScene";

    private Button searchOpponentButton;
    private Button inviteFriendButton;
    private Button collectionButton;
    private Label statusText;
    private VisualElement searchingPanel;

    private HomeService service;
    private HomeState state;

    private void OnEnable()
    {
        InitializeUI();
        InitializeService();
        InitializeState();
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        service?.Cleanup();
    }

    private void InitializeUI()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument manquant sur ce GameObject (ou non assigné).");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        searchOpponentButton = root.Q<Button>(searchOpponentButtonName);
        inviteFriendButton = root.Q<Button>(inviteFriendButtonName);
        collectionButton = root.Q<Button>(collectionButtonName);
        statusText = root.Q<Label>(statusTextName);
        searchingPanel = root.Q<VisualElement>(searchingPanelName);

        if (searchOpponentButton != null)
            searchOpponentButton.clicked += OnClickSearchOpponent;
        else
            Debug.LogWarning($"Bouton '{searchOpponentButtonName}' introuvable dans l'UXML.");

        if (inviteFriendButton != null)
            inviteFriendButton.clicked += OnClickInviteFriend;
        else
            Debug.LogWarning($"Bouton '{inviteFriendButtonName}' introuvable dans l'UXML.");

        if (collectionButton != null)
            collectionButton.clicked += OnClickCollection;
        else
            Debug.LogWarning($"Bouton '{collectionButtonName}' introuvable dans l'UXML.");

        SetVisible(searchingPanel, false);
    }

    private void InitializeService()
    {
        service = new HomeService(networkRef, connectHereIfNeeded, verboseLogs);
        service.Initialize();
    }

    private void InitializeState()
    {
        state = new HomeState(deckId);
    }

    private void SubscribeToEvents()
    {
        if (service != null)
        {
            service.OnStatusChanged += HandleStatusChanged;
            service.OnMatched += HandleMatched;
            service.OnOpponentLeft += HandleOpponentLeft;
        }

        if (state != null)
        {
            state.OnSearchingStateChanged += HandleSearchingStateChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (searchOpponentButton != null)
            searchOpponentButton.clicked -= OnClickSearchOpponent;

        if (inviteFriendButton != null)
            inviteFriendButton.clicked -= OnClickInviteFriend;

        if (collectionButton != null)
            collectionButton.clicked -= OnClickCollection;

        if (service != null)
        {
            service.OnStatusChanged -= HandleStatusChanged;
            service.OnMatched -= HandleMatched;
            service.OnOpponentLeft -= HandleOpponentLeft;
        }

        if (state != null)
        {
            state.OnSearchingStateChanged -= HandleSearchingStateChanged;
        }
    }

    private async void OnClickSearchOpponent()
    {
        SetButtonsEnabled(false);
        state?.SetSearching(true);
        SetVisible(searchingPanel, true);

        if (service != null)
            await service.SearchOpponent(deckId);

        state?.SetSearching(false);
        SetButtonsEnabled(true);
    }

    private void OnClickInviteFriend()
    {
        Debug.Log("Inviter un ami (non implémenté pour l'instant).");
    }

    private void OnClickCollection()
    {
        Debug.Log("[HomeUI] Ouverture de la collection.");
        LoadingScreen.Load("CollectionScene", loadMenu: false, unloadMenu: false);
    }

    private void HandleStatusChanged(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    private void HandleSearchingStateChanged(bool isSearching)
    {
        SetVisible(searchingPanel, isSearching);
    }

    private void HandleMatched(string roomKey)
    {
        Debug.Log("HandleMatched reçu, salle: " + roomKey);
        SetVisible(searchingPanel, false);
        SceneManager.LoadScene(matchSceneName);
    }

    private void HandleOpponentLeft()
    {
        SetVisible(searchingPanel, false);
        if (statusText != null)
            statusText.text = "L'adversaire a quitté.";
    }

    private void SetButtonsEnabled(bool enabled)
    {
        searchOpponentButton?.SetEnabled(enabled);
        inviteFriendButton?.SetEnabled(enabled);
    }

    private static void SetVisible(VisualElement ve, bool visible)
    {
        if (ve == null) return;
        ve.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
