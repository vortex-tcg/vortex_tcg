using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.Features.Match.Services;
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
        public HandService HandService { get; private set; }
        
        public CardUI CardPrefab => _cardPrefab;
        public Transform HandRoot => _handRoot;
        public List<CardUI> HandCards => _handCards;
        protected List<Transform> HandSlots => _handSlots;

        [Header("Hand Spawn")]
        [SerializeField] private CardUI _cardPrefab;
        [SerializeField] private Transform _handRoot;
        [SerializeField] protected List<Transform> _handSlots = new();

        [HideInInspector] public CardUI SelectedCard;

        protected readonly List<CardUI> _handCards = new();
        private readonly List<DrawnCardDto> _pendingDrawnCards = new();
        private bool _gameStarted;

        public bool HasPendingPlay => HandService?.HasPendingPlay ?? false;

        private void Awake()
        {
            Debug.Log($"[HandUI] ✅✅✅ AWAKE CALLED - Instance currently={(Instance != null ? Instance.name : "NULL")}");
            
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[HandUI] ❌ Duplicate HandUI detected! Disabling this={gameObject.name}, keeping Instance={Instance.gameObject.name}");
                enabled = false;
                return;
            }
            
            Instance = this;
            HandService = new HandService();
            _gameStarted = false;
            
            Debug.Log($"[HandUI] ✅ Instance SET to {gameObject.name}");
            Debug.Log("[HandUI] Awake - FindAndAssignHandSlots");
            FindAndAssignHandSlots();
            Debug.Log($"[HandUI] Awake - Slots trouvés: {_handSlots.Count}, CardPrefab: {(_cardPrefab != null ? _cardPrefab.name : "NULL")}");
            
            // Subscribe to service events
            HandService.OnPlayCancelled += HandleServicePlayCancelled;
            HandService.OnPlayConfirmed += HandleServicePlayConfirmed;
        }

        private void OnEnable()
        {
            Debug.Log($"[HandUI] OnEnable appelé - Instance={(Instance != null ? Instance.name : "NULL")}, this={gameObject.name}");
            
            // Safety check: ensure Instance is set (in case Awake order issues)
            if (Instance == null && enabled)
            {
                Debug.LogWarning($"[HandUI] ⚠️ Instance was NULL in OnEnable! Setting it now to {gameObject.name}");
                Instance = this;
                if (HandService == null)
                {
                    HandService = new HandService();
                }
            }
            
            // Always subscribe to service events (in case they were not subscribed in Awake)
            if (HandService != null)
            {
                HandService.OnPlayCancelled += HandleServicePlayCancelled;
                HandService.OnPlayConfirmed += HandleServicePlayConfirmed;
                Debug.Log("[HandUI] ✅ HandService events subscribed in OnEnable");
            }
            
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
            
            // Unsubscribe from service events
            if (HandService != null)
            {
                HandService.OnPlayCancelled -= HandleServicePlayCancelled;
                HandService.OnPlayConfirmed -= HandleServicePlayConfirmed;
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
            Debug.Log("[HandUI] ✅✅✅ HandleGameStarted appelé - Clearing hand and waiting for CardsDrawn");
            _gameStarted = true;
            ClearHand();

            if (_pendingDrawnCards.Count > 0)
            {
                Debug.Log($"[HandUI] ✅ Applying buffered cards: {_pendingDrawnCards.Count}");
                AddCards(_pendingDrawnCards);
                _pendingDrawnCards.Clear();
            }
        }

        protected void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            Debug.Log($"[HandUI] ✅✅✅ HandleCardsDrawn appelé - Cards: {result?.DrawnCards?.Count ?? 0}");
            if (result?.DrawnCards != null && result.DrawnCards.Count > 0)
            {
                if (!_gameStarted)
                {
                    Debug.LogWarning("[HandUI] Game not started yet - buffering drawn cards");
                    _pendingDrawnCards.Clear();
                    _pendingDrawnCards.AddRange(result.DrawnCards);
                    return;
                }

                Debug.Log($"[HandUI] ✅ Appel de AddCards avec {result.DrawnCards.Count} cartes");
                foreach (var dto in result.DrawnCards)
                {
                    Debug.Log($"[HandUI] - Carte reçue: ID={dto.GameCardId}, Name='{dto.Name}', HP={dto.Hp}, ATK={dto.Attack}");
                }
                AddCards(result.DrawnCards);
            }
            else
            {
                Debug.LogError("[HandUI] ❌ HandleCardsDrawn: Aucune carte dans result OU result NULL");
            }
        }

        /// <summary>
        /// Fallback direct de SignalRClient si MatchService n'existe pas
        /// </summary>
        protected void HandleSignalRCardsDrawn(DrawResultForPlayerDto result)
        {
            Debug.Log("[HandUI] 🔥🔥🔥 HandleSignalRCardsDrawn (fallback direct) - Cartes reçues");
            Debug.Log($"[HandUI] Result: {(result != null ? "NOT NULL" : "NULL")}");
            Debug.Log($"[HandUI] DrawnCards: {(result?.DrawnCards != null ? result.DrawnCards.Count : "NULL")}");
            HandleCardsDrawn(result);
        }

        private void HandleCardSelected(CardUI card)
        {
            if (card == null)
            {
                Debug.LogWarning("[HandUI] HandleCardSelected: card is NULL");
                return;
            }
            
            Debug.Log($"[HandUI] ✅✅✅ HandleCardSelected: '{card.cardName}' (ID={card.cardId})");
            
            if (SelectedCard != null)
            {
                Debug.Log($"[HandUI] Deselecting previous card: '{SelectedCard.cardName}'");
                SelectedCard.SetSelected(false);
            }

            SelectedCard = card;
            SelectedCard.SetSelected(true);
            Debug.Log($"[HandUI] ✅ SelectedCard is now: '{SelectedCard.cardName}'");
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
            Debug.Log($"[HandUI] ========== CardPlayRequested ==========");
            Debug.Log($"[HandUI] Card: {(card != null ? card.cardName : "NULL")} (ID: {(card != null ? card.cardId : "NULL")})");
            Debug.Log($"[HandUI] Slot: {(slot != null ? slot.slotIndex : -1)}");
            _ = RequestPlayCard(card, slot);
        }

        protected void HandleServerStatus(string message)
        {
            Debug.Log($"[HandUI] HandleServerStatus: '{message}'");
            
            if (HandService != null && HandService.ShouldCancelOnServerError(message))
            {
                Debug.LogWarning($"[HandUI] Cancelling pending play due to server error: {message}");
                HandService.CancelPendingPlay("Hub error: " + message);
                
                // Deselect card after server error
                if (SelectedCard != null)
                {
                    SelectedCard.SetSelected(false);
                    SelectedCard = null;
                }
            }
        }

        protected void HandleCardPlayResult(PlayCardPlayerResultDto result)
        {
            Debug.Log($"[HandUI] HandleCardPlayResult called - Result is {(result != null ? "NOT NULL" : "NULL")}");
            if (result == null) 
            {
                Debug.LogError("[HandUI] ❌ PlayCardPlayerResultDto is NULL!");
                return;
            }
            
            Debug.Log($"[HandUI] Result.PlayedCard: {(result.PlayedCard != null ? "NOT NULL" : "NULL")}");
            if (result.PlayedCard != null)
                Debug.Log($"[HandUI] - GameCardId={result.PlayedCard.GameCardId}, Name={result.PlayedCard.Name}");
            
            Debug.Log($"[HandUI] Result.Champion: {(result.Champion != null ? "NOT NULL" : "NULL")}");
            Debug.Log($"[HandUI] Result.location={result.location}, canPlayed={result.canPlayed}");
            
            // Use pending card ID since we know which card we played
            int gameCardId = -1;
            if (HandService?.PendingCard != null && int.TryParse(HandService.PendingCard.cardId, out int id))
            {
                gameCardId = id;
            }
            
            HandService?.ConfirmPlayFromServer(
                gameCardId, 
                result.location, 
                result.canPlayed
            );
        }

        protected void HandlePendingPlayCancelled(string reason)
        {
            HandService?.CancelPendingPlay(reason);
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
                
                // Créer une liste des slots disponibles (vides)
                List<int> availableSlotIndices = new List<int>();
                for (int i = 0; i < _handSlots.Count; i++)
                {
                    Transform slotTransform = _handSlots[i];
                    if (slotTransform == null) continue;
                    
                    CardSlotUI cardSlot = slotTransform.GetComponent<CardSlotUI>();
                    if (cardSlot == null) 
                    {
                        Debug.LogWarning($"[HandUI] ⚠️ Slot {i} ({slotTransform.name}) has NO CardSlotUI component!");
                        continue;
                    }
                    
                    // Vérifier si le slot est vide
                    if (cardSlot.CurrentCard == null)
                    {
                        availableSlotIndices.Add(i);
                        Debug.Log($"[HandUI] Slot {i} ({slotTransform.name}) is available");
                    }
                    else
                    {
                        Debug.Log($"[HandUI] Slot {i} ({slotTransform.name}) already has card: {cardSlot.CurrentCard.cardName}");
                    }
                }
                
                Debug.Log($"[HandUI] Found {availableSlotIndices.Count} available slots");
                
                int cardsAdded = 0;
                foreach (DrawnCardDto dto in drawnCards)
                {
                    if (cardsAdded >= HandService.MaxHandSize)
                    {
                        Debug.LogWarning($"[HandUI] Main pleine. MaxHandSize atteint: {HandService.MaxHandSize}");
                        break;
                    }

                    if (cardsAdded >= availableSlotIndices.Count)
                    {
                        Debug.LogWarning($"[HandUI] ❌ Plus de slots disponibles ! (cardsAdded={cardsAdded}, availableSlots={availableSlotIndices.Count})");
                        break;
                    }

                    int slotIndex = availableSlotIndices[cardsAdded];
                    Transform slot = _handSlots[slotIndex];

                    Debug.Log($"[HandUI] Création carte {cardsAdded + 1}: '{dto.Name}' dans slot {slotIndex} ({slot.name})");
                    
                    CardUI card = Instantiate(_cardPrefab, slot);
                    card.gameObject.name = $"Card_{dto.Name}_{slotIndex}";
                    
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
                    
                    Debug.Log($"[HandUI] ✅ Carte {cardsAdded}: pos={card.transform.localPosition}, active={card.gameObject.activeSelf}");

                    EnsureCollider(card);
                    _handCards.Add(card);
                    
                    // ✅ IMPORTANT: Télécharger le slot et appeler PlaceCard pour le tracker
                    CardSlotUI cardSlot = slot.GetComponent<CardSlotUI>();
                    if (cardSlot != null)
                    {
                        cardSlot.PlaceCard(card);
                        Debug.Log($"[HandUI] ✅ PlaceCard() appelé sur slot {slotIndex}, CurrentCard now: {cardSlot.CurrentCard?.cardName}");
                    }
                    else
                    {
                        Debug.LogError($"[HandUI] ❌ CardSlotUI NON FOUND sur slot {slotIndex}!");
                    }
                    
                    cardsAdded++;
                }
                Debug.Log($"[HandUI] ✅ {cardsAdded} cartes ajoutées avec succès");
            }
            else if (_handRoot != null)
            {
                // Fallback: utiliser _handRoot si aucun slot n'est configuré
                Debug.LogWarning("[HandUI] ⚠️ Aucun slot configuré, fallback sur _handRoot");
                foreach (DrawnCardDto dto in drawnCards)
                {
                    if (_handCards.Count >= HandService.MaxHandSize) break;

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
            if (SelectedCard != null && HandService != null)
            {
                SignalRClient client = SignalRClient.Instance;
                await HandService.RequestPlayCard(SelectedCard, slot, client);
            }
        }

        // ========== SERVICE EVENT HANDLERS ==========
        
        private void HandleServicePlayCancelled(string reason)
        {
            // UI reaction to play cancellation
            if (SelectedCard != null)
                SelectedCard.SetSelected(false);
        }
        
        private void HandleServicePlayConfirmed(int gameCardId, int location, bool canPlayed)
        {
            Debug.Log($"[HandUI] HandleServicePlayConfirmed: gameCardId={gameCardId}, location={location}, canPlayed={canPlayed}");
            
            // UI reaction to server confirmation
            if (canPlayed)
            {
                CardUI pendingCard = HandService.PendingCard;
                CardSlotUI pendingSlot = HandService.PendingSlot;
                
                Debug.Log($"[HandUI] PendingCard: {(pendingCard != null ? pendingCard.cardName : "NULL")}");
                Debug.Log($"[HandUI] PendingSlot: {(pendingSlot != null ? $"slot {pendingSlot.slotIndex}" : "NULL")}");
                
                if (pendingCard == null)
                {
                    Debug.LogError("[HandUI] ❌ PendingCard is NULL!");
                    return;
                }
                
                if (pendingSlot == null)
                {
                    Debug.LogError("[HandUI] ❌ PendingSlot is NULL!");
                    return;
                }
                
                if (!int.TryParse(pendingCard.cardId, out int id))
                {
                    Debug.LogError($"[HandUI] ❌ Cannot parse cardId '{pendingCard.cardId}' to int");
                    return;
                }
                
                Debug.Log($"[HandUI] Parsed cardId={id}, comparing with gameCardId={gameCardId}");
                
                if (id != gameCardId)
                {
                    Debug.LogError($"[HandUI] ❌ CardId mismatch! pending={id} vs server={gameCardId}");
                    return;
                }
                
                Debug.Log($"[HandUI] ✅ Server confirmed play for card '{pendingCard.cardName}' at slot {location}");
                
                // Deselect and remove from hand
                pendingCard.SetSelected(false);
                _handCards.Remove(pendingCard);
                if (SelectedCard == pendingCard)
                    SelectedCard = null;
                
                // Place card in the slot (moves it visually)
                Debug.Log($"[HandUI] Calling PlaceCard for '{pendingCard.cardName}' in slot {pendingSlot.slotIndex}");
                pendingSlot.PlaceCard(pendingCard);
                Debug.Log($"[HandUI] ✅ Card '{pendingCard.cardName}' placed in slot {pendingSlot.slotIndex}");
            }
            else
            {
                Debug.LogWarning($"[HandUI] ❌ Server rejected card play (canPlayed=false)");
                CardUI pendingCard = HandService.PendingCard;
                if (pendingCard != null)
                    pendingCard.SetSelected(false);
            }
        }

        // ========== PRIVATE METHODS ==========

        private void ClearHand()
        {
            if (_handSlots != null && _handSlots.Count > 0)
            {
                foreach (Transform slot in _handSlots)
                {
                    if (slot == null) continue;
                    CardSlotUI cardSlot = slot.GetComponent<CardSlotUI>();
                    if (cardSlot != null)
                        cardSlot.ClearSlot();
                }
            }

            foreach (CardUI card in _handCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            _handCards.Clear();
            SelectedCard = null;
            HandService?.Reset();
        }

        protected void EnsureCollider(CardUI card)
        {
            if (card.GetComponent<Collider>() != null) return;

            BoxCollider bc = card.gameObject.AddComponent<BoxCollider>();
            bc.size = Vector3.one;
        }

        private async Task RequestPlayCard(CardUI card, CardSlotUI slot)
        {
            Debug.Log($"[HandUI] RequestPlayCard called - card={card?.cardName}, slot={slot?.slotIndex}");
            if (HandService == null) 
            {
                Debug.LogError("[HandUI] ❌ HandService is NULL!");
                return;
            }
            
            Debug.Log("[HandUI] ✅ HandService exists, about to call RequestPlayCard");
            
            SignalRClient client = SignalRClient.Instance;
            Debug.Log($"[HandUI] SignalRClient: {(client != null ? "EXISTS" : "NULL")}");
            
            Debug.Log("[HandUI] 🔄 Calling HandService.RequestPlayCard()...");
            await HandService.RequestPlayCard(card, slot, client);
            Debug.Log("[HandUI] ✅ HandService.RequestPlayCard() completed");
        }
    }
}
