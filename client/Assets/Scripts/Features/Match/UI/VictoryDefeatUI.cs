using UnityEngine;
using UnityEngine.UIElements;

namespace VortexTCG.Scripts.Features.Match.UI
{
    public class MatchEndingScreenUI : MonoBehaviour
    {
        public static MatchEndingScreenUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Fade")]
        [SerializeField, Min(0.05f)] private float fadeDuration = 0.8f;
        [SerializeField, Min(0.05f)] private float quitButtonFadeDuration = 2f;

        [Header("Background Textures (optional)")]
        [SerializeField] private Texture2D victoryTexture;
        [SerializeField] private Texture2D defeatTexture;

        [Header("Resources Fallback")]
        [SerializeField] private string victoryResourcePath = "GUI/VictoryScreen";
        [SerializeField] private string defeatResourcePath = "GUI/DefeatScreen";

        private VisualElement _endingScreen;
        private Button _quitButton;
        private VisualElement _root;
        private bool _isVisible;
        private bool _isQuitButtonFading;
        private float _fadeElapsed;
        private float _quitButtonFadeElapsed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument == null)
            {
                Debug.LogError("[MatchEndingScreenUI] UIDocument missing.");
                return;
            }

            _root = uiDocument.rootVisualElement;
            _root.pickingMode = PickingMode.Ignore;

            _endingScreen = _root.Q<VisualElement>("EndingScreen");
            _quitButton = _root.Q<Button>("QuitButton");

            if (_endingScreen == null)
            {
                Debug.LogError("[MatchEndingScreenUI] EndingScreen element not found in UXML.");
                return;
            }

            _endingScreen.style.opacity = 0f;
            _endingScreen.style.display = DisplayStyle.None;
            _endingScreen.visible = false;
            _endingScreen.pickingMode = PickingMode.Ignore;

            if (_quitButton != null)
            {
                _quitButton.style.opacity = 0f;
                _quitButton.style.display = DisplayStyle.None;
                _quitButton.visible = false;
                _quitButton.pickingMode = PickingMode.Ignore;
            }
        }

        private void Update()
        {
            if (!_isVisible || _endingScreen == null)
            {
                return;
            }

            if (_endingScreen.style.opacity.value < 1f)
            {
                _fadeElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_fadeElapsed / fadeDuration);
                _endingScreen.style.opacity = t;
            }

            if (_isQuitButtonFading && _quitButton != null)
            {
                _quitButtonFadeElapsed += Time.deltaTime;
                float buttonOpacity = Mathf.Clamp01(_quitButtonFadeElapsed / quitButtonFadeDuration);
                _quitButton.style.opacity = buttonOpacity;

                if (buttonOpacity >= 1f)
                {
                    _isQuitButtonFading = false;
                    _quitButton.SetEnabled(true);
                }
            }
        }

        public void ShowEndingScreen(bool hasWon)
        {
            if (_endingScreen == null)
            {
                return;
            }

            Texture2D texture = hasWon ? ResolveVictoryTexture() : ResolveDefeatTexture();
            if (texture != null)
            {
                _endingScreen.style.backgroundImage = new StyleBackground(texture);
            }

            _isVisible = true;
            _fadeElapsed = 0f;
            _root.pickingMode = PickingMode.Position;
            _endingScreen.style.opacity = 0f;
            _endingScreen.style.display = DisplayStyle.Flex;
            _endingScreen.visible = true;
            _endingScreen.pickingMode = PickingMode.Position;

            if (_quitButton != null)
            {
                _quitButtonFadeElapsed = 0f;
                _isQuitButtonFading = true;
                _quitButton.style.opacity = 0f;
                _quitButton.style.display = DisplayStyle.Flex;
                _quitButton.visible = true;
                _quitButton.pickingMode = PickingMode.Position;
                _quitButton.SetEnabled(false);
            }

            Debug.Log($"[MatchEndingScreenUI] ShowEndingScreen hasWon={hasWon}");
        }

        public void HideEndingScreenImmediate()
        {
            if (_endingScreen == null)
            {
                return;
            }

            _isVisible = false;
            _isQuitButtonFading = false;
            _fadeElapsed = 0f;
            _quitButtonFadeElapsed = 0f;
            if (_root != null)
            {
                _root.pickingMode = PickingMode.Ignore;
            }
            _endingScreen.style.opacity = 0f;
            _endingScreen.style.display = DisplayStyle.None;
            _endingScreen.visible = false;
            _endingScreen.pickingMode = PickingMode.Ignore;

            if (_quitButton != null)
            {
                _quitButton.style.opacity = 0f;
                _quitButton.style.display = DisplayStyle.None;
                _quitButton.visible = false;
                _quitButton.pickingMode = PickingMode.Ignore;
                _quitButton.SetEnabled(true);
            }
        }

        private Texture2D ResolveVictoryTexture()
        {
            if (victoryTexture != null)
            {
                return victoryTexture;
            }

            return Resources.Load<Texture2D>(victoryResourcePath);
        }

        private Texture2D ResolveDefeatTexture()
        {
            if (defeatTexture != null)
            {
                return defeatTexture;
            }

            return Resources.Load<Texture2D>(defeatResourcePath);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
