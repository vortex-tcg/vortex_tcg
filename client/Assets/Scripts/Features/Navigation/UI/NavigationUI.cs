using UnityEngine;
using UnityEngine.UIElements;

public class NavigationUI : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    public LoadingRequest menuParams;

    private NavigationService navigationService;
    private NavigationState navigationState;
    private EventBus eventBus = EventBus.Instance;

    private Button playButton;
    private Button profileButton;
    private Button collectionButton;
    private Button friendsButton;

    private VisualElement cartIcon;
    private VisualElement settingsIcon;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument non assigné.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("rootVisualElement null.");
            return;
        }

        UpMenuStatus initialStatus = menuParams != null ? menuParams.status : UpMenuStatus.MAIN;
        navigationState = new NavigationState(initialStatus);
        navigationService = new NavigationService(navigationState);

        eventBus.Subscribe<NavigationStatusChangedEvent>(OnNavigationStatusChanged);

        playButton = root.Q<Button>("Play");
        profileButton = root.Q<Button>("Profile");
        collectionButton = root.Q<Button>("Collection");
        friendsButton = root.Q<Button>("Friends");

        cartIcon = root.Q<VisualElement>("CartIcon");
        settingsIcon = root.Q<VisualElement>("SettingsIcon");

        if (playButton == null) Debug.LogError("Button 'Play' introuvable.");
        if (profileButton == null) Debug.LogError("Button 'Profile' introuvable.");
        if (collectionButton == null) Debug.LogError("Button 'Collection' introuvable.");
        if (friendsButton == null) Debug.LogError("Button 'Friends' introuvable.");
        if (cartIcon == null) Debug.LogError("VisualElement 'CartIcon' introuvable.");
        if (settingsIcon == null) Debug.LogError("VisualElement 'SettingsIcon' introuvable.");

        ApplyStatusVisual(navigationState.GetStatus());

        if (playButton != null) playButton.clicked += OnHomeClicked;
        if (profileButton != null) profileButton.clicked += OnProfileClicked;
        if (collectionButton != null) collectionButton.clicked += OnCollectionClicked;
        if (friendsButton != null) friendsButton.clicked += OnFriendsClicked;

        if (cartIcon != null) cartIcon.RegisterCallback<ClickEvent>(OnCartClicked);
        if (settingsIcon != null) settingsIcon.RegisterCallback<ClickEvent>(OnSettingsClicked);
    }

    private void OnDisable()
    {
        eventBus.Unsubscribe<NavigationStatusChangedEvent>(OnNavigationStatusChanged);

        if (playButton != null) playButton.clicked -= OnHomeClicked;
        if (profileButton != null) profileButton.clicked -= OnProfileClicked;
        if (collectionButton != null) collectionButton.clicked -= OnCollectionClicked;
        if (friendsButton != null) friendsButton.clicked -= OnFriendsClicked;

        if (cartIcon != null) cartIcon.UnregisterCallback<ClickEvent>(OnCartClicked);
        if (settingsIcon != null) settingsIcon.UnregisterCallback<ClickEvent>(OnSettingsClicked);
    }

    private void OnHomeClicked()
    {
        eventBus.Publish(new NavigationRequestedEvent("Scenes/HomeScene", UpMenuStatus.MAIN));
    }

    private void OnProfileClicked()
    {
        eventBus.Publish(new NavigationRequestedEvent("Scenes/ProfileScene", UpMenuStatus.PROFIL));
    }

    private void OnCollectionClicked()
    {
        eventBus.Publish(new NavigationRequestedEvent("Scenes/CollectionScene", UpMenuStatus.COLLECTION));
    }

    private void OnFriendsClicked()
    {
        eventBus.Publish(new NavigationRequestedEvent("Scenes/FriendsScene", UpMenuStatus.FRIENDS));
    }

    private void OnCartClicked(ClickEvent evt)
    {
        eventBus.Publish(new NavigationRequestedEvent("Scenes/MarketScene", UpMenuStatus.MARKET));
    }

    private void OnSettingsClicked(ClickEvent evt)
    {
        eventBus.Publish(new NavigationRequestedEvent("Scenes/OptionScene", UpMenuStatus.OPTIONS));
    }

    private void OnNavigationStatusChanged(NavigationStatusChangedEvent evt)
    {
        ApplyStatusVisual(evt.NewStatus);
    }

    private void ApplyStatusVisual(UpMenuStatus s)
    {
        ClearVisualState();

        if (s == UpMenuStatus.MAIN && playButton != null)
            playButton.AddToClassList("active");
        else if (s == UpMenuStatus.PROFIL && profileButton != null)
            profileButton.AddToClassList("active");
        else if (s == UpMenuStatus.COLLECTION && collectionButton != null)
            collectionButton.AddToClassList("active");
        else if (s == UpMenuStatus.FRIENDS && friendsButton != null)
            friendsButton.AddToClassList("active");
        else if (s == UpMenuStatus.MARKET && cartIcon != null)
            cartIcon.AddToClassList("underline");
        else if (s == UpMenuStatus.OPTIONS && settingsIcon != null)
            settingsIcon.AddToClassList("underline");
    }

    private void ClearVisualState()
    {
        if (playButton != null) playButton.RemoveFromClassList("active");
        if (profileButton != null) profileButton.RemoveFromClassList("active");
        if (collectionButton != null) collectionButton.RemoveFromClassList("active");
        if (friendsButton != null) friendsButton.RemoveFromClassList("active");

        if (cartIcon != null) cartIcon.RemoveFromClassList("underline");
        if (settingsIcon != null) settingsIcon.RemoveFromClassList("underline");
    }

    public UpMenuStatus GetStatus()
    {
        return navigationState.GetStatus();
    }
}
