using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.UI
{
    /// <summary>
    /// Gestionnaire UI combiné de l'adversaire
    /// Remplace OpponentHandManager et OpponentBoardService en utilisant MatchEvents
    /// </summary>
    public class OpponentUI : MonoBehaviour
    {
        public static OpponentUI Instance { get; private set; }

        [Header("Opponent Hand")]
        [SerializeField] private Transform _opponentHandRoot;
        [SerializeField] private CardUI _cardPrefab;
        [SerializeField] protected List<Transform> _opponentHandSlots = new();

        [Header("Opponent Board")]
        [SerializeField] private List<CardSlotUI> _opponentBoardSlots = new();

        private readonly List<CardUI> _opponentHandCards = new();
        private readonly Dictionary<int, CardUI> _opponentBoardCards = new();
        
        private bool _gameStarted = false;
        private int _pendingCardsCount = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Debug.Log("[OpponentUI] Awake - FindAndAssignOpponentHandSlots");
            FindAndAssignOpponentHandSlots();
            Debug.Log($"[OpponentUI] Awake - Slots trouvés: {_opponentHandSlots.Count}");
        }

        /// <summary>
        /// Cherche automatiquement les slots enfants de la main adversaire
        /// Si des slots sont déjà assignés, ne fait rien
        /// </summary>
        private void FindAndAssignOpponentHandSlots()
        {
            if (_opponentHandSlots.Count > 0)
            {
                Debug.Log($"[OpponentUI] ✅ {_opponentHandSlots.Count} slots assignés manuellement");
                return;
            }
            
            _opponentHandSlots.Clear();
            Transform parent = transform;
            for (int i = 1; i <= 7; i++)
            {
                Transform slot = parent.Find($"P2HandSlot{i}");
                if (slot != null)
                {
                    _opponentHandSlots.Add(slot);
                    Debug.Log($"[OpponentUI] ✅ Slot P2HandSlot{i} trouvé automatiquement");
                }
            }
            
            if (_opponentHandSlots.Count > 0)
            {
                Debug.Log($"[OpponentUI] ✅ {_opponentHandSlots.Count} slots trouvés automatiquement");
            }
            else
            {
                Debug.LogWarning("[OpponentUI] ⚠️ Aucun slot trouvé! Assignez les manuellement ou créez P2HandSlot1-7");
            }
        }

        /// <summary>
        /// Cherche automatiquement les slots du board adversaire
        /// Si des slots sont déjà assignés, ne fait rien
        /// </summary>
        private void FindAndAssignOpponentBoardSlots()
        {
            if (_opponentBoardSlots.Count > 0)
            {
                Debug.Log($"[OpponentUI] ✅ {_opponentBoardSlots.Count} board slots assignés manuellement");
                return;
            }
            
            _opponentBoardSlots.Clear();
            Transform parent = transform;
            Debug.Log($"[OpponentUI] Searching for board slots under: {parent.name}");
            
            for (int i = 1; i <= 7; i++)
            {
                Transform slot = parent.Find($"P2BoardSlot{i}");
                Debug.Log($"[OpponentUI] Looking for P2BoardSlot{i}: {(slot != null ? "✅ FOUND" : "❌ NOT FOUND")}");
                
                if (slot != null)
                {
                    CardSlotUI cardSlot = slot.GetComponent<CardSlotUI>();
                    if (cardSlot != null)
                    {
                        _opponentBoardSlots.Add(cardSlot);
                        
                        cardSlot.isOpponentSlot = true;
                        
                    }
                    else
                    {
                        Debug.LogWarning($"[OpponentUI] ⚠️ P2BoardSlot{i} found but NO CardSlotUI component!");
                    }
                }
            }
            
            if (_opponentBoardSlots.Count > 0)
            {
                Debug.Log($"[OpponentUI] ✅ {_opponentBoardSlots.Count} board slots trouvés automatiquement");
            }
            else
            {
                Debug.LogError("[OpponentUI] ❌ Aucun board slot trouvé! Assignez les manuellement ou créez P2BoardSlot1-7");
            }
        }

        private void OnEnable()
        {
            Debug.Log("[OpponentUI] OnEnable - Inscription aux événements");
            
            // Auto-detect board slots if not assigned
            
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnOpponentCardsDrawn += HandleOpponentCardsDrawn;
            MatchEvents.OnOpponentCardPlayed += HandleOpponentCardPlayed;
            Debug.Log($"[OpponentUI] ✅ OnOpponentCardPlayed subscribed - Instance={(Instance != null ? Instance.name : "NULL")}");

            // Check if we have opponent hand size from matchFound and game hasn't started yet
            if (!_gameStarted && SignalRClient.Instance != null && SignalRClient.Instance.OpponentHandSize > 0)
            {
                Debug.Log($"[OpponentUI] ✅ Opponent hand size available: {SignalRClient.Instance.OpponentHandSize} - displaying face down cards");
                ResetHand();
                AddFaceDownCards(SignalRClient.Instance.OpponentHandSize);
                _gameStarted = true; // Mark as started since we have the initial hand size
            }
        }

        private void OnDisable()
        {
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnOpponentCardsDrawn -= HandleOpponentCardsDrawn;
            MatchEvents.OnOpponentCardPlayed -= HandleOpponentCardPlayed;
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            Debug.Log("[OpponentUI] ✅✅✅ HandleGameStarted appelé - Clearing hand and waiting for CardsDrawn");
            _gameStarted = true;
            ResetHand();
            ResetBoard();
            
            if (_pendingCardsCount > 0)
            {
                Debug.Log($"[OpponentUI] ✅ Applying buffered cards: {_pendingCardsCount}");
                AddFaceDownCards(_pendingCardsCount);
                _pendingCardsCount = 0;
            }
        }

        private void HandleOpponentCardsDrawn(DrawResultForOpponentDto result)
        {
            int count = result?.CardsDrawnCount ?? 0;
            Debug.Log($"[OpponentUI] ✅✅✅ HandleOpponentCardsDrawn appelé - Cards: {count}, GameStarted: {_gameStarted}");
            
            if (!_gameStarted)
            {
                // Le jeu n'a pas encore démarré, stocker les cartes pour plus tard
                _pendingCardsCount += count;
                return;
            }
            
            AddFaceDownCards(count);
        }

        private void HandleOpponentCardPlayed(PlayCardOpponentResultDto result)
        {
            Debug.Log($"[OpponentUI] ========== HandleOpponentCardPlayed CALLED ==========");
            Debug.Log($"[OpponentUI] Instance={(Instance != null ? Instance.name : "NULL")}");
            Debug.Log($"[OpponentUI] Result: {(result != null ? "NOT NULL" : "NULL")}");
            
            if (result == null)
            {
                Debug.LogError("[OpponentUI] ❌ Result is NULL!");
                return;
            }
            
            Debug.Log($"[OpponentUI] Result.location={result.location}, PlayedCard={(result.PlayedCard != null ? "NOT NULL" : "NULL")}");
            if (result.PlayedCard != null)
                Debug.Log($"[OpponentUI] - PlayedCard.GameCardId={result.PlayedCard.GameCardId}, Name={result.PlayedCard.Name}");
            
            RemoveOneCardFromHand();
            
            if (result?.location >= 0 && result.PlayedCard != null)
            {
                Debug.Log($"[OpponentUI] ✅ Calling PlaceCardOnBoard(location={result.location})");
                PlaceCardOnBoard(result.location, result.PlayedCard);
            }
            else
            {
                Debug.LogWarning($"[OpponentUI] ⚠️ Cannot place card: location={result?.location}, playedCard={(result?.PlayedCard != null)}");
            }
        }

        // ========== HAND METHODS ==========

        public void ResetHand()
        {
            foreach (CardUI card in _opponentHandCards)
            {
                Destroy(card.gameObject);
            }
            _opponentHandCards.Clear();
        }

        public void AddFaceDownCards(int count)
        {
            if (_cardPrefab == null)
            {
                Debug.LogError("[OpponentUI] ❌ Prefab carte non assigné.");
                return;
            }

            Debug.Log($"[OpponentUI] AddFaceDownCards appelé - {count} cartes à ajouter, Slots={_opponentHandSlots.Count}");

            if (_opponentHandSlots != null && _opponentHandSlots.Count > 0)
            {
                Debug.Log($"[OpponentUI] ✅ Utilisation de {_opponentHandSlots.Count} slots adversaire");
                for (int i = 0; i < count && i < _opponentHandSlots.Count; i++)
                {
                    Transform slot = _opponentHandSlots[i];
                    if (slot == null)
                    {
                        Debug.LogError($"[OpponentUI] ❌ Slot {i} est NULL!");
                        continue;
                    }

                    Debug.Log($"[OpponentUI] Création carte adversaire {i + 1} dans slot '{slot.name}'");
                    CardSlotUI cardSlot = slot.GetComponent<CardSlotUI>();
                    
                    CardUI faceDown = Instantiate(_cardPrefab, slot);
                    faceDown.gameObject.name = $"OpponentCard_{i}";
                    cardSlot?.SetCurrentCard(faceDown);
                    
                    faceDown.SetFaceDown(true);
                    faceDown.SetFaceDown(true);
                    
                    faceDown.transform.localPosition = Vector3.zero;
                    faceDown.transform.localScale = Vector3.one;
                    Debug.Log($"[OpponentUI] ✅ Carte adversaire {i}: pos={faceDown.transform.localPosition}, parent={faceDown.transform.parent.name}");
                    
                    _opponentHandCards.Add(faceDown);
                }
                Debug.Log($"[OpponentUI] ✅ {count} cartes adversaire ajoutées. Total: {_opponentHandCards.Count}");
            }
            else if (_opponentHandRoot != null)
            {
                Debug.LogWarning("[OpponentUI] ⚠️ Aucun slot configuré, fallback sur _opponentHandRoot");
                for (int i = 0; i < count; i++)
                {
                    CardUI faceDown = Instantiate(_cardPrefab, _opponentHandRoot);
                    
                    faceDown.SetFaceDown(true);
                    
                    _opponentHandCards.Add(faceDown);
                }
                Debug.Log($"[OpponentUI] ✅ {count} cartes adversaire (fallback) ajoutées. Total: {_opponentHandCards.Count}");
            }
            else
            {
                Debug.LogError("[OpponentUI] ❌ Ni slots ni handRoot assignés!");
            }
        }

        public void RemoveOneCardFromHand()
        {
            if (_opponentHandCards.Count == 0) return;

            CardUI card = _opponentHandCards[0];
            Destroy(card.gameObject);
            _opponentHandCards.RemoveAt(0);

            Debug.Log($"[OpponentUI] Removed 1 card from opponent hand. Total: {_opponentHandCards.Count}");
        }

        // ========== BOARD METHODS ==========

        public void ResetBoard()
        {
            foreach (var card in _opponentBoardCards.Values)
            {
                Destroy(card.gameObject);
            }
            _opponentBoardCards.Clear();

            foreach (CardSlotUI slot in _opponentBoardSlots)
            {
                if (slot != null)
                    slot.ClearSlot();
            }
        }

        public void PlaceCardOnBoard(int slotIndex, GameCardDto cardDto)
        {
            Debug.Log($"[OpponentUI] ========== PlaceCardOnBoard ==========");
            Debug.Log($"[OpponentUI] slotIndex={slotIndex}, cardDto={(cardDto != null ? cardDto.GameCardId : "NULL")}");
            Debug.Log($"[OpponentUI] _opponentBoardSlots.Count={_opponentBoardSlots.Count}");
            
            if (slotIndex < 0 || slotIndex >= _opponentBoardSlots.Count)
            {
                Debug.LogError($"[OpponentUI] ❌ Invalid slot index: {slotIndex} (range: 0-{_opponentBoardSlots.Count - 1})");
                return;
            }

            if (cardDto == null)
            {
                Debug.LogError($"[OpponentUI] ❌ cardDto is NULL at slot {slotIndex}");
                return;
            }

            CardSlotUI slot = _opponentBoardSlots[slotIndex];
            Debug.Log($"[OpponentUI] slot at index {slotIndex}: {(slot != null ? "NOT NULL" : "NULL")}");
            
            if (slot == null)
            {
                Debug.LogError($"[OpponentUI] ❌ slot is NULL at index {slotIndex}");
                return;
            }

            Debug.Log($"[OpponentUI] Using slot: name={slot.gameObject.name}, isOpponentSlot={slot.isOpponentSlot}, slotIndex={slot.slotIndex}");
            if (!slot.isOpponentSlot)
            {
                Debug.LogError($"[OpponentUI] ❌ ERROR: Trying to place opponent card on a PLAYER slot! This is not an opponent board slot!");
            }

            if (slot.CurrentCard != null)
            {
                Debug.LogWarning($"[OpponentUI] ⚠️ Slot {slotIndex} already has a card! Destroying it first");
                Destroy(slot.CurrentCard.gameObject);
            }
            
            if (_cardPrefab == null)
            {
                Debug.LogError("[OpponentUI] ❌ _cardPrefab is NULL!");
                return;
            }

            Debug.Log($"[OpponentUI] Creating card from prefab: {_cardPrefab.name}");
            CardUI card = Instantiate(_cardPrefab, slot.transform, false);
            card.name = $"OpponentCard_{cardDto.GameCardId}";
            
            // Apply card data
            card.ApplyDTO(
                cardDto.GameCardId.ToString(),
                cardDto.Name,
                cardDto.Hp,
                cardDto.Attack,
                cardDto.Cost,
                cardDto.Description ?? "",
                ""
            );

            card.SetFaceDown(false);
            Debug.Log($"[OpponentUI] ✅ Card set to face-up (visible on board)");

            // Place the card in the slot
            slot.PlaceCard(card);

            if (int.TryParse(cardDto.GameCardId.ToString(), out int id))
            {
                _opponentBoardCards[id] = card;
                
                // Register in OpponentBoardService for attack/defense mechanics
                if (OpponentBoardUI.Instance != null && OpponentBoardUI.Instance.OpponentBoardService != null)
                {
                    OpponentBoardUI.Instance.OpponentBoardService.RegisterOpponentCard(id, card);
                    Debug.Log($"[OpponentUI] ✅ Card {cardDto.Name} placed at slot {slotIndex} and registered in OpponentBoardService");
                }
                else
                {
                    Debug.LogWarning($"[OpponentUI] ⚠️ OpponentBoardUI.Instance or OpponentBoardService is NULL - card not registered for attack/defense tracking");
                }
            }
            else
            {
                Debug.LogError($"[OpponentUI] ❌ Could not parse gameCardId: {cardDto.GameCardId}");
            }
        }
    }
}
