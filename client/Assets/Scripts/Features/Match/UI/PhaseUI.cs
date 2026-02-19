using System;
using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;

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

        private VisualElement placementIcon;
        private VisualElement attackIcon;
        private VisualElement defenseIcon;
        private VisualElement endTurnIcon;
        private Button endPhaseButton;

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
            if (endPhaseButton != null)
            {
                endPhaseButton.clicked -= HandleEndPhaseButtonClicked;
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
            
            // Récupérer les éléments d'icônes
            placementIcon = root.Q<VisualElement>("Placement");
            attackIcon = root.Q<VisualElement>("Attack");
            defenseIcon = root.Q<VisualElement>("Defense");
            endTurnIcon = root.Q<VisualElement>("EndTurn");

            // Récupérer et binder le bouton EndPhase
            endPhaseButton = root.Q<Button>("EndPhaseButton");
            if (endPhaseButton != null)
            {
                endPhaseButton.clicked -= HandleEndPhaseButtonClicked;
                endPhaseButton.clicked += HandleEndPhaseButtonClicked;
                Debug.Log("[PhaseUI] EndPhaseButton bound");
            }
            else
            {
                Debug.LogWarning("[PhaseUI] EndPhaseButton not found in UI");
            }

            UpdateIcons(_currentPhase);
        }

        // ========== EVENT HANDLERS ==========

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            _currentPhase = result.CurrentPhase;
            UpdateIcons(_currentPhase);
            UpdateButtonLabel(_currentPhase);
            Debug.Log($"[PhaseUI] Game started - Phase: {_currentPhase}");
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            _currentPhase = result.CurrentPhase;
            UpdateIcons(_currentPhase);
            UpdateButtonLabel(_currentPhase);
            Debug.Log($"[PhaseUI] Phase changed - New phase: {_currentPhase}");
        }

        private void HandleEndPhaseButtonClicked()
        {
            Debug.Log("[PhaseUI] End Phase button clicked - requesting phase change");
            
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

        // ========== UI UPDATE ==========

        private void UpdateIcons(GamePhase phase)
        {
            SetHighlight(placementIcon, phase == GamePhase.PLACEMENT);
            SetHighlight(attackIcon, phase == GamePhase.ATTACK);
            SetHighlight(defenseIcon, phase == GamePhase.DEFENSE);
            SetHighlight(endTurnIcon, phase == GamePhase.END_TURN);
        }

        private void UpdateButtonLabel(GamePhase phase)
        {
            if (endPhaseButton == null) return;

            string label = phase switch
            {
                GamePhase.PLACEMENT => "End Placement",
                GamePhase.ATTACK => "End Attack",
                GamePhase.DEFENSE => "End Defense",
                GamePhase.END_TURN => "End Turn",
                _ => "Next Phase"
            };

            endPhaseButton.text = label;
        }

        private static void SetHighlight(VisualElement icon, bool active)
        {
            if (icon == null) return;
            icon.style.opacity = active ? 1f : 0.3f;
        }
    }
}
