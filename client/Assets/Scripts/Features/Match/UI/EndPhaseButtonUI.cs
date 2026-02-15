using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.MatchScene
{
    public class EndPhaseButtonUI : MonoBehaviour
    {
        private Button endPhaseButton;

        private void OnEnable()
        {
            UIDocument doc = GetComponent<UIDocument>();
            if (doc == null) return;

            VisualElement root = doc.rootVisualElement;
            endPhaseButton = root.Q<Button>("EndPhaseButton");

            if (endPhaseButton == null)
                Debug.LogWarning("Bouton EndPhaseButton introuvable dans le UXML !");
            else
                RefreshLabel();

            // S'abonner aux événements de la nouvelle architecture
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
            MatchEvents.OnGameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            // Se désabonner
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnGameStarted -= HandleGameStarted;
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            RefreshLabel();
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (endPhaseButton == null) return;

            PhaseService phaseService = PhaseService.Instance;
            if (phaseService == null)
            {
                Debug.LogWarning("[EndPhaseButtonHandler] PhaseService not found");
                return;
            }

            GamePhase phase = phaseService.CurrentPhase;
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
    }
}