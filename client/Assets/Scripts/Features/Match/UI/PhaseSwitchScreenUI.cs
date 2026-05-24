using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;

namespace VortexTCG.Scripts.Features.Match.UI
{
    /// <summary>
    /// Controls a dedicated Phase HUD visual that changes texture on phase change.
    /// Default timing is 4 seconds total: 1s fade-in, 2s hold, 1s fade-out.
    /// </summary>
    public class PhaseSwitchScreenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string phaseElementName = "PhaseScreen";
        [SerializeField] private string phaseBackgroundElementName = "PhaseBackground";

        [Header("Textures (optional)")]
        [SerializeField] private Texture2D standByTexture;
        [SerializeField] private Texture2D attackTexture;
        [SerializeField] private Texture2D defenseTexture;
        [SerializeField] private Texture2D endingTexture;

        [Header("Resources fallback")]
        [SerializeField] private string standByResourcePath = "GUI/StandByPhase";
        [SerializeField] private string attackResourcePath = "GUI/AttackPhase";
        [SerializeField] private string defenseResourcePath = "GUI/DefensePhase";
        [SerializeField] private string endingResourcePath = "GUI/EndingPhase";

        [Header("Background Textures (optional)")]
        [SerializeField] private Texture2D standByBackgroundTexture;
        [SerializeField] private Texture2D attackBackgroundTexture;
        [SerializeField] private Texture2D defenseBackgroundTexture;
        [SerializeField] private Texture2D endingBackgroundTexture;

        [Header("Background Resources fallback")]
        [SerializeField] private string standByBackgroundResourcePath = "GUI/StandByPhaseBackground";
        [SerializeField] private string attackBackgroundResourcePath = "GUI/AttackPhaseBackground";
        [SerializeField] private string defenseBackgroundResourcePath = "GUI/DefensePhaseBackground";
        [SerializeField] private string endingBackgroundResourcePath = "GUI/EndingPhaseBackground";

        [Header("Fade")]
        [SerializeField, Min(0.05f)] private float fadeInSeconds = 1f;
        [SerializeField, Min(0f)] private float holdSeconds = 2f;
        [SerializeField, Min(0.05f)] private float fadeOutSeconds = 1f;

        private VisualElement _phaseElement;
        private VisualElement _phaseBackgroundElement;
        private float _elapsed;
        private bool _isVisible;
        private bool _hasKnownPhase;
        private GamePhase _lastKnownPhase;

        private float TotalDuration => fadeInSeconds + holdSeconds + fadeOutSeconds;

        private void Awake()
        {
            Debug.Log($"[PhaseSwitchScreenUI] Awake on '{gameObject.name}' activeSelf={gameObject.activeSelf} enabled={enabled}");
        }

        private void OnEnable()
        {
            Debug.Log($"[PhaseSwitchScreenUI] OnEnable on '{gameObject.name}' - subscribing to phase events");
            BindUI();
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            Debug.Log($"[PhaseSwitchScreenUI] OnDisable on '{gameObject.name}' - unsubscribing from phase events");
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            if (result == null)
            {
                return;
            }

            // Baseline only: do not show on game start.
            _lastKnownPhase = result.CurrentPhase;
            _hasKnownPhase = true;
            Debug.Log($"[PhaseSwitchScreenUI] Game started baseline phase={_lastKnownPhase} (no display)");
        }

        private void Update()
        {
            if (!_isVisible || _phaseElement == null)
            {
                return;
            }

            _elapsed += Time.deltaTime;

            float fadeInEnd = fadeInSeconds;
            float holdEnd = fadeInEnd + holdSeconds;
            float fadeOutEnd = holdEnd + fadeOutSeconds;
            float opacity;

            if (_elapsed <= fadeInEnd)
            {
                opacity = Mathf.Clamp01(_elapsed / fadeInSeconds);
            }
            else if (_elapsed <= holdEnd)
            {
                opacity = 1f;
            }
            else
            {
                float fadeProgress = (_elapsed - holdEnd) / fadeOutSeconds;
                opacity = Mathf.Clamp01(1f - fadeProgress);
            }

            _phaseElement.style.opacity = opacity;
            if (_phaseBackgroundElement != null)
            {
                _phaseBackgroundElement.style.opacity = opacity;
            }

            if (_elapsed >= fadeOutEnd)
            {
                HideImmediate();
            }
        }

        private void BindUI()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument == null)
            {
                Debug.LogError("[PhaseSwitchScreenUI] UIDocument missing.");
                return;
            }

            Debug.Log($"[PhaseSwitchScreenUI] BindUI using UIDocument on '{gameObject.name}'");

            VisualElement root = uiDocument.rootVisualElement;
            _phaseElement = root.Q<VisualElement>(phaseElementName)
                ?? root.Q<VisualElement>("PhaseName")
                ?? root.Q<VisualElement>("SwitchingPhase");
            _phaseBackgroundElement = root.Q<VisualElement>(phaseBackgroundElementName)
                ?? root.Q<VisualElement>("Background");

            if (_phaseElement == null)
            {
                Debug.LogError($"[PhaseSwitchScreenUI] Element '{phaseElementName}' not found (fallbacks: PhaseName, SwitchingPhase).");
                return;
            }

            HideImmediate();
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            if (result == null)
            {
                Debug.LogWarning("[PhaseSwitchScreenUI] HandlePhaseChanged received null result.");
                return;
            }

            if (result.CurrentPhase == GamePhase.STAND_BY)
            {
                _lastKnownPhase = result.CurrentPhase;
                _hasKnownPhase = true;
                HideImmediate();
                Debug.Log("[PhaseSwitchScreenUI] StandBy phase received, hiding display.");
                return;
            }

            if (_hasKnownPhase && result.CurrentPhase == _lastKnownPhase)
            {
                Debug.Log($"[PhaseSwitchScreenUI] Phase unchanged ({result.CurrentPhase}), skipping display.");
                return;
            }

            _lastKnownPhase = result.CurrentPhase;
            _hasKnownPhase = true;

            Debug.Log($"[PhaseSwitchScreenUI] Phase event received: {result.CurrentPhase}");
            ShowPhase(result.CurrentPhase);
        }

        public void ShowPhase(GamePhase phase)
        {
            if (_phaseElement == null)
            {
                return;
            }

            Texture2D texture = ResolveTexture(phase);
            if (texture == null)
            {
                Debug.LogWarning($"[PhaseSwitchScreenUI] Missing texture for phase '{phase}'.");
                return;
            }

            _phaseElement.style.backgroundImage = new StyleBackground(texture);
            _phaseElement.style.display = DisplayStyle.Flex;
            _phaseElement.visible = true;
            _phaseElement.style.opacity = 0f;

            if (_phaseBackgroundElement != null)
            {
                Texture2D backgroundTexture = ResolveBackgroundTexture(phase);
                if (backgroundTexture != null)
                {
                    _phaseBackgroundElement.style.backgroundImage = new StyleBackground(backgroundTexture);
                }

                _phaseBackgroundElement.style.display = DisplayStyle.Flex;
                _phaseBackgroundElement.visible = true;
                _phaseBackgroundElement.style.opacity = 0f;
            }

            _elapsed = 0f;
            _isVisible = true;

            Debug.Log($"[PhaseSwitchScreenUI] ShowPhase {phase}");
        }

        public void HideImmediate()
        {
            _isVisible = false;
            _elapsed = 0f;

            if (_phaseElement == null)
            {
                return;
            }

            _phaseElement.style.opacity = 0f;
            _phaseElement.style.display = DisplayStyle.None;
            _phaseElement.visible = false;

            if (_phaseBackgroundElement != null)
            {
                _phaseBackgroundElement.style.opacity = 0f;
                _phaseBackgroundElement.style.display = DisplayStyle.None;
                _phaseBackgroundElement.visible = false;
            }
        }

        private Texture2D ResolveTexture(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.STAND_BY => standByTexture != null ? standByTexture : Resources.Load<Texture2D>(standByResourcePath),
                GamePhase.ATTACK => attackTexture != null ? attackTexture : Resources.Load<Texture2D>(attackResourcePath),
                GamePhase.DEFENSE => defenseTexture != null ? defenseTexture : Resources.Load<Texture2D>(defenseResourcePath),
                GamePhase.END_TURN => endingTexture != null ? endingTexture : Resources.Load<Texture2D>(endingResourcePath),
                _ => null
            };
        }

        private Texture2D ResolveBackgroundTexture(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.STAND_BY => standByBackgroundTexture != null ? standByBackgroundTexture : Resources.Load<Texture2D>(standByBackgroundResourcePath),
                GamePhase.ATTACK => attackBackgroundTexture != null ? attackBackgroundTexture : Resources.Load<Texture2D>(attackBackgroundResourcePath),
                GamePhase.DEFENSE => defenseBackgroundTexture != null ? defenseBackgroundTexture : Resources.Load<Texture2D>(defenseBackgroundResourcePath),
                GamePhase.END_TURN => endingBackgroundTexture != null ? endingBackgroundTexture : Resources.Load<Texture2D>(endingBackgroundResourcePath),
                _ => null
            };
        }
    }
}