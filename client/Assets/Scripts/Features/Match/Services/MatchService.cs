using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Service principal orchestrant le match
    /// Remplace MatchController en utilisant MatchEvents
    /// Pont entre SignalR et les services/UI via événements
    /// </summary>
    public class MatchService : MonoBehaviour
    {
        public static MatchService Instance { get; private set; }

        private SignalRClient _client;
        private bool _gameStarted;

        private readonly List<DrawResultForPlayerDto> _bufferedDraws = new();
        private readonly List<DrawResultForOpponentDto> _bufferedOpponentDraws = new();

        private void Awake()
        {
            Debug.Log("[MatchService] Awake");
            if (Instance != null && Instance != this)
            {
                Debug.Log("[MatchService] Instance déjà existante, destruction");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("[MatchService] ✅ Instance définie");
        }

        private void OnEnable()
        {
            Debug.Log("[MatchService] OnEnable appelé");
            _client = SignalRClient.Instance;
            if (_client == null)
            {
                Debug.LogError("[MatchService] SignalRClient.Instance NULL");
                return;
            }

            Debug.Log("[MatchService] S'abonner aux événements SignalR");
            // S'abonner aux événements SignalR
            _client.OnGameStarted += HandleGameStarted;
            _client.OnPhaseChanged += HandlePhaseChanged;
            _client.OnCardsDrawn += HandleCardsDrawn;
            _client.OnOpponentCardsDrawn += HandleOpponentCardsDrawn;
            _client.OnPlayCardResult += HandlePlayCardResult;
            _client.OnOpponentPlayCardResult += HandleOpponentPlayCardResult;
            _client.OnStatus += HandleStatus;
            _client.OnAttackEngage += HandleAttackEngage;
            _client.OnOpponentAttackEngage += HandleOpponentAttackEngage;
            _client.OnBattleResolution += HandleBattleResolution;
            _client.OnDefenseEngage += HandleDefenseEngage;
            _client.OnOpponentDefenseEngage += HandleOpponentDefenseEngage;

            Debug.Log("[MatchService] ✅ Tous les événements enregistrés");

            // S'abonner aux événements locaux
            if (PhaseService.Instance != null)
                PhaseService.Instance.OnRequestChangePhase += HandleRequestChangePhase;

            StartCoroutine(BindPhaseServiceWhenReady());
        }

        private IEnumerator BindPhaseServiceWhenReady()
        {
            while (PhaseService.Instance == null)
                yield return null;

            Debug.Log("[MatchService] Bind OnRequestChangePhase");
            PhaseService.Instance.OnRequestChangePhase -= HandleRequestChangePhase;
            PhaseService.Instance.OnRequestChangePhase += HandleRequestChangePhase;
        }

        private void OnDisable()
        {
            if (_client != null)
            {
                _client.OnGameStarted -= HandleGameStarted;
                _client.OnPhaseChanged -= HandlePhaseChanged;
                _client.OnCardsDrawn -= HandleCardsDrawn;
                _client.OnOpponentCardsDrawn -= HandleOpponentCardsDrawn;
                _client.OnBattleResolution -= HandleBattleResolution;
                _client.OnAttackEngage -= HandleAttackEngage;
                _client.OnDefenseEngage -= HandleDefenseEngage;
                _client.OnOpponentAttackEngage -= HandleOpponentAttackEngage;
                _client.OnOpponentDefenseEngage -= HandleOpponentDefenseEngage;
                _client.OnPlayCardResult -= HandlePlayCardResult;
                _client.OnOpponentPlayCardResult -= HandleOpponentPlayCardResult;
                _client.OnStatus -= HandleStatus;
            }

            if (PhaseService.Instance != null)
                PhaseService.Instance.OnRequestChangePhase -= HandleRequestChangePhase;
        }

        // ========== HANDLERS SIGNALR → MATCHEVENTS ==========

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            Debug.Log($"[MatchService] HandleGameStarted APPELÉ phase={result?.CurrentPhase}, turn={result?.TurnNumber}");
            Debug.Log($"[MatchService] GameStarted phase={result.CurrentPhase} turn={result.TurnNumber}");
            _gameStarted = true;

            // Émettre l'événement pour que les UI/Services réagissent
            Debug.Log("[MatchService] 🔥 Déclenchement FireGameStarted");
            MatchEvents.FireGameStarted(result);
            Debug.Log("[MatchService] ✅ FireGameStarted fait");

            // Traiter les draws mis en buffer
            Debug.Log($"[MatchService] Traitement de {_bufferedDraws.Count} draws bufferisés");
            foreach (DrawResultForPlayerDto d in _bufferedDraws)
            {
                Debug.Log($"[MatchService] 🔥 Déclenchement FirePlayerCardsDrawn pour {d.DrawnCards?.Count ?? 0} cartes");
                MatchEvents.FirePlayerCardsDrawn(d);
            }
            _bufferedDraws.Clear();
            Debug.Log("[MatchService] ✅ Tous les draws traités");

            foreach (DrawResultForOpponentDto od in _bufferedOpponentDraws)
            {
                MatchEvents.FireOpponentCardsDrawn(od);
            }
            _bufferedOpponentDraws.Clear();
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO result)
        {
            Debug.Log($"[MatchService] PhaseChanged phase={result.CurrentPhase} turn={result.TurnNumber}");
            MatchEvents.FirePhaseChanged(result);

            if (result.AutoChanged && !string.IsNullOrWhiteSpace(result.AutoChangeReason))
                Debug.Log($"[MatchService] AutoChangeReason: {result.AutoChangeReason}");
        }

        private void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            Debug.Log($"[MatchService] HandleCardsDrawn appelé - drawnCards={result?.DrawnCards?.Count ?? 0}, gameStarted={_gameStarted}");
            
            if (!_gameStarted)
            {
                _bufferedDraws.Add(result);
                Debug.Log("[MatchService] Buffering draw (game not started yet)");
                return;
            }

            Debug.Log($"[MatchService] PlayerCardsDrawn drawn={result.DrawnCards?.Count ?? 0} graveyard={result.SentToGraveyard?.Count ?? 0}");
            MatchEvents.FirePlayerCardsDrawn(result);
        }

        private void HandleOpponentCardsDrawn(DrawResultForOpponentDto result)
        {
            if (!_gameStarted)
            {
                _bufferedOpponentDraws.Add(result);
                Debug.Log("[MatchService] Buffering opponent draw (game not started yet)");
                return;
            }

            Debug.Log($"[MatchService] OpponentCardsDrawn count={result.CardsDrawnCount} burned={result.CardsBurnedCount}");
            MatchEvents.FireOpponentCardsDrawn(result);
        }

        private void HandlePlayCardResult(PlayCardPlayerResultDto result)
        {
            Debug.Log($"[MatchService] PlayerCardPlayed canPlayed={result.canPlayed} location={result.location}");
            MatchEvents.FirePlayerCardPlayed(result);
        }

        private void HandleOpponentPlayCardResult(PlayCardOpponentResultDto result)
        {
            Debug.Log($"[MatchService] OpponentCardPlayed location={result.location} cardId={result.PlayedCard?.GameCardId}");
            MatchEvents.FireOpponentCardPlayed(result);
        }

        private void HandleAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchService] PlayerAttackEngaged ids={string.Join(",", attackIds)}");
            
            AttackResponseDto dto = new AttackResponseDto { AttackCardsId = attackIds };
            MatchEvents.FirePlayerAttackEngaged(dto);
        }

        private void HandleOpponentAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchService] OpponentAttackEngaged ids={string.Join(",", attackIds)}");
            
            AttackResponseDto dto = new AttackResponseDto { AttackCardsId = attackIds };
            MatchEvents.FireOpponentAttackEngaged(dto);
        }

        private void HandleDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchService] PlayerDefenseEngaged defenses={data?.DefenseCards?.Count ?? 0}");
            MatchEvents.FirePlayerDefenseEngaged(data);
        }

        private void HandleOpponentDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchService] OpponentDefenseEngaged defenses={data?.DefenseCards?.Count ?? 0}");
            MatchEvents.FireOpponentDefenseEngaged(data);
        }

        private void HandleBattleResolution(BattlesDataDto data, bool localIsAttacker)
        {
            if (!_gameStarted)
            {
                Debug.LogWarning("[MatchService] BattleResolution ignored: game not started");
                return;
            }

            Debug.Log($"[MatchService] BattleResolution battles={data?.battles?.Count ?? 0} localIsAttacker={localIsAttacker}");
            MatchEvents.FireBattleResolution(data, localIsAttacker);
        }

        private void HandleStatus(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Debug.Log($"[MatchService] ServerStatus: {message}");
                MatchEvents.FireServerStatusMessage(message);
            }
        }

        private async void HandleRequestChangePhase(GamePhase newPhase)
        {
            Debug.Log($"[MatchService] RequestChangePhase({newPhase}) -> calling hub");
            try
            {
                if (_client != null && _client.IsConnected)
                {
                    await _client.ChangePhase();
                    Debug.Log("[MatchService] ChangePhase hub call succeeded");
                }
                else
                {
                    Debug.LogWarning("[MatchService] Cannot change phase: client not connected");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MatchService] ChangePhase failed: {ex}");
            }
        }
    }
}
