using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Service de gestion de la main du joueur - Logique métier pure
    /// Gère l'état des cartes, validation, et communication serveur
    /// </summary>
    public class HandService
    {
        public const int MaxHandSize = 7;

        private bool _playRequestInFlight;
        private CardUI _pendingCard;
        private CardSlotUI _pendingSlot;
        private CancellationTokenSource _pendingTimeoutCts;
        
        public bool HasPendingPlay => _playRequestInFlight;
        public CardUI PendingCard => _pendingCard;
        public CardSlotUI PendingSlot => _pendingSlot;

        // Events pour notifier l'UI
        public event Action<string> OnPlayCancelled;
        public event Action<int, int, bool> OnPlayConfirmed;

        public async Task<bool> RequestPlayCard(CardUI card, CardSlotUI slot, SignalRClient client)
        {
            Debug.Log($"[HandService] RequestPlayCard called - card={card?.name}, slot={slot?.slotIndex}, client={(client != null ? "EXISTS" : "NULL")}");
            
            if (_playRequestInFlight)
            {
                Debug.Log("[HandService] RequestPlayCard STOP: already in flight");
                return false;
            }

            if (card == null || slot == null)
            {
                Debug.LogWarning("[HandService] Card ou slot NULL");
                return false;
            }

            if (client == null || !client.IsConnected)
            {
                Debug.LogWarning("[HandService] SignalRClient pas connecté");
                OnPlayCancelled?.Invoke("Client not connected");
                return false;
            }

            // Validate phase
            if (PhaseService.Instance != null)
            {
                GamePhase currentPhase = PhaseService.Instance.CurrentPhase;
                Debug.Log($"[HandService] Current phase: {currentPhase}");
                
                if (currentPhase != GamePhase.PLACEMENT)
                {
                    Debug.LogWarning($"[HandService] Cannot play cards during {currentPhase} phase. Only PLACEMENT phase allows card play.");
                    OnPlayCancelled?.Invoke($"Wrong phase: {currentPhase}");
                    return false;
                }
            }
            else
            {
                Debug.LogWarning("[HandService] PhaseService.Instance is NULL - cannot validate phase");
            }

            if (!int.TryParse(card.cardId, out int gameCardId))
            {
                Debug.LogError($"[HandService] card.cardId pas un int: {card.cardId}");
                OnPlayCancelled?.Invoke("Invalid card ID");
                return false;
            }

            _playRequestInFlight = true;
            _pendingCard = card;
            _pendingSlot = slot;

            Debug.Log($"[HandService] RequestPlayCard -> PlayCard(gameCardId={gameCardId}, loc={slot.slotIndex})");
            StartPendingTimeout(2500);

            try
            {
                await client.PlayCard(gameCardId, slot.slotIndex);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HandService] PlayCard invoke failed: {ex}");
                CancelPendingPlay("Invoke exception");
                return false;
            }
        }

        public void ConfirmPlayFromServer(int gameCardId, int location, bool canPlayed)
        {
            if (!_playRequestInFlight)
            {
                Debug.LogWarning("[HandService] ConfirmPlayFromServer called but no pending play!");
                return;
            }

            CancelPendingTimeout();

            OnPlayConfirmed?.Invoke(gameCardId, location, canPlayed);

            _playRequestInFlight = false;
            _pendingCard = null;
            _pendingSlot = null;
        }

        public void CancelPendingPlay(string reason)
        {
            if (!_playRequestInFlight) return;

            _playRequestInFlight = false;
            _pendingCard = null;
            _pendingSlot = null;

            CancelPendingTimeout();

            Debug.Log($"[HandService] PendingPlay cancelled: {reason}");
            OnPlayCancelled?.Invoke(reason);
        }

        public bool ShouldCancelOnServerError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            if (!HasPendingPlay) return false;

            return message.Contains("Can't play", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("play the card", StringComparison.OrdinalIgnoreCase);
        }

        private void StartPendingTimeout(int delayMs)
        {
            CancelPendingTimeout();
            _pendingTimeoutCts = new CancellationTokenSource();
            _ = PendingTimeoutTask(_pendingTimeoutCts.Token, delayMs);
        }

        private void CancelPendingTimeout()
        {
            _pendingTimeoutCts?.Cancel();
            _pendingTimeoutCts?.Dispose();
            _pendingTimeoutCts = null;
        }

        private async Task PendingTimeoutTask(CancellationToken ct, int delayMs)
        {
            try
            {
                await Task.Delay(delayMs, ct);
                if (!ct.IsCancellationRequested && _playRequestInFlight)
                {
                    Debug.LogWarning("[HandService] Pending play timeout!");
                    CancelPendingPlay("Timeout");
                }
            }
            catch (OperationCanceledException)
            {
                // Annulation normale
            }
        }

        public void Reset()
        {
            CancelPendingPlay("Reset");
        }
    }
}
