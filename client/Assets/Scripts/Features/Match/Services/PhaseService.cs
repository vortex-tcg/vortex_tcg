using System;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Service de gestion de l'état des phases de jeu
    /// Gère uniquement la logique métier et broadcast les événements
    /// </summary>
    public class PhaseService : MonoBehaviour
    {
        private static PhaseService _instance;

        public static PhaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find existing PhaseService in scene
                    _instance = FindFirstObjectByType<PhaseService>();
                    
                    // If not found, create it dynamically
                    if (_instance == null)
                    {
                        GameObject phaseServiceObject = new GameObject("PhaseService");
                        _instance = phaseServiceObject.AddComponent<PhaseService>();
                        Debug.Log("[PhaseService] ✅ PhaseService auto-created as it was missing from scene");
                    }
                }
                return _instance;
            }
        }

        public GamePhase CurrentPhase { get; private set; } = GamePhase.PLACEMENT;

        // Événements de phases
        public event Action OnEnterPlacement;
        public event Action OnEnterAttack;
        public event Action OnEnterDefense;
        public event Action OnEnterEndTurn;
        
        // Alias pour compatibilité avec ancien code
        public event Action OnEnterStandBy
        {
            add => OnEnterEndTurn += value;
            remove => OnEnterEndTurn -= value;
        }

        // Événement pour demander un changement de phase
        public event Action<GamePhase> OnRequestChangePhase;

        private GamePhase _previousPhase;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnEnable()
        {
            // Écoute les événements de changement de phase depuis le serveur
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
            MatchEvents.OnGameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnGameStarted -= HandleGameStarted;
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            Debug.Log($"[PhaseService] Game started with phase: {result.CurrentPhase}");
            ApplyServerPhase(result.CurrentPhase);
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            Debug.Log($"[PhaseService] Phase changed to: {result.CurrentPhase}");
            ApplyServerPhase(result.CurrentPhase);
        }

        /// <summary>
        /// Applique une phase reçue du serveur
        /// </summary>
        public void ApplyServerPhase(GamePhase newPhase)
        {
            if (CurrentPhase == newPhase)
            {
                Debug.Log($"[PhaseService] Already in phase {newPhase}, skipping");
                return;
            }

            _previousPhase = CurrentPhase;
            CurrentPhase = newPhase;

            Debug.Log($"[PhaseService] Phase transition: {_previousPhase} -> {CurrentPhase}");

            // Déclenche l'événement approprié
            InvokePhaseEvent(newPhase);
        }

        /// <summary>
        /// Demande un changement de phase (envoyé au serveur via MatchService)
        /// </summary>
        public void RequestPhaseChange(GamePhase targetPhase)
        {
            Debug.Log($"[PhaseService] Requesting phase change to: {targetPhase}");
            OnRequestChangePhase?.Invoke(targetPhase);
        }

        private void InvokePhaseEvent(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.PLACEMENT:
                    OnEnterPlacement?.Invoke();
                    Debug.Log("[PhaseService] Invoked OnEnterPlacement");
                    break;

                case GamePhase.ATTACK:
                    OnEnterAttack?.Invoke();
                    Debug.Log("[PhaseService] Invoked OnEnterAttack");
                    break;

                case GamePhase.DEFENSE:
                    OnEnterDefense?.Invoke();
                    Debug.Log("[PhaseService] Invoked OnEnterDefense");
                    break;

                case GamePhase.END_TURN:
                    OnEnterEndTurn?.Invoke();
                    Debug.Log("[PhaseService] Invoked OnEnterEndTurn");
                    break;

                default:
                    Debug.LogWarning($"[PhaseService] Unknown phase: {phase}");
                    break;
            }
        }

        /// <summary>
        /// Réinitialise le service (utile pour les tests)
        /// </summary>
        public void ResetPhase()
        {
            CurrentPhase = GamePhase.PLACEMENT;
            _previousPhase = GamePhase.PLACEMENT;
            Debug.Log("[PhaseService] Phase reset to PLACEMENT");
        }
    }
}
