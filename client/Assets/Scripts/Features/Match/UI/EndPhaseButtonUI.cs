using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.MatchScene
{
    public class EndPhaseButtonUI : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument; // Assign HUD UIDocument here
        private Button endPhaseButton;

        private void OnEnable()
        {
            Debug.Log("[EndPhaseButtonHandler] OnEnable - Binding UI and subscribing to events");
            
            // Try assigned UIDocument first, then fallback to GetComponent
            UIDocument doc = uiDocument;
            if (doc == null)
            {
                doc = GetComponent<UIDocument>();
                if (doc == null)
                {
                    Debug.LogError("[EndPhaseButtonHandler] ❌ UIDocument not found! Please assign HUD UIDocument in inspector");
                    return;
                }
                else
                {
                    Debug.Log("[EndPhaseButtonHandler] ✅ Found UIDocument via GetComponent");
                }
            }
            else
            {
                Debug.Log("[EndPhaseButtonHandler] ✅ Using assigned UIDocument");
            }

            VisualElement root = doc.rootVisualElement;
            endPhaseButton = root.Q<Button>("EndPhaseButton");

            if (endPhaseButton == null)
            {
                Debug.LogError("[EndPhaseButtonHandler] ❌ Button 'EndPhaseButton' not found in UXML!");
                return;
            }
            
            Debug.Log("[EndPhaseButtonHandler] ✅ Button found - binding click event");
            
            // Bind click handler
            endPhaseButton.clicked -= HandleEndPhaseButtonClicked;
            endPhaseButton.clicked += HandleEndPhaseButtonClicked;
            
            RefreshLabel();

            // S'abonner aux événements de la nouvelle architecture
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
            MatchEvents.OnGameStarted += HandleGameStarted;
            
            Debug.Log("[EndPhaseButtonHandler] ✅ Subscribed to MatchEvents");
            Debug.Log($"[EndPhaseButtonHandler] PhaseService.Instance: {(PhaseService.Instance != null ? "EXISTS" : "NULL")}");
            
            Debug.Log("[EndPhaseButtonHandler] ✅ Fully initialized and ready");
        }

        private void OnDisable()
        {
            if (endPhaseButton != null)
            {
                endPhaseButton.clicked -= HandleEndPhaseButtonClicked;
            }
            
            // Se désabonner
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnGameStarted -= HandleGameStarted;
        }

        private void HandleEndPhaseButtonClicked()
        {
                 // Call server to change phase
            SignalRClient client = SignalRClient.Instance;
            if (client != null && client.IsConnected)
            {
                Debug.Log("[EndPhaseButtonHandler] Sending ChangePhase to server...");
                _ = client.ChangePhase();
                Debug.Log("[EndPhaseButtonHandler] ✅ ChangePhase request sent to server");
            }
            else
            {
                Debug.LogWarning("[EndPhaseButtonHandler] ❌ Cannot change phase - SignalRClient not connected");
            }
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            Debug.Log($"[EndPhaseButtonHandler] HandleGameStarted called - phase={result?.CurrentPhase}");
            RefreshLabel();
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            Debug.Log($"[EndPhaseButtonHandler] HandlePhaseChanged called - phase={result?.CurrentPhase}");
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (endPhaseButton == null) 
            {
                Debug.LogError("[EndPhaseButtonHandler] ❌ endPhaseButton is NULL!");
                return;
            }

            PhaseService phaseService = PhaseService.Instance;
            if (phaseService == null)
            {
                Debug.LogError("[EndPhaseButtonHandler] ❌ PhaseService.Instance is NULL!");
                return;
            }

            GamePhase phase = phaseService.CurrentPhase;
            Debug.Log($"[EndPhaseButtonHandler] RefreshLabel - Current phase: {phase}");
            
            string label = phase switch
            {
                GamePhase.PLACEMENT => "End Placement",
                GamePhase.ATTACK => "End Attack",
                GamePhase.DEFENSE => "End Defense",
                GamePhase.END_TURN => "End Turn",
                _ => "Next Phase"
            };

            Debug.Log($"[EndPhaseButtonHandler] ✅ Changing button label to: '{label}'");
            endPhaseButton.text = label;
        }
    }
}