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
        public List<CardUI> HandCards => _handCards;
        protected List<Transform> HandSlots => _handSlots;
        protected const int MaxHandSize = 7;

        [Header("Hand Spawn")]
        [SerializeField] private CardUI _cardPrefab;
        [SerializeField] private Transform _handRoot;
        [SerializeField] protected List<Transform> _handSlots = new();

        [HideInInspector] public CardUI SelectedCard;

        protected readonly List<CardUI> _handCards = new();
        private bool _playRequestInFlight;
        private CardUI _pendingCard;
        private CardSlotUI _pendingSlot;
        private CancellationTokenSource _pendingTimeoutCts;

        public bool HasPendingPlay => _playRequestInFlight;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                return;
            }
            Instance = this;
            Debug.Log("[HandUI] Awake - FindAndAssignHandSlots");
            FindAndAssignHandSlots();
            Debug.Log($"[HandUI] Awake - Slots trouvés: {_handSlots.Count}, CardPrefab: {(_cardPrefab != null ? _cardPrefab.name : "NULL")}");
        }

        private void OnEnable()
        {
            Debug.Log("[HandUI] OnEnable appelé");
            OnEnableEvents();
        }

        protected virtual void OnEnableEvents()
        {
            Debug.Log("[HandUI] OnEnableEvents - Inscription aux événements");
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnPlayerCardsDrawn += HandleCardsDrawn;
            MatchEvents.OnCardSelected += HandleCardSelected;
            MatchEvents.OnCardDeselected += HandleCardDeselected;
            MatchEvents.OnCardPlayRequested += HandleCardPlayRequested;
            MatchEvents.OnServerStatusMessage += HandleServerStatus;
            MatchEvents.OnPlayerCardPlayed += HandleCardPlayResult;
            MatchEvents.OnPendingPlayCancelled += HandlePendingPlayCancelled;
            Debug.Log("[HandUI] ✅ Tous les événements enregistrés");

            // Fallback: s'enregistrer directement auprès de SignalRClient
            SignalRClient client = SignalRClient.Instance;
            if (client != null)
            {
                Debug.Log("[HandUI] SignalRClient trouvé, s'enregistrer directement pour CardsDrawn");
                client.OnCardsDrawn += HandleSignalRCardsDrawn;
            }
        }

        private void OnDisable()
        {
            OnDisableEvents();
        }

        protected virtual void OnDisableEvents()
        {
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnPlayerCardsDrawn -= HandleCardsDrawn;
            MatchEvents.OnCardSelected -= HandleCardSelected;
            MatchEvents.OnCardDeselected -= HandleCardDeselected;
            MatchEvents.OnCardPlayRequested -= HandleCardPlayRequested;
            MatchEvents.OnServerStatusMessage -= HandleServerStatus;
            MatchEvents.OnPlayerCardPlayed -= HandleCardPlayResult;
            MatchEvents.OnPendingPlayCancelled -= HandlePendingPlayCancelled;

            // Unsubscribe from SignalRClient fallback
            SignalRClient client = SignalRClient.Instance;
            if (client != null)
            {
                client.OnCardsDrawn -= HandleSignalRCardsDrawn;
            }
        }

        /// <summary>
        /// Cherche automatiquement les slots enfants de la main
        /// Si des slots sont déjà assignés, ne fait rien
        /// </summary>
        protected virtual void FindAndAssignHandSlots()
        {
            // Si les slots sont déjà assignés manuellement dans l'inspecteur, ne pas chercher
            if (_handSlots.Count > 0)
            {
                Debug.Log($"[HandUI] ✅ {_handSlots.Count} slots assignés manuellement");
                return;
            }
            
            // Sinon, cherche automatiquement P1HandSlot1 à P1HandSlot7
            _handSlots.Clear();
            Transform parent = transform;
            for (int i = 1; i <= 7; i++)
            {
                Transform slot = parent.Find($"P1HandSlot{i}");
                if (slot != null)
                {
                    _handSlots.Add(slot);
                    Debug.Log($"[HandUI] ✅ Slot P1HandSlot{i} trouvé automatiquement");
                }
            }
            
            if (_handSlots.Count > 0)
            {
                Debug.Log($"[HandUI] ✅ {_handSlots.Count} slots trouvés automatiquement");
            }
            else
            {
                Debug.LogWarning("[HandUI] ⚠️ Aucun slot trouvé! Assignez les manuellement ou créez P1HandSlot1-7");
            }
        }

        protected void HandleGameStarted(PhaseChangeResultDTO result)
        {
            Debug.Log("[HandUI] HandleGameStarted appelé - Clearing hand");
            ClearHand();
        }

        protected void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            Debug.Log($"[HandUI] HandleCardsDrawn appelé - Cards: {result?.DrawnCards?.Count ?? 0}");
            if (result?.DrawnCards != null && result.DrawnCards.Count > 0)
            {
                Debug.Log($"[HandUI] Appel de AddCards avec {result.DrawnCards.Count} cartes");
                AddCards(result.DrawnCards);
            }
            else
            {
                Debug.LogWarning("[HandUI] HandleCardsDrawn: Aucune carte dans result");
            }
        }

        /// <summary>
        /// Fallback direct de SignalRClient si MatchService n'existe pas
        /// </summary>
        protected void HandleSignalRCardsDrawn(DrawResultForPlayerDto result)
        {
            Debug.Log("[HandUI] 🔥 HandleSignalRCardsDrawn (fallback direct) - Cartes reçues");
            Debug.Log($"[HandUI] Result: {(result != null ? "NOT NULL" : "NULL")}");
            Debug.Log($"[HandUI] DrawnCards: {(result?.DrawnCards != null ? result.DrawnCards.Count : "NULL")}");
            HandleCardsDrawn(result);
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

        protected void HandleServerStatus(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (!HasPendingPlay) return;

            if (message.Contains("Can't play", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("play the card", StringComparison.OrdinalIgnoreCase))
            {
                CancelPendingPlay("Hub error: " + message);
            }
        }

        protected void HandleCardPlayResult(PlayCardPlayerResultDto result)
        {
            if (result == null) return;
            ConfirmPlayFromServer(result.PlayedCard?.GameCardId ?? -1, result.location, result.canPlayed);
        }

        protected void HandlePendingPlayCancelled(string reason)
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

            AddCardsInternal(drawnCards);
        }

        /// <summary>
        /// Méthode virtuelle pour ajouter les cartes
        /// OpponentHandUI peut surcharger pour un comportement différent
        /// </summary>
        protected virtual void AddCardsInternal(List<DrawnCardDto> drawnCards)
        {
            Debug.Log($"[HandUI] AddCardsInternal appelé - {drawnCards?.Count ?? 0} cartes à ajouter");
            
            if (_cardPrefab == null)
            {
                Debug.LogError("[HandUI] ❌ cardPrefab non assigné.");
                return;
            }

            Debug.Log($"[HandUI] État: Slots={_handSlots.Count}, HandRoot={(_handRoot != null ? _handRoot.name : "NULL")}, CardPrefab={_cardPrefab.name}");

            // Utiliser les slots pré-définis si disponibles
            if (_handSlots != null && _handSlots.Count > 0)
            {
                Debug.Log($"[HandUI] ✅ Utilisation de {_handSlots.Count} slots pour les cartes");
                int cardIndex = 0;
                foreach (DrawnCardDto dto in drawnCards)
                {
                    if (cardIndex >= _handSlots.Count || cardIndex >= MaxHandSize)
                    {
                        Debug.LogWarning($"[HandUI] Plus de slots disponibles. CardIndex: {cardIndex}, SlotCount: {_handSlots.Count}");
                        break;
                    }

                    Transform slot = _handSlots[cardIndex];
                    if (slot == null)
                    {
                        Debug.LogError($"[HandUI] ❌ Slot {cardIndex} est NULL!");
                        cardIndex++;
                        continue;
                    }

                    Debug.Log($"[HandUI] Création carte {cardIndex + 1}: '{dto.Name}' dans slot '{slot.name}'");
                    
                    CardUI card = Instantiate(_cardPrefab, slot);
                    card.gameObject.name = $"Card_{dto.Name}_{cardIndex}";
                    
                    Debug.Log($"[HandUI] Carte instantiée '{card.gameObject.name}', parent: {card.transform.parent.name}");
                    
                    card.ApplyDTO(
                        dto.GameCardId.ToString(),
                        dto.Name,
                        dto.Hp,
                        dto.Attack,
                        dto.Cost,
                        dto.Description,
                        ""
                    );

                    // Positionner la carte sur le slot
                    card.transform.localPosition = Vector3.zero;
                    card.transform.localRotation = Quaternion.identity;
                    card.transform.localScale = Vector3.one;
                    
                    Debug.Log($"[HandUI] ✅ Carte {cardIndex}: pos={card.transform.localPosition}, active={card.gameObject.activeSelf}");

                    EnsureCollider(card);
                    _handCards.Add(card);
                    cardIndex++;
                }
                Debug.Log($"[HandUI] ✅ {cardIndex} cartes ajoutées avec succès");
            }
            else if (_handRoot != null)
            {
                // Fallback: utiliser _handRoot si aucun slot n'est configuré
                Debug.LogWarning("[HandUI] ⚠️ Aucun slot configuré, fallback sur _handRoot");
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
            }
            else
            {
                Debug.LogError("[HandUI] ❌ Ni slots ni handRoot assignés!");
            }
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

        protected void EnsureCollider(CardUI card)
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
