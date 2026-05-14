using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Collection.Services;
using VortexTCG.Scripts.Features.Deck.Services;

public class HomeUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string searchOpponentButtonName = "PlayButton";
    [SerializeField] private string inviteFriendButtonName = "PlayWithFriendsButton";
    [SerializeField] private string collectionButtonName = "CollectionButton";
    [SerializeField] private string statusTextName = "StatusText";
    [SerializeField] private string searchingPanelName = "SearchingPanel";
    [SerializeField] private string deckSelectionModalName = "DeckSelectionModalHUD";
    [SerializeField] private string deckDropdownContainerName = "DeckDropdownContainer";
    [SerializeField] private string deckSelectionStatusName = "SelectionStatusLabel";

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
    private UIDocument deckSelectionModalDocument;
    private VisualElement deckSelectionRoot;
    private VisualElement deckDropdownContainer;
    private DropdownField deckDropdown;
    private Label deckSelectionStatus;
    private Button deckSelectionConfirmButton;
    private Button deckSelectionCancelButton;

    private HomeService service;
    private HomeState state;
    private CollectionService collectionService;
    private DeckService deckService;
    private readonly List<UserCollectionDeckDto> ownedDecks = new List<UserCollectionDeckDto>();
    private readonly List<UserCollectionDeckDto> selectableDecks = new List<UserCollectionDeckDto>();
    private bool isDeckSelectionLoading;
    private int selectedDeckIndex = -1;

    private void OnEnable()
    {
        InitializeUI();
        InitializeDeckSelectionModal();
        InitializeService();
        InitializeState();
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        UnbindDeckSelectionModal();
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

    private void InitializeDeckSelectionModal()
    {
        if (deckSelectionModalDocument == null)
        {
            GameObject modalObject = GameObject.Find(deckSelectionModalName);
            if (modalObject != null)
                deckSelectionModalDocument = modalObject.GetComponent<UIDocument>();
        }

        if (deckSelectionModalDocument == null)
        {
            Debug.LogWarning($"[HomeUI] Modal '{deckSelectionModalName}' introuvable.");
            return;
        }

        deckSelectionRoot = deckSelectionModalDocument.rootVisualElement;
        if (deckSelectionRoot == null)
        {
            Debug.LogWarning("[HomeUI] rootVisualElement du modal est null.");
            return;
        }

        deckDropdownContainer = deckSelectionRoot.Q<VisualElement>(deckDropdownContainerName);
        deckSelectionStatus = deckSelectionRoot.Q<Label>(deckSelectionStatusName);
        deckSelectionConfirmButton = deckSelectionRoot.Q<Button>("ConfirmButton");
        deckSelectionCancelButton = deckSelectionRoot.Q<Button>("CancelButton");

        if (deckDropdownContainer != null && deckDropdown == null)
        {
            deckDropdown = new DropdownField();
            deckDropdown.name = "DeckDropdown";
            deckDropdown.style.width = 330;
            deckDropdown.style.height = 46;
            deckDropdown.style.maxHeight = 46;
            deckDropdown.style.marginLeft = 0;
            deckDropdown.style.marginRight = 0;
            deckDropdown.style.marginTop = 0;
            deckDropdown.style.marginBottom = 0;
            deckDropdown.style.flexGrow = 0;
            deckDropdown.style.fontSize = 20;
            deckDropdown.style.unityTextAlign = TextAnchor.MiddleCenter;
            deckDropdown.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));
            deckDropdown.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 0f));
            deckDropdown.style.borderTopWidth = 0;
            deckDropdown.style.borderRightWidth = 0;
            deckDropdown.style.borderBottomWidth = 0;
            deckDropdown.style.borderLeftWidth = 0;
            deckDropdown.RegisterValueChangedCallback(OnDeckDropdownChanged);
            deckDropdownContainer.Add(deckDropdown);
        }

        if (deckSelectionConfirmButton != null)
            deckSelectionConfirmButton.clicked += OnDeckSelectionConfirmed;

        if (deckSelectionCancelButton != null)
            deckSelectionCancelButton.clicked += OnDeckSelectionCancelled;

        SetDeckSelectionModalVisible(false);
    }

    private void InitializeService()
    {
        service = new HomeService(networkRef, connectHereIfNeeded, verboseLogs);
        service.Initialize();
        collectionService ??= new CollectionService();
        deckService ??= new DeckService();
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

        if (deckDropdown != null)
            deckDropdown.UnregisterValueChangedCallback(OnDeckDropdownChanged);

        if (deckSelectionConfirmButton != null)
            deckSelectionConfirmButton.clicked -= OnDeckSelectionConfirmed;

        if (deckSelectionCancelButton != null)
            deckSelectionCancelButton.clicked -= OnDeckSelectionCancelled;

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

    private void OnClickSearchOpponent()
    {
        OpenDeckSelectionModal();
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

    private void OpenDeckSelectionModal()
    {
        if (isDeckSelectionLoading)
            return;

        SetButtonsEnabled(false);
        SetDeckSelectionModalVisible(true);
        if (deckSelectionStatus != null)
            deckSelectionStatus.text = "Chargement des decks...";
        SetDeckSelectionInteractable(false);
        StartCoroutine(LoadOwnedDecksAndRefreshModal());
    }

    private IEnumerator LoadOwnedDecksAndRefreshModal()
    {
        isDeckSelectionLoading = true;
        ownedDecks.Clear();
        selectableDecks.Clear();

        if (collectionService == null)
            collectionService = new CollectionService();

        if (deckService == null)
            deckService = new DeckService();

        string error = null;

        yield return collectionService.FetchUserCollectionDto(
            onSuccess: dto =>
            {
                ownedDecks.Clear();
                if (dto?.Decks != null)
                    ownedDecks.AddRange(dto.Decks);
            },
            onError: serviceError => error = serviceError
        );

        isDeckSelectionLoading = false;

        if (!string.IsNullOrWhiteSpace(error))
        {
            Debug.LogError(error);
            RefreshDeckSelectionChoices();
            SetDeckSelectionInteractable(false);
            if (deckSelectionStatus != null)
                deckSelectionStatus.text = "Erreur de chargement des decks.";
            SetButtonsEnabled(true);
            yield break;
        }

        yield return ValidateDecksForMatch();

        RefreshDeckSelectionChoices();
        SetDeckSelectionInteractable(selectableDecks.Count > 0);

        if (deckSelectionStatus != null)
        {
            int invalidDecks = Math.Max(0, ownedDecks.Count - selectableDecks.Count);
            if (selectableDecks.Count == 0)
                deckSelectionStatus.text = "Aucun deck valide (30 cartes minimum).";
            else if (invalidDecks > 0)
                deckSelectionStatus.text = $"{invalidDecks} deck(s) ignoré(s) (<30 cartes).";
            else
                deckSelectionStatus.text = "Sélectionnez un deck.";
        }

        SetButtonsEnabled(true);
    }

    private IEnumerator ValidateDecksForMatch()
    {
        selectableDecks.Clear();

        for (int i = 0; i < ownedDecks.Count; i++)
        {
            UserCollectionDeckDto deck = ownedDecks[i];
            if (deck == null || deck.DeckId == Guid.Empty)
                continue;

            DeckDataDto deckData = null;
            string error = null;

            yield return deckService.FetchDeckData(deck.DeckId, d => deckData = d, e => error = e);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogWarning($"[HomeUI] Deck ignore ({deck.DeckName}) : {error}");
                continue;
            }

            if (GetDeckCardCount(deckData) >= 30)
                selectableDecks.Add(deck);
        }
    }

    private static int GetDeckCardCount(DeckDataDto deckData)
    {
        if (deckData?.Cards == null || deckData.Cards.Count == 0)
            return 0;

        return deckData.Cards.Sum(card => Math.Max(card?.Quantity ?? 0, 1));
    }

    private void RefreshDeckSelectionChoices()
    {
        if (deckDropdown == null)
            return;

        List<string> choices = new List<string>();
        selectedDeckIndex = -1;

        for (int i = 0; i < selectableDecks.Count; i++)
        {
            UserCollectionDeckDto deck = selectableDecks[i];
            if (deck == null)
                continue;

            string label = BuildDeckLabel(deck);
            choices.Add(label);

            if (selectedDeckIndex < 0 && IsPreferredDeck(deck))
                selectedDeckIndex = choices.Count - 1;
        }

        if (choices.Count == 0)
        {
            choices.Add("Aucun deck valide (30 cartes)");
            deckDropdown.choices = choices;
            deckDropdown.SetValueWithoutNotify(choices[0]);
            deckDropdown.SetEnabled(false);
            return;
        }

        deckDropdown.choices = choices;
        deckDropdown.SetEnabled(true);

        if (selectedDeckIndex < 0 || selectedDeckIndex >= choices.Count)
            selectedDeckIndex = 0;

        deckDropdown.index = selectedDeckIndex;
        deckDropdown.SetValueWithoutNotify(choices[selectedDeckIndex]);
    }

    private static string BuildDeckLabel(UserCollectionDeckDto deck)
    {
        if (deck == null)
            return "Deck";

        string deckName = string.IsNullOrWhiteSpace(deck.DeckName) ? "Deck" : deck.DeckName.Trim();
        string shortId = deck.DeckId != Guid.Empty ? deck.DeckId.ToString("N").Substring(0, 6) : "------";
        return $"{deckName}";
    }

    private bool IsPreferredDeck(UserCollectionDeckDto deck)
    {
        if (deck == null)
            return false;

        if (!string.IsNullOrWhiteSpace(state?.DeckId) && Guid.TryParse(state.DeckId, out Guid stateDeckId) && stateDeckId == deck.DeckId)
            return true;

        return Guid.TryParse(deckId, out Guid fallbackDeckId) && fallbackDeckId == deck.DeckId;
    }

    private void OnDeckDropdownChanged(ChangeEvent<string> evt)
    {
        if (deckDropdown == null || selectableDecks.Count == 0)
            return;

        selectedDeckIndex = Mathf.Clamp(deckDropdown.index, 0, selectableDecks.Count - 1);
        UserCollectionDeckDto deck = selectableDecks[selectedDeckIndex];
    }

    private void OnDeckSelectionConfirmed()
    {
        if (selectedDeckIndex < 0 || selectedDeckIndex >= selectableDecks.Count)
            return;

        UserCollectionDeckDto selectedDeck = selectableDecks[selectedDeckIndex];

        string selectedDeckId = selectedDeck.DeckId.ToString();
        deckId = selectedDeckId;
        state?.SetDeckId(selectedDeckId);

        SetDeckSelectionModalVisible(false);
        SetButtonsEnabled(false);
        StartSearchWithDeck(selectedDeckId);
    }

    private void OnDeckSelectionCancelled()
    {
        SetDeckSelectionModalVisible(false);
        SetButtonsEnabled(true);
    }

    private async void StartSearchWithDeck(string selectedDeckId)
    {
        if (string.IsNullOrWhiteSpace(selectedDeckId))
        {
            SetButtonsEnabled(true);
            return;
        }

        state?.SetSearching(true);
        SetVisible(searchingPanel, true);

        if (service != null)
            await service.SearchOpponent(selectedDeckId);

        state?.SetSearching(false);
        SetButtonsEnabled(true);
    }

    private void SetDeckSelectionModalVisible(bool visible)
    {
        if (deckSelectionRoot != null)
            deckSelectionRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void SetDeckSelectionInteractable(bool interactable)
    {
        deckDropdown?.SetEnabled(interactable);
        deckSelectionConfirmButton?.SetEnabled(interactable);
    }


    private void UnbindDeckSelectionModal()
    {
        if (deckDropdown != null)
            deckDropdown.UnregisterValueChangedCallback(OnDeckDropdownChanged);

        if (deckSelectionConfirmButton != null)
            deckSelectionConfirmButton.clicked -= OnDeckSelectionConfirmed;

        if (deckSelectionCancelButton != null)
            deckSelectionCancelButton.clicked -= OnDeckSelectionCancelled;
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
