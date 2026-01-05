using UnityEngine;
using UnityEngine.UIElements;

public class UILoadingScene : MonoBehaviour
{
    [Header("UI Toolkit")]
    [SerializeField] private UIDocument uiDocument;

    public LoadingRequest menuParams;

    private UpMenuStatus status = UpMenuStatus.MAIN;

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
            Debug.LogError("UILoadingScene : UIDocument non assign�.");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("UILoadingScene : rootVisualElement null.");
            return;
        }

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

        status = menuParams != null ? menuParams.status : UpMenuStatus.MAIN;
        ApplyStatusVisual(status);

        if (playButton != null) playButton.clicked += OnPlayClicked;
        if (profileButton != null) profileButton.clicked += OnProfileClicked;
        if (collectionButton != null) collectionButton.clicked += OnCollectionClicked;
        if (friendsButton != null) friendsButton.clicked += OnFriendsClicked;

        if (cartIcon != null) cartIcon.RegisterCallback<ClickEvent>(OnCartClicked);
        if (settingsIcon != null) settingsIcon.RegisterCallback<ClickEvent>(OnSettingsClicked);
    }

    private void OnDisable()
    {
        if (playButton != null) playButton.clicked -= OnPlayClicked;
        if (profileButton != null) profileButton.clicked -= OnProfileClicked;
        if (collectionButton != null) collectionButton.clicked -= OnCollectionClicked;
        if (friendsButton != null) friendsButton.clicked -= OnFriendsClicked;

        if (cartIcon != null) cartIcon.UnregisterCallback<ClickEvent>(OnCartClicked);
        if (settingsIcon != null) settingsIcon.UnregisterCallback<ClickEvent>(OnSettingsClicked);
    }

    private void OnPlayClicked()
    {
        CallLoadScene("Scene/MainPage", UpMenuStatus.MAIN);
    }

    private void OnProfileClicked()
    {
        CallLoadScene("Scene/ProfilScene", UpMenuStatus.PROFIL);
    }

    private void OnCollectionClicked()
    {
        CallLoadScene("Scene/CollectionScene", UpMenuStatus.COLLECTION);
    }

    private void OnFriendsClicked()
    {
        CallLoadScene("Scene/FriendsScene", UpMenuStatus.FRIENDS);
    }

    private void OnCartClicked(ClickEvent evt)
    {
        CallLoadScene("Scene/MarketScene", UpMenuStatus.MARKET);
    }

    private void OnSettingsClicked(ClickEvent evt)
    {
        CallLoadScene("Scene/OptionScene", UpMenuStatus.OPTIONS);
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

    private void CallLoadScene(string loadScene, UpMenuStatus newStatus)
    { 
        if (menuParams != null)
            menuParams.status = newStatus;

        LoadingScreen.Load(loadScene, loadMenu: true, unloadMenu: false);
    }

    public UpMenuStatus GetStatus()
    {
        return status;
    }
}
