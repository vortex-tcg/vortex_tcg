using System;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private HandManager handManager;
        [SerializeField] private GraveyardManager graveyardManager;
        [SerializeField] private OpponentHandManager opponentHandManager;

        private SignalRClient client;

        private bool _gameStarted;

        private readonly List<DrawResultForPlayerDto> _bufferedDraws = new();
        private readonly List<DrawResultForOpponentDto> _bufferedOpponentDraws = new();

        private void OnEnable()
        {
            client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[MatchController] SignalRClient.Instance NULL");
                return;
            }

            if (handManager == null) handManager = HandManager.Instance;
            if (graveyardManager == null) graveyardManager = GraveyardManager.Instance;
            if (opponentHandManager == null) opponentHandManager = OpponentHandManager.Instance;

            client.OnGameStarted += HandleGameStarted;
            client.OnPhaseChanged += HandlePhaseChanged;
            client.OnCardsDrawn += HandleCardsDrawn;
            client.OnOpponentCardsDrawn += HandleOpponentCardsDrawn;

            StartCoroutine(BindPhaseManagerWhenReady());
        }

        private System.Collections.IEnumerator BindPhaseManagerWhenReady()
        {
            while (PhaseManager.Instance == null)
                yield return null;

            Debug.Log("[MatchController] Bind OnRequestChangePhase");
            PhaseManager.Instance.OnRequestChangePhase -= HandleRequestChangePhase;
            PhaseManager.Instance.OnRequestChangePhase += HandleRequestChangePhase;
        }

        private void OnDisable()
        {
            if (client != null)
            {
                client.OnGameStarted -= HandleGameStarted;
                client.OnPhaseChanged -= HandlePhaseChanged;
                client.OnCardsDrawn -= HandleCardsDrawn;
                client.OnOpponentCardsDrawn -= HandleOpponentCardsDrawn;
            }

            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnRequestChangePhase -= HandleRequestChangePhase;
        }

        private void HandleGameStarted(PhaseChangeResultDTO r)
        {
            Debug.Log($"[MatchController] GameStarted phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct}");

            _gameStarted = true;

            // reset UI local, puis on applique ce que le serveur envoie
            handManager?.SetHand(new List<DrawnCardDto>());
            graveyardManager?.ResetGraveyard();
            opponentHandManager?.ResetHand();

            PhaseManager.Instance?.ApplyServerPhase(r.CurrentPhase);

            // rejouer les pioches arrivées trop tôt
            foreach (var d in _bufferedDraws) ApplyDraw(d);
            _bufferedDraws.Clear();

            foreach (var d in _bufferedOpponentDraws) ApplyOpponentDraw(d);
            _bufferedOpponentDraws.Clear();
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO r)
        {
            Debug.Log($"[MatchController] PhaseChanged phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct} auto={r.AutoChanged}");

            PhaseManager.Instance?.ApplyServerPhase(r.CurrentPhase);

            if (r.AutoChanged && !string.IsNullOrWhiteSpace(r.AutoChangeReason))
                Debug.Log("[MatchController] AutoChangeReason: " + r.AutoChangeReason);
        }

        private async void HandleRequestChangePhase()
        {
            Debug.Log("[MatchController] HandleRequestChangePhase() -> calling hub ChangePhase");
            try
            {
                if (client != null && client.IsConnected)
                    await client.ChangePhase();
                else
                    Debug.LogWarning("[MatchController] client not connected -> ChangePhase ignored");
            }
            catch (Exception ex)
            {
                Debug.LogError("[MatchController] ChangePhase failed: " + ex);
            }
        }

        private void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            if (!_gameStarted)
            {
                _bufferedDraws.Add(result);
                return;
            }

            ApplyDraw(result);
        }

        private void ApplyDraw(DrawResultForPlayerDto result)
        {
            if (result == null) return;

            if (result.SentToGraveyard != null && result.SentToGraveyard.Count > 0)
                graveyardManager?.AddCards(result.SentToGraveyard);

            if (result.DrawnCards != null && result.DrawnCards.Count > 0)
                handManager?.AddCards(result.DrawnCards);
        }

        private void HandleOpponentCardsDrawn(DrawResultForOpponentDto result)
        {
            if (!_gameStarted)
            {
                _bufferedOpponentDraws.Add(result);
                return;
            }

            ApplyOpponentDraw(result);
        }

        private void ApplyOpponentDraw(DrawResultForOpponentDto result)
        {
            int added = result?.CardsDrawnCount ?? 0;
            int burned = result?.CardsBurnedCount ?? 0;
            int fatigue = result?.FatigueCount ?? 0;

            Debug.Log($"[MatchController] Opponent drew +{added} (burn {burned}, fatigue {fatigue})");

            if (added > 0)
                opponentHandManager?.AddFaceDownCards(added);
        }
    }
}
