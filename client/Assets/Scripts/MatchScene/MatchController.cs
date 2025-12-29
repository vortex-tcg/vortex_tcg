using System;
using System.Collections;
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

        private readonly List<DrawResultForPlayerDto> _bufferedDraws = new List<DrawResultForPlayerDto>();
        private readonly List<DrawResultForOpponentDto> _bufferedOpponentDraws = new List<DrawResultForOpponentDto>();

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
            client.OnPlayCardResult += HandlePlayCardResult;
            client.OnOpponentPlayCardResult += HandleOpponentPlayCardResult;
            client.OnStatus += HandleStatus;
            client.OnAttackEngage += HandleAttackEngage;
            client.OnOpponentAttackEngage += HandleOpponentAttackEngage;

            client.OnDefenseEngage += HandleDefenseEngage;
            client.OnOpponentDefenseEngage += HandleOpponentDefenseEngage;


            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnRequestChangePhase += HandleRequestChangePhase;

            StartCoroutine(BindPhaseManagerWhenReady());
        }

        private IEnumerator BindPhaseManagerWhenReady()
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

                client.OnAttackEngage -= HandleAttackEngage;
                client.OnDefenseEngage -= HandleDefenseEngage;

                client.OnPlayCardResult -= HandlePlayCardResult;
                client.OnOpponentPlayCardResult -= HandleOpponentPlayCardResult;
                client.OnStatus -= HandleStatus;
            }

            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnRequestChangePhase -= HandleRequestChangePhase;
        }

        private void HandleStatus(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            if (handManager == null || !handManager.HasPendingPlay) return;

            if (msg.Contains("Can't play", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("play the card", StringComparison.OrdinalIgnoreCase))
            {
                handManager.CancelPendingPlay("hub error: " + msg);
            }
        }

        private void HandleGameStarted(PhaseChangeResultDTO r)
        {
            Debug.Log($"[MatchController] GameStarted phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct}");
            _gameStarted = true;

            handManager?.SetHand(new List<DrawnCardDto>());
            graveyardManager?.ResetGraveyard();
            opponentHandManager?.ResetHand();

            PhaseManager.Instance?.ApplyServerPhase(r.CurrentPhase);
            OpponentBoardManager.Instance?.ResetBoard();

            foreach (DrawResultForPlayerDto d in _bufferedDraws) ApplyDraw(d);
            _bufferedDraws.Clear();

            foreach (DrawResultForOpponentDto od in _bufferedOpponentDraws) ApplyOpponentDraw(od);
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

        private void HandlePlayCardResult(PlayCardPlayerResultDto r)
        {
            if (r == null) return;

            Debug.Log($"[MatchController] PlayCardResult canPlayed={r.canPlayed} loc={r.location} gameCardId={r.PlayedCard?.GameCardId}");

            if (r.PlayedCard == null) return;
            handManager?.ConfirmPlayFromServer(r.PlayedCard.GameCardId, r.location, r.canPlayed);
        }

        private void HandleOpponentPlayCardResult(PlayCardOpponentResultDto r)
        {
            if (r == null) return;

            Debug.Log($"[MatchController] OpponentPlayCardResult loc={r.location} gameCardId={r.PlayedCard?.GameCardId}");

            opponentHandManager?.RemoveOneCardFromHand();

            if (OpponentBoardManager.Instance != null)
                OpponentBoardManager.Instance.PlaceOpponentCard(r.location, r.PlayedCard);
        }

        private void HandleAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchController] HandleAttackEngage ids={string.Join(",", attackIds)}");
            AttackManager.Instance?.ApplyAttackStateFromServer(attackIds);
        }

        private void HandleOpponentAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchController] HandleOpponentAttackEngage ids={string.Join(",", attackIds)}");
            OpponentBoardManager.Instance?.ApplyOpponentAttackState(attackIds);
        }

        private void HandleDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchController] HandleDefenseEngage defenses={(data?.DefenseCards?.Count ?? 0)}");
            DefenseManager.Instance?.ApplyDefenseStateFromServer(data);
        }

        private void HandleOpponentDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchController] HandleOpponentDefenseEngage defenses={(data?.DefenseCards?.Count ?? 0)}");
            OpponentBoardManager.Instance?.ApplyOpponentDefenseState(data);
        }

    }
}
