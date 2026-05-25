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

        public GamePhase CurrentPhase { get; private set; } = GamePhase.STAND_BY;
        public bool CanAct { get; private set; }

        // The current turn number reported by the server (1-based)
        public int CurrentTurn { get; private set; } = 0;
        public event Action<int> OnTurnChanged;

        public event Action OnEnterAttack;
        public event Action OnEnterDefense;
        public event Action OnEnterEndTurn;
        public event Action OnEnterStandBy;

        public event Action<GamePhase> OnRequestChangePhase;

        private GamePhase _previousPhase;

        private void UpdateTurn(int newTurn)
        {
            if (newTurn <= 0) return;
            if (newTurn == CurrentTurn) return;

            int old = CurrentTurn;
            CurrentTurn = newTurn;
            Debug.Log($"[PhaseService] Turn changed: {old} -> {CurrentTurn}");
            OnTurnChanged?.Invoke(CurrentTurn);

            // manage sleepy state: only first turn cards are sleepy
            if (CurrentTurn == 1)
            {
                SleepManager.SleepAll();
            }
            else if (old <= 1 && CurrentTurn > 1)
            {
                SleepManager.WakeAll();
            }
        }

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
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnEnable()
        {
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnGameStarted -= HandleGameStarted;
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            Debug.Log($"[PhaseService] Game started with phase: {result.CurrentPhase} turn={result.TurnNumber}");
            ResetPhase();
            CanAct = result.CanAct;
            UpdateTurn(result.TurnNumber);
            ApplyServerPhase(result.CurrentPhase, true);
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            Debug.Log($"[PhaseService] Phase changed to: {result.CurrentPhase} turn={result.TurnNumber}");
            CanAct = result.CanAct;
            UpdateTurn(result.TurnNumber);
            ApplyServerPhase(result.CurrentPhase);
        }
        public void ApplyServerPhase(GamePhase newPhase, bool forceNotify = false)
        {
            if (!forceNotify && CurrentPhase == newPhase)
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
                case GamePhase.STAND_BY:
                    OnEnterStandBy?.Invoke();
                    Debug.Log("[PhaseService] Invoked OnEnterStandBy");
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
            CurrentPhase = GamePhase.STAND_BY;
            CanAct = false;
            _previousPhase = GamePhase.STAND_BY;
            CurrentTurn = 0;
            // also clear any sleeping state left over
            SleepManager.WakeAll();
            Debug.Log("[PhaseService] Phase reset to STAND_BY");
        }
    }
}
