using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
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

        public event Action<string> OnPlayCancelled;
        public event Action<int, int, bool> OnPlayConfirmed;

        public async Task<bool> RequestPlayCard(CardUI card, CardSlotUI slot, SignalRClient client)
        {
            if (client == null || !client.IsConnected)
            {
                Debug.LogWarning("[HandService] SignalRClient pas connecté");
                OnPlayCancelled?.Invoke("Client not connected");
                return false;
            }

            if (PhaseService.Instance != null)
            {
                GamePhase currentPhase = PhaseService.Instance.CurrentPhase;
                Debug.Log($"[HandService] Current phase: {currentPhase}");
                
                if (currentPhase != GamePhase.PLACEMENT)
                {
                    Debug.LogWarning($"[HandService] Impossible de placer une carte pendant la phase {currentPhase}.");
                    OnPlayCancelled?.Invoke($"Wrong phase: {currentPhase}");
                    return false;
                }
            }

            if (!int.TryParse(card.cardId, out int gameCardId))
            {
                OnPlayCancelled?.Invoke("Invalid card ID");
                return false;
            }

            _playRequestInFlight = true;
            _pendingCard = card;
            _pendingSlot = slot;

            StartPendingTimeout(2500);

            try
            {
                await client.PlayCard(gameCardId, slot.slotIndex);
                return true;
            }
            catch (Exception)
            {
                CancelPendingPlay("Invoke exception");
                return false;
            }
        }

        public void ConfirmPlayFromServer(int gameCardId, int location, bool canPlayed)
        {

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
                    CancelPendingPlay("Timeout");
                }
            }
            catch (TaskCanceledException){
                // Normal, do nothing
            }
            catch (Exception ex){
                Debug.LogError($"[HandService] PendingTimeoutTask exception: {ex}");
            }
        }

        public void Reset()
        {
            CancelPendingPlay("Reset");
        }
    }
}
