using System;
using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.Features.Match.UI
{
    /// <summary>
    /// Gestionnaire UI des phases du match
    /// Remplace l'ancien PhaseManager en utilisant MatchEvents
    /// </summary>
    public class PhaseUI : MonoBehaviour
    {
        public static PhaseUI Instance { get; private set; }

        [Header("UI Toolkit")]
        [SerializeField] private UIDocument uiDoc;

        private VisualElement endTurnButton;
        private Label matchPhaseLabel;
        private float endTurnDefaultOpacity = 1f;
        private Scale endTurnDefaultScale = new Scale(Vector3.one);

        private GamePhase _currentPhase = GamePhase.PLACEMENT;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            Debug.Log("[PhaseUI] OnEnable");

            // Bind UI Toolkit elements
            BindUIElements();

            // S'abonner aux événements
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
            MatchEvents.OnGameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            // Se désabonner
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnGameStarted -= HandleGameStarted;

            // Débinder UI
            if (endTurnButton != null)
            {
                endTurnButton.UnregisterCallback<ClickEvent>(HandleEndTurnClicked);
                endTurnButton.UnregisterCallback<PointerDownEvent>(HandleEndTurnPointerDown);
                endTurnButton.UnregisterCallback<PointerUpEvent>(HandleEndTurnPointerUp);
                endTurnButton.UnregisterCallback<PointerLeaveEvent>(HandleEndTurnPointerLeave);
            }
        }

        private void BindUIElements()
        {
            if (uiDoc == null)
                uiDoc = GetComponent<UIDocument>();

            if (uiDoc == null)
            {
                Debug.LogError("[PhaseUI] UIDocument not found");
                return;
            }

            VisualElement root = uiDoc.rootVisualElement;

            // Récupérer et binder le bouton EndTurn
            endTurnButton = root.Q<VisualElement>("EndTurnButton");
            if (endTurnButton != null)
            {
                endTurnDefaultOpacity = endTurnButton.resolvedStyle.opacity;
                endTurnDefaultScale = endTurnButton.resolvedStyle.scale;

                endTurnButton.UnregisterCallback<ClickEvent>(HandleEndTurnClicked);
                endTurnButton.RegisterCallback<ClickEvent>(HandleEndTurnClicked);
                endTurnButton.UnregisterCallback<PointerDownEvent>(HandleEndTurnPointerDown);
                endTurnButton.UnregisterCallback<PointerUpEvent>(HandleEndTurnPointerUp);
                endTurnButton.UnregisterCallback<PointerLeaveEvent>(HandleEndTurnPointerLeave);
                endTurnButton.RegisterCallback<PointerDownEvent>(HandleEndTurnPointerDown);
                endTurnButton.RegisterCallback<PointerUpEvent>(HandleEndTurnPointerUp);
                endTurnButton.RegisterCallback<PointerLeaveEvent>(HandleEndTurnPointerLeave);
                Debug.Log("[PhaseUI] EndTurnButton bound");
            }
            else
            {
                Debug.LogWarning("[PhaseUI] EndTurnButton not found in UI");
            }

            // Récupérer le label de phase
            matchPhaseLabel = root.Q<Label>("MatchPhase");

            // Initialiser la phase depuis PhaseService (source de vérité)
            PhaseService phaseService = PhaseService.Instance;
            if (phaseService != null)
            {
                _currentPhase = phaseService.CurrentPhase;
                Debug.Log($"[PhaseUI] Initialized phase from PhaseService: {_currentPhase}");
            }

            UpdatePhaseLabel(_currentPhase);
        }

        // ========== EVENT HANDLERS ==========

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            _currentPhase = result.CurrentPhase;
            UpdatePhaseLabel(_currentPhase);
            Debug.Log($"[PhaseUI] Game started - Phase: {_currentPhase}");
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            _currentPhase = result.CurrentPhase;
            UpdatePhaseLabel(_currentPhase);
            Debug.Log($"[PhaseUI] Phase changed - New phase: {_currentPhase}");
        }

        private void HandleEndTurnClicked(ClickEvent evt)
        {
            Debug.Log("[PhaseUI] End Phase button clicked - requesting phase change");
            RestoreEndTurnEffect();
            
            // Call server to change phase
            SignalRClient client = SignalRClient.Instance;
            if (client != null && client.IsConnected)
            {
                _ = client.ChangePhase();
                Debug.Log("[PhaseUI] ✅ ChangePhase request sent to server");
            }
            else
            {
                Debug.LogWarning("[PhaseUI] ❌ Cannot change phase - SignalRClient not connected");
            }
            
            // Also fire event for any local listeners
            MatchEvents.FirePhaseChangeRequested();
        }

        private void HandleEndTurnPointerDown(PointerDownEvent evt)
        {
            if (endTurnButton == null) return;
            endTurnButton.style.opacity = 0.9f;
            endTurnButton.style.scale = new Scale(new Vector3(0.97f, 0.97f, 1f));
        }

        private void HandleEndTurnPointerUp(PointerUpEvent evt)
        {
            RestoreEndTurnEffect();
        }

        private void HandleEndTurnPointerLeave(PointerLeaveEvent evt)
        {
            RestoreEndTurnEffect();
        }

        private void RestoreEndTurnEffect()
        {
            if (endTurnButton == null) return;
            endTurnButton.style.opacity = endTurnDefaultOpacity;
            endTurnButton.style.scale = endTurnDefaultScale;
        }

        // ========== UI UPDATE ==========

        private void UpdatePhaseLabel(GamePhase phase)
        {
            if (matchPhaseLabel == null) return;

            string label = phase switch
            {
                GamePhase.PLACEMENT => "STAND BY PHASE",
                GamePhase.ATTACK => "ATTACK PHASE",
                GamePhase.DEFENSE => "DEFENSE PHASE",
                GamePhase.END_TURN => "END TURN PHASE",
                _ => "PHASE"
            };

            matchPhaseLabel.text = label;
        }

    }
}
