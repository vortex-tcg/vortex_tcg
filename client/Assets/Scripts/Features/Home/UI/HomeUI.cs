using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string searchOpponentButtonName = "PlayButton";
    [SerializeField] private string inviteFriendButtonName = "PlayWithFriendsButton";
    [SerializeField] private string statusTextName = "StatusText";
    [SerializeField] private string searchingPanelName = "SearchingPanel";
    [SerializeField] private string deckIdInputName = "DeckIdInput";

    [SerializeField] private NetworkRef networkRef;
    [SerializeField] private string deckId = "d3b07384-d9a1-4d3b-92d8-4f5c6e7a8b9c";
    [SerializeField] private bool connectHereIfNeeded = true;
    [SerializeField] private bool verboseLogs = true;
    [SerializeField] private string matchSceneName = "MatchScene";

    private const string DeckIdKey = "SelectedDeckId";

    private Button searchOpponentButton;
    private Button inviteFriendButton;
    private Label statusText;
    private VisualElement searchingPanel;
    private TextField deckIdInput;

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
        statusText = root.Q<Label>(statusTextName);
        searchingPanel = root.Q<VisualElement>(searchingPanelName);
        deckIdInput = root.Q<TextField>(deckIdInputName);

        if (searchOpponentButton != null)
            searchOpponentButton.clicked += OnClickSearchOpponent;
        else
            Debug.LogWarning($"Bouton '{searchOpponentButtonName}' introuvable dans l'UXML.");

        if (inviteFriendButton != null)
            inviteFriendButton.clicked += OnClickInviteFriend;
        else
            Debug.LogWarning($"Bouton '{inviteFriendButtonName}' introuvable dans l'UXML.");

        if (deckIdInput != null)
            deckIdInput.value = PlayerPrefs.GetString(DeckIdKey, deckId);
        else
            Debug.LogWarning($"Champ '{deckIdInputName}' introuvable dans l'UXML.");

        SetVisible(searchingPanel, false);
    }

    private void InitializeService()
    {
        service = new HomeService(networkRef, connectHereIfNeeded, verboseLogs);
        service.Initialize();
    }

    private void InitializeState()
    {
        string initialDeckId = deckIdInput != null ? deckIdInput.value : deckId;
        state = new HomeState(initialDeckId);
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
        if (deckIdInput != null)
            state?.SetDeckId(deckIdInput.value);

        // Save the selected deck id
        if (state != null)
            PlayerPrefs.SetString(DeckIdKey, state.DeckId);

        SetButtonsEnabled(false);
        state?.SetSearching(true);
        SetVisible(searchingPanel, true);

        if (service != null && state != null)
            await service.SearchOpponent(state.DeckId);

        state?.SetSearching(false);
        SetButtonsEnabled(true);
    }

    private void OnClickInviteFriend()
    {
        Debug.Log("Inviter un ami (non implémenté pour l'instant).");
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
