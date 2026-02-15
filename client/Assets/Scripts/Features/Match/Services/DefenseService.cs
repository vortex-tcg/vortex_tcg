using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Service de gestion de la logique de défense
    /// Remplace DefenseManager en utilisant MatchEvents
    /// </summary>
    public class DefenseService : MonoBehaviour
    {
        public static DefenseService Instance { get; private set; }

        [SerializeField] private List<CardSlotUI> _playerBoardSlots = new();
        [SerializeField] private List<CardSlotUI> _opponentBoardSlots = new();

        private readonly Dictionary<int, CardUI> _boardCardsById = new();
        private readonly Dictionary<CardUI, CardUI> _defenseAssignments = new();

        private CardUI _currentDefender;

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
            MatchEvents.OnPhaseChanged += HandlePhaseChanged;
            MatchEvents.OnPlayerDefenseEngaged += HandleDefenseEngaged;
            MatchEvents.OnPlayerAttackEngaged += HandleAttackEngaged;
            MatchEvents.OnCardClicked += HandleCardClicked;
        }

        private void OnDisable()
        {
            MatchEvents.OnPhaseChanged -= HandlePhaseChanged;
            MatchEvents.OnPlayerDefenseEngaged -= HandleDefenseEngaged;
            MatchEvents.OnPlayerAttackEngaged -= HandleAttackEngaged;
            MatchEvents.OnCardClicked -= HandleCardClicked;
        }

        private void HandlePhaseChanged(VortexTCG.Scripts.DTOs.PhaseChangeResultDTO result)
        {
            if (result.CurrentPhase == GamePhase.DEFENSE)
            {
                OnEnterDefensePhase();
            }
            else
            {
                ClearAllDefense();
            }
        }

        private void HandleAttackEngaged(AttackResponseDto dto)
        {
            OnEnterDefensePhase();
        }

        // ========== DEFENSE PHASE ==========

        private void OnEnterDefensePhase()
        {
            ClearAllDefense();
            RegisterExistingCardsFromSlots();
            Debug.Log("[DefenseService] Entered DEFENSE phase");
        }

        public void RegisterExistingCardsFromSlots()
        {
            if (_playerBoardSlots != null)
            {
                for (int i = 0; i < _playerBoardSlots.Count; i++)
                {
                    CardSlotUI slot = _playerBoardSlots[i];
                    if (slot?.CurrentCard != null)
                    {
                        RegisterCard(slot.CurrentCard);
                    }
                }
            }

            if (_opponentBoardSlots != null)
            {
                for (int i = 0; i < _opponentBoardSlots.Count; i++)
                {
                    CardSlotUI slot = _opponentBoardSlots[i];
                    if (slot?.CurrentCard != null)
                    {
                        RegisterCard(slot.CurrentCard);
                    }
                }
            }
        }

        public void RegisterCard(CardUI card)
        {
            if (card == null) return;

            if (!int.TryParse(card.cardId, out int id))
                return;

            _boardCardsById[id] = card;
            Debug.Log($"[DefenseService] Registered card {id}");
        }

        // ========== DEFENSE ASSIGNMENT ==========

        public bool IsPlayerBoardSlot(CardSlotUI slot)
        {
            return slot != null && _playerBoardSlots != null && _playerBoardSlots.Contains(slot);
        }

        public bool IsOpponentBoardSlot(CardSlotUI slot)
        {
            return slot != null && _opponentBoardSlots != null && _opponentBoardSlots.Contains(slot);
        }

        public bool CanDefend(CardUI attacker, CardUI defender)
        {
            if (attacker == null || defender == null) return false;
            if (attacker == defender) return false;
            if (!IsPlayerBoardSlot(defender.GetComponentInParent<CardSlotUI>())) return false;

            return true;
        }

        public void AssignDefense(CardUI attacker, CardUI defender)
        {
            if (!CanDefend(attacker, defender)) return;

            _defenseAssignments[attacker] = defender;
            // SetAsDefending disabled - method not available on CardUI
            // defender.SetAsDefending(true);

            Debug.Log($"[DefenseService] Assigned defense: attacker={attacker.cardId} defender={defender.cardId}");
        }

        public void UnassignDefense(CardUI attacker)
        {
            if (!_defenseAssignments.TryGetValue(attacker, out CardUI defender))
                return;

            _defenseAssignments.Remove(attacker);
            if (defender != null)
            {
                // SetAsDefending disabled - method not available on CardUI
                // defender.SetAsDefending(false);
            }

            Debug.Log($"[DefenseService] Unassigned defense for attacker={attacker.cardId}");
        }

        public void ClearAllDefense()
        {
            foreach (var kvp in _defenseAssignments)
            {
                if (kvp.Value != null)
                {
                    // SetAsDefending disabled - method not available on CardUI
                    // kvp.Value.SetAsDefending(false);
                }
            }
            _defenseAssignments.Clear();
            _currentDefender = null;
        }

        // ========== EVENT HANDLERS ==========

        private void HandleDefenseEngaged(DefenseDataResponseDto dto)
        {
            // À implémenter selon votre logique de défense
            Debug.Log($"[DefenseService] Defense engaged from server");
        }

        public void HandleCardClicked(CardUI card)
        {
            // Vérifier si c'est en phase DEFENSE
            PhaseService phaseService = PhaseService.Instance;
            if (phaseService == null || phaseService.CurrentPhase != GamePhase.DEFENSE)
                return;

            if (card == null)
            {
                Debug.LogWarning("[DefenseService] HandleCardClicked: card is NULL");
                return;
            }

            // Logique de défense à implémenter selon vos besoins
            Debug.Log($"[DefenseService] Card clicked in DEFENSE phase: {card.cardName}");
        }
    }
}
