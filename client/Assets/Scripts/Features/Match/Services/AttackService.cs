using System;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Service de gestion de la logique d'attaque
    /// Remplace AttackManager en utilisant MatchEvents
    /// </summary>
    public class AttackService : MonoBehaviour
    {
        public static AttackService Instance { get; private set; }

        [Header("Player Board")]
        [SerializeField] private List<CardSlotUI> _playerBoardSlots = new();

        private readonly Dictionary<int, CardUI> _boardCardsById = new();
        private readonly List<CardUI> _selectedCards = new();
        private SignalRClient _client;

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
            _client = SignalRClient.Instance;
            
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
            MatchEvents.OnPlayerAttackEngaged += HandleAttackEngaged;
            MatchEvents.OnCardClicked += HandleCardClicked;
        }

        private void OnDisable()
        {
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnPlayerAttackEngaged -= HandleAttackEngaged;
            MatchEvents.OnCardClicked -= HandleCardClicked;
        }

        private void HandlePhaseChanged(VortexTCG.Scripts.DTOs.PhaseChangeResultDTO result)
        {
            if (result.CurrentPhase == GamePhase.ATTACK)
            {
                OnEnterAttackPhase();
            }
            else
            {
                ClearSelections();
            }
        }

        // ========== ATTACK PHASE ==========

        private void OnEnterAttackPhase()
        {
            ClearSelections();
            RegisterExistingCardsFromSlots();
            Debug.Log("[AttackService] Entered ATTACK phase");
        }

        public void RegisterExistingCardsFromSlots()
        {
            if (_playerBoardSlots == null) return;

            for (int i = 0; i < _playerBoardSlots.Count; i++)
            {
                CardSlotUI slot = _playerBoardSlots[i];
                if (slot?.CurrentCard != null)
                {
                    RegisterCard(slot.CurrentCard);
                }
            }
        }

        public void RegisterCard(CardUI card)
        {
            if (card == null) return;

            if (!int.TryParse(card.cardId, out int id))
            {
                Debug.LogError($"[AttackService] RegisterCard: cardId invalide '{card.cardId}'");
                return;
            }

            _boardCardsById[id] = card;

            // Ajouter un collider si nécessaire
            Collider col = card.GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider bc = card.gameObject.AddComponent<BoxCollider>();
                bc.size = Vector3.one;
            }
        }

        // ========== CARD SELECTION ==========

        public bool IsCardOnPlayerBoard(CardUI card)
        {
            if (card == null) return false;
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            return slot != null && _playerBoardSlots.Contains(slot);
        }

        public void ToggleCardSelection(CardUI card)
        {
            if (card == null) return;

            if (_selectedCards.Contains(card))
            {
                _selectedCards.Remove(card);
                card.SetSelected(false);
                Debug.Log($"[AttackService] Deselected card {card.cardId}");
            }
            else
            {
                _selectedCards.Add(card);
                card.SetSelected(true);
                Debug.Log($"[AttackService] Selected card {card.cardId}");
            }
        }

        public void ClearSelections()
        {
            foreach (CardUI card in _selectedCards)
            {
                if (card != null)
                    card.SetSelected(false);
            }
            _selectedCards.Clear();
        }

        // ========== ATTACK ENGAGEMENT ==========

        public async void HandleCardClicked(CardUI card)
        {
            if (card == null)
            {
                Debug.LogWarning("[AttackService] HandleCardClicked: card is NULL");
                return;
            }

            PhaseService phaseService = PhaseService.Instance;
            if (phaseService == null || phaseService.CurrentPhase != GamePhase.ATTACK)
            {
                Debug.LogWarning("[AttackService] Not in ATTACK phase -> ignore click");
                return;
            }

            if (!IsCardOnPlayerBoard(card))
            {
                Debug.LogWarning("[AttackService] Card is NOT on player board -> ignore click");
                return;
            }

            if (!int.TryParse(card.cardId, out int cardIdInt))
            {
                Debug.LogError($"[AttackService] card.cardId not int! value='{card.cardId}'");
                return;
            }

            if (_client == null || !_client.IsConnected)
            {
                Debug.LogError("[AttackService] SignalRClient not connected");
                return;
            }

            Debug.Log($"[AttackService] -> Calling Hub HandleAttackPos(cardId={cardIdInt})");
            ToggleCardSelection(card);

            try
            {
                await _client.HandleAttackPos(cardIdInt);
                Debug.Log($"[AttackService] Hub call HandleAttackPos DONE");
            }
            catch (Exception ex)
            {
                Debug.LogError("[AttackService] HandleAttackPos invoke failed: " + ex);
                ToggleCardSelection(card); // Rollback
            }
        }

        private void HandleAttackEngaged(AttackResponseDto dto)
        {
            // À implémenter selon votre logique d'affichage d'attaque
            Debug.Log($"[AttackService] Attack engaged from server");
        }
    }
}
