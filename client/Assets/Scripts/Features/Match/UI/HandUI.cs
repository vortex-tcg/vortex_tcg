using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.UI
{
    /// <summary>
    /// Gestionnaire UI de la main du joueur
    /// Remplace HandManager en utilisant MatchEvents
    /// </summary>
    public class HandUI : MonoBehaviour
    {
        public static HandUI Instance { get; private set; }
        public CardUI CardPrefab => _cardPrefab;
        public Transform HandRoot => _handRoot;

        [Header("Hand Spawn")]
        [SerializeField] private CardUI _cardPrefab;
        [SerializeField] private Transform _handRoot;
        [SerializeField] private float _cardSpacing = 1.2f;
        private const int MaxHandSize = 7;

        [HideInInspector] public CardUI SelectedCard;

        private readonly List<CardUI> _handCards = new();
        private bool _playRequestInFlight;
        private CardUI _pendingCard;
        private CardSlotUI _pendingSlot;
        private CancellationTokenSource _pendingTimeoutCts;

        public bool HasPendingPlay => _playRequestInFlight;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnPlayerCardsDrawn += HandleCardsDrawn;
            MatchEvents.OnCardSelected += HandleCardSelected;
            MatchEvents.OnCardDeselected += HandleCardDeselected;
            MatchEvents.OnCardPlayRequested += HandleCardPlayRequested;
            MatchEvents.OnServerStatusMessage += HandleServerStatus;
            MatchEvents.OnPlayerCardPlayed += HandleCardPlayResult;
            MatchEvents.OnPendingPlayCancelled += HandlePendingPlayCancelled;
        }

        private void OnDisable()
        {
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnPlayerCardsDrawn -= HandleCardsDrawn;
            MatchEvents.OnCardSelected -= HandleCardSelected;
            MatchEvents.OnCardDeselected -= HandleCardDeselected;
            MatchEvents.OnCardPlayRequested -= HandleCardPlayRequested;
            MatchEvents.OnServerStatusMessage -= HandleServerStatus;
            MatchEvents.OnPlayerCardPlayed -= HandleCardPlayResult;
            MatchEvents.OnPendingPlayCancelled -= HandlePendingPlayCancelled;
        }

        // ========== EVENT HANDLERS ==========

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            ClearHand();
        }

        private void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            if (result?.DrawnCards != null && result.DrawnCards.Count > 0)
                AddCards(result.DrawnCards);
        }

        private void HandleCardSelected(CardUI card)
        {
            if (card == null) return;
            if (SelectedCard != null)
                SelectedCard.SetSelected(false);

            SelectedCard = card;
            SelectedCard.SetSelected(true);
        }

        private void HandleCardDeselected()
        {
            if (SelectedCard != null)
            {
                SelectedCard.SetSelected(false);
                SelectedCard = null;
            }
        }

        private void HandleCardPlayRequested(CardUI card, CardSlotUI slot)
        {
            RequestPlayCard(card, slot);
        }

        private void HandleServerStatus(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (!HasPendingPlay) return;

            if (message.Contains("Can't play", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("play the card", StringComparison.OrdinalIgnoreCase))
            {
                CancelPendingPlay("Hub error: " + message);
            }
        }

        private void HandleCardPlayResult(PlayCardPlayerResultDto result)
        {
            if (result == null) return;
            ConfirmPlayFromServer(result.PlayedCard?.GameCardId ?? -1, result.location, result.canPlayed);
        }

        private void HandlePendingPlayCancelled(string reason)
        {
            CancelPendingPlay(reason);
        }

        // ========== PUBLIC METHODS ==========

        public void SetHand(List<DrawnCardDto> drawnCards)
        {
            ClearHand();
            AddCards(drawnCards);
        }

        public void AddCards(List<DrawnCardDto> drawnCards)
        {
            if (drawnCards == null || drawnCards.Count == 0) return;

            if (_cardPrefab == null || _handRoot == null)
            {
                Debug.LogError("[HandUIManager] cardPrefab ou handRoot non assigné.");
                return;
            }

            foreach (DrawnCardDto dto in drawnCards)
            {
                if (_handCards.Count >= MaxHandSize) break;

                CardUI card = Instantiate(_cardPrefab, _handRoot);
                card.ApplyDTO(
                    dto.GameCardId.ToString(),
                    dto.Name,
                    dto.Hp,
                    dto.Attack,
                    dto.Cost,
                    dto.Description,
                    ""
                );

                EnsureCollider(card);
                _handCards.Add(card);
            }

            LayoutHand();
        }

        public void SelectCard(CardUI card)
        {
            MatchEvents.FireCardSelected(card);
        }

        public void DeselectCurrentCard()
        {
            MatchEvents.FireCardDeselected();
        }

        public async Task RequestPlaySelectedCard(CardSlotUI slot)
        {
            if (SelectedCard != null)
                await RequestPlayCard(SelectedCard, slot);
        }

        public void CancelPendingPlay(string reason)
        {
            if (!_playRequestInFlight) return;

            _playRequestInFlight = false;
            _pendingCard = null;
            _pendingSlot = null;

            CancelPendingTimeout();

            if (SelectedCard != null)
                SelectedCard.SetSelected(false);

            Debug.Log($"[HandUIManager] PendingPlay cancelled: {reason}");
        }

        // ========== PRIVATE METHODS ==========

        private void ClearHand()
        {
            foreach (CardUI card in _handCards)
            {
                Destroy(card.gameObject);
            }
            _handCards.Clear();
            SelectedCard = null;
        }

        private void LayoutHand()
        {
            if (_handRoot == null) return;

            int count = _handCards.Count;
            float totalWidth = (count - 1) * _cardSpacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                RectTransform rect = _handCards[i].GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.localPosition = new Vector3(startX + i * _cardSpacing, 0, 0);
                }
            }
        }

        private void EnsureCollider(CardUI card)
        {
            if (card.GetComponent<Collider>() != null) return;

            BoxCollider bc = card.gameObject.AddComponent<BoxCollider>();
            bc.size = Vector3.one;
        }

        private async Task RequestPlayCard(CardUI card, CardSlotUI slot)
        {
            if (_playRequestInFlight)
            {
                Debug.Log("[HandUIManager] RequestPlayCard STOP: already in flight");
                return;
            }

            if (card == null || slot == null) return;

            SignalRClient client = SignalRClient.Instance;
            if (client == null || !client.IsConnected)
            {
                Debug.LogWarning("[HandUIManager] SignalRClient pas connecté.");
                return;
            }

            if (!int.TryParse(card.cardId, out int gameCardId))
            {
                Debug.LogError("[HandUIManager] card.cardId pas un int: " + card.cardId);
                return;
            }

            _playRequestInFlight = true;
            _pendingCard = card;
            _pendingSlot = slot;

            Debug.Log($"[HandUIManager] RequestPlayCard -> PlayCard(gameCardId={gameCardId}, loc={slot.slotIndex})");
            StartPendingTimeout(2500);

            try
            {
                await client.PlayCard(gameCardId, slot.slotIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError("[HandUIManager] PlayCard invoke failed: " + ex);
                CancelPendingPlay("Invoke exception");
            }
        }

        private void ConfirmPlayFromServer(int gameCardId, int location, bool canPlayed)
        {
            if (!_playRequestInFlight)
            {
                Debug.LogWarning($"[HandUIManager] ConfirmPlayFromServer called but no pending play!");
                return;
            }

            CancelPendingTimeout();

            if (canPlayed)
            {
                if (_pendingCard != null && int.TryParse(_pendingCard.cardId, out int id) && id == gameCardId)
                {
                    Destroy(_pendingCard.gameObject);
                    _handCards.Remove(_pendingCard);
                    LayoutHand();
                }
            }
            else
            {
                if (_pendingCard != null)
                    _pendingCard.SetSelected(false);
            }

            _playRequestInFlight = false;
            _pendingCard = null;
            _pendingSlot = null;
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
                    Debug.LogWarning("[HandUIManager] Pending play timeout!");
                    CancelPendingPlay("Timeout");
                }
            }
            catch (OperationCanceledException)
            {
                // Annulation normale
            }
        }
    }
}
