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
        }

        private void OnEnable()
        {
            MatchEvents.OnGameStarted += HandleGameStarted;
            MatchEvents.OnOpponentCardsDrawn += HandleOpponentCardsDrawn;
            MatchEvents.OnOpponentCardPlayed += HandleOpponentCardPlayed;
        }

        private void OnDisable()
        {
            MatchEvents.OnGameStarted -= HandleGameStarted;
            MatchEvents.OnOpponentCardsDrawn -= HandleOpponentCardsDrawn;
            MatchEvents.OnOpponentCardPlayed -= HandleOpponentCardPlayed;
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
            if (_faceDownCardPrefab == null || _opponentHandRoot == null)
            {
                Debug.LogError("[OpponentUIManager] Prefab ou Root non assigné.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                CardUI faceDown = Instantiate(_faceDownCardPrefab, _opponentHandRoot);
                faceDown.gameObject.name = $"FaceDownCard_{_opponentHandCards.Count}";
                _opponentHandCards.Add(faceDown);
            }

            Debug.Log($"[OpponentUIManager] Added {count} face-down cards. Total: {_opponentHandCards.Count}");
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
