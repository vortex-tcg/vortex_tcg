using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.UI
{
    /// <summary>
    /// Gestionnaire UI combiné de l'adversaire
    /// Remplace OpponentHandManager et OpponentBoardManager en utilisant MatchEvents
    /// </summary>
    public class OpponentUI : MonoBehaviour
    {
        public static OpponentUI Instance { get; private set; }

        [Header("Opponent Hand")]
        [SerializeField] private Transform _opponentHandRoot;
        [SerializeField] private CardUI _faceDownCardPrefab;
        [SerializeField] protected List<Transform> _opponentHandSlots = new();

        [Header("Opponent Board")]
        [SerializeField] private List<CardSlotUI> _opponentBoardSlots = new();

        private readonly List<CardUI> _opponentHandCards = new();
        private readonly Dictionary<int, CardUI> _opponentBoardCards = new();

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
            // Si les slots sont déjà assignés manuellement dans l'inspecteur, ne pas chercher
            if (_opponentHandSlots.Count > 0)
            {
                Debug.Log($"[OpponentUI] ✅ {_opponentHandSlots.Count} slots assignés manuellement");
                return;
            }
            
            // Sinon, cherche automatiquement P2HandSlot1 à P2HandSlot7
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

        private void OnEnable()
        {
            Debug.Log("[OpponentUI] OnEnable - Inscription aux événements");
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnOpponentCardsDrawn += HandleOpponentCardsDrawn;
            MatchEvents.OnOpponentCardPlayed += HandleOpponentCardPlayed;
            Debug.Log("[OpponentUI] ✅ Événements MatchEvents enregistrés");

            // Fallback: s'enregistrer directement auprès de SignalRClient
            SignalRClient client = SignalRClient.Instance;
            if (client != null)
            {
                Debug.Log("[OpponentUI] SignalRClient trouvé, s'enregistrer directement pour OpponentCardsDrawn");
                client.OnOpponentCardsDrawn += HandleSignalROpponentCardsDrawn;
            }
        }

        private void OnDisable()
        {
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnOpponentCardsDrawn -= HandleOpponentCardsDrawn;
            MatchEvents.OnOpponentCardPlayed -= HandleOpponentCardPlayed;

            // Fallback: se désinscrire de SignalRClient
            SignalRClient client = SignalRClient.Instance;
            if (client != null)
            {
                client.OnOpponentCardsDrawn -= HandleSignalROpponentCardsDrawn;
            }
        }

        private void HandleGameStarted(PhaseChangeResultDTO result)
        {
            ResetHand();
            ResetBoard();
        }

        private void HandleOpponentCardsDrawn(DrawResultForOpponentDto result)
        {
            int count = result?.CardsDrawnCount ?? 0;
            AddFaceDownCards(count);
        }

        /// <summary>
        /// Fallback direct de SignalRClient si MatchService n'existe pas
        /// </summary>
        private void HandleSignalROpponentCardsDrawn(DrawResultForOpponentDto result)
        {
            Debug.Log("[OpponentUI] 🔥 HandleSignalROpponentCardsDrawn (fallback direct) - Cartes adversaire reçues");
            HandleOpponentCardsDrawn(result);
        }

        private void HandleOpponentCardPlayed(PlayCardOpponentResultDto result)
        {
            RemoveOneCardFromHand();
            
            if (result?.location >= 0 && result.PlayedCard != null)
            {
                PlaceCardOnBoard(result.location, result.PlayedCard);
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
            if (_faceDownCardPrefab == null)
            {
                Debug.LogError("[OpponentUI] ❌ Prefab face cachée non assigné.");
                return;
            }

            Debug.Log($"[OpponentUI] AddFaceDownCards appelé - {count} cartes à ajouter, Slots={_opponentHandSlots.Count}");

            // Utiliser les slots pré-définis si disponibles
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
                    
                    CardUI faceDown = Instantiate(_faceDownCardPrefab, slot);
                    faceDown.gameObject.name = $"OpponentCard_{i}";
                    
                    // Positionner la carte sur le slot
                    faceDown.transform.localPosition = Vector3.zero;
                    faceDown.transform.localRotation = Quaternion.identity;
                    faceDown.transform.localScale = Vector3.one;
                    
                    Debug.Log($"[OpponentUI] ✅ Carte adversaire {i}: pos={faceDown.transform.localPosition}, parent={faceDown.transform.parent.name}");
                    
                    _opponentHandCards.Add(faceDown);
                }
                Debug.Log($"[OpponentUI] ✅ {count} cartes adversaire ajoutées. Total: {_opponentHandCards.Count}");
            }
            else if (_opponentHandRoot != null)
            {
                // Fallback: utiliser _opponentHandRoot si aucun slot n'est configuré
                Debug.LogWarning("[OpponentUI] ⚠️ Aucun slot configuré, fallback sur _opponentHandRoot");
                for (int i = 0; i < count; i++)
                {
                    CardUI faceDown = Instantiate(_faceDownCardPrefab, _opponentHandRoot);
                    faceDown.gameObject.name = $"FaceDownCard_{_opponentHandCards.Count}";
                    _opponentHandCards.Add(faceDown);
                }
                Debug.Log($"[OpponentUI] Added {count} face-down cards. Total: {_opponentHandCards.Count}");
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

            Debug.Log($"[OpponentUIManager] Removed 1 card from opponent hand. Total: {_opponentHandCards.Count}");
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
            if (slotIndex < 0 || slotIndex >= _opponentBoardSlots.Count)
            {
                Debug.LogError($"[OpponentUIManager] Invalid slot index: {slotIndex}");
                return;
            }

            CardSlotUI slot = _opponentBoardSlots[slotIndex];
            if (slot == null) return;

            // Créer et placer la carte
            CardUI card = new CardUI(); // À implémenter selon votre CardUI.cs
            slot.PlaceCard(card);

            if (int.TryParse(cardDto.GameCardId.ToString(), out int id))
            {
                _opponentBoardCards[id] = card;
            }

            Debug.Log($"[OpponentUIManager] Placed card at slot {slotIndex}");
        }
    }
}
