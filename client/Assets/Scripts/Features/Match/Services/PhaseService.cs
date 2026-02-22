using System;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;

namespace VortexTCG.Scripts.Features.Match.Services
{
    public class PhaseService : MonoBehaviour
    {
        private static PhaseService _instance;
        private static bool _isQuitting = false;

        public static PhaseService Instance
        {
            get
            {
                if (_isQuitting)
                {
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PhaseService>();
                    
                    if (_instance == null && !_isQuitting)
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

        public event Action OnEnterPlacement;
        public event Action OnEnterAttack;
        public event Action OnEnterDefense;
        public event Action OnEnterEndTurn;
        
        public event Action OnEnterStandBy
        {
            add => OnEnterEndTurn += value;
            remove => OnEnterEndTurn -= value;
        }

        public event Action<GamePhase> OnRequestChangePhase;

        private GamePhase _previousPhase;

        private void Awake()
        {
            _isQuitting = false;

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _isQuitting = true;
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnEnable()
        {
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

            InvokePhaseEvent(newPhase);
        }

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

        public void ResetPhase()
        {
            CurrentPhase = GamePhase.PLACEMENT;
            _previousPhase = GamePhase.PLACEMENT;
            Debug.Log("[PhaseService] Phase reset to PLACEMENT");
        }
    }
}
