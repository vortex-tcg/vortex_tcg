using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.MatchScene
{
    /// <summary>
    /// Gère l'interaction UI pour les attaques
    /// Wrapper MonoBehaviour autour d'AttackService
    /// À placer dans la scène Unity
    /// </summary>
    public class AttackUI : MonoBehaviour
    {
        public static AttackUI Instance { get; private set; }
        public AttackService AttackService { get; private set; }
        
        /// <summary>
        /// Liste ordonnée des cartes actuellement en mode attaque
        /// </summary>
        public IReadOnlyList<CardUI> AttackingCards => attackingCards;

        [Header("Player 1 Cards on Board")]
        [SerializeField] private List<CardSlotUI> P1BoardSlots = new List<CardSlotUI>();

        private AttackService attackLogic;
        private CardUI selectedAttacker;
        private List<CardUI> attackingCards = new List<CardUI>();
        private bool _attackSlotsAreOneBased;

        private void Awake()
        {
            Instance = this;
            AttackService = new AttackService(P1BoardSlots);
            attackLogic = AttackService;
        }

        private void Start()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterAttack += OnEnterAttackPhase;
                PhaseService.Instance.OnEnterDefense += OnEnterDefensePhase;
                PhaseService.Instance.OnEnterEndTurn += OnEnterEndTurnPhase;
            }
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage += OnAttackEngage;

            // Subscribe to card clicks for attack flow
            MatchEvents.OnCardClicked += OnCardClickedHandler;

            attackLogic.RegisterExistingCardsFromSlots();
            ClearAttackStatesFromSlots();
            DetectAttackSlotIndexBase();
        }

        private void OnDestroy()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterAttack -= OnEnterAttackPhase;
                PhaseService.Instance.OnEnterDefense -= OnEnterDefensePhase;
                PhaseService.Instance.OnEnterEndTurn -= OnEnterEndTurnPhase;
            }

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage -= OnAttackEngage;

            // Unsubscribe from card clicks
            MatchEvents.OnCardClicked -= OnCardClickedHandler;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnterAttackPhase()
        {
        }
        private void OnEnterDefensePhase() { }
        private void OnEnterEndTurnPhase()
        {
            ResetAllAttackStates();
        }

        public void ResetAllAttackStates()
        {
            selectedAttacker = null;
            attackLogic.ClearSelections();
            attackLogic.ResetAllCardAttackStates();
            attackingCards.Clear();
            ClearAttackStatesFromSlots();
        }

        public void ResetBoard()
        {
            ResetAllAttackStates();

            if (P1BoardSlots == null)
                return;

            for (int i = 0; i < P1BoardSlots.Count; i++)
            {
                CardSlotUI slot = P1BoardSlots[i];
                if (slot == null)
                    continue;

                slot.ClearSlot();
            }
        }

        public void RegisterCard(CardUI card)
        {
            attackLogic.RegisterCard(card);
            SetAttackState(card, false);
            if (SleepManager.IsSleeping && card != null)
                card.SetSleepy(true);
        }

        public bool IsP1BoardSlot(CardSlotUI slot) => attackLogic.IsP1BoardSlot(slot);

        public bool IsCardOnP1Board(CardUI card) => attackLogic.IsCardOnP1Board(card);

        public void HandleCardClicked(CardUI card)
        {
            if (card == null)
            {
                Debug.LogWarning("[AttackUI] HandleCardClicked: card is NULL");
                return;
            }

            if (PhaseService.Instance == null)
            {
                Debug.LogWarning("[AttackUI] HandleCardClicked: PhaseService.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackUI] Click card name='{card.name}' cardId='{card.cardId}' " +
                      $"phase={PhaseService.Instance.CurrentPhase} onP1Board={IsCardOnP1Board(card)}");

            if (PhaseService.Instance.CurrentPhase != GamePhase.ATTACK)
            {
                Debug.LogWarning("[AttackUI] Not in ATTACK phase -> ignore click");
                return;
            }

            if (card.IsSleepy)
            {
                Debug.LogWarning($"[AttackUI] Card '{card.cardName}' is sleepy and cannot be used");
                return;
            }

            if (!IsCardOnP1Board(card))
            {
                Debug.LogWarning("[AttackUI] Card is NOT on P1 board -> ignore click");
                return;
            }

            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            if (slot == null)
            {
                Debug.LogError($"[AttackUI] Cannot resolve board slot for card '{card.cardName}'");
                return;
            }

            int rawAttackPosition = slot.slotIndex;
            int attackPosition = ResolveAttackPosition(slot, card);

            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[AttackUI] SignalRClient.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackUI] -> sending attack toggle through AttackService rawPosition={rawAttackPosition} normalizedPosition={attackPosition} oneBased={_attackSlotsAreOneBased}");
            // Server is authoritative for attack selection state.
            _ = attackLogic.ToggleCardAttackOnServer(client, attackPosition, card);
        }

        private int ResolveAttackPosition(CardSlotUI slot, CardUI card)
        {
            if (slot == null)
                return 0;

            if (P1BoardSlots != null)
            {
                for (int i = 0; i < P1BoardSlots.Count; i++)
                {
                    CardSlotUI configured = P1BoardSlots[i];
                    if (configured == null)
                        continue;

                    if (configured == slot || configured.CurrentCard == card)
                        return i;
                }
            }

            return NormalizeAttackPosition(slot);
        }

        private void DetectAttackSlotIndexBase()
        {
            if (P1BoardSlots == null || P1BoardSlots.Count == 0)
            {
                _attackSlotsAreOneBased = false;
                return;
            }

            List<int> indices = P1BoardSlots
                .Where(s => s != null)
                .Select(s => s.slotIndex)
                .Distinct()
                .OrderBy(i => i)
                .ToList();

            bool hasZero = indices.Contains(0);
            bool looksOneBasedRange = indices.Count > 0 && indices.First() == 1 && indices.Last() == indices.Count;
            _attackSlotsAreOneBased = !hasZero && looksOneBasedRange;

            Debug.Log($"[AttackUI] Slot index base detection: indices=[{string.Join(",", indices)}] oneBased={_attackSlotsAreOneBased}");
        }

        private int NormalizeAttackPosition(CardSlotUI slot)
        {
            if (slot == null)
                return 0;

            int position = slot.slotIndex;
            if (_attackSlotsAreOneBased)
            {
                position = Mathf.Max(0, position - 1);
            }

            // Fallback safety: if scene slot index is out of server board range,
            // convert from one-based to zero-based for the last slot edge case.
            if (P1BoardSlots != null && P1BoardSlots.Count > 0)
            {
                int maxServerIndex = P1BoardSlots.Count - 1;
                if (position > maxServerIndex && slot.slotIndex > 0)
                {
                    position = slot.slotIndex - 1;
                }

                position = Mathf.Clamp(position, 0, maxServerIndex);
            }

            return position;
        }

        private void ToggleCard(CardUI card)
        {
            if (card.IsSelected)
                DeselectCard(card);
            else
                SelectCard(card);
        }

        private void SelectCard(CardUI card)
        {
            card.SetSelected(true);
        }

        private void DeselectCard(CardUI card)
        {
            card.SetSelected(false);
            card.ClearAttackOrder();
        }

        public void ClearSelections() => attackLogic.ClearSelections();

        private void OnAttackEngage(List<int> attackIds)
        {
            attackLogic.ApplyAttackStateFromServer(attackIds);
            SyncAttackingCardsFromServer(attackIds);
        }

        private void SyncAttackingCardsFromServer(List<int> attackIds)
        {
            attackingCards.Clear();

            if (attackIds == null || attackIds.Count == 0)
            {
                selectedAttacker = null;
                return;
            }

            for (int i = 0; i < attackIds.Count; i++)
            {
                int id = attackIds[i];
                CardUI card = FindBoardCardById(id);
                if (card != null)
                {
                    attackingCards.Add(card);
                }
            }

            if (selectedAttacker != null && !attackingCards.Contains(selectedAttacker))
            {
                selectedAttacker = null;
            }
        }

        private CardUI FindBoardCardById(int id)
        {
            if (P1BoardSlots == null)
                return null;

            for (int i = 0; i < P1BoardSlots.Count; i++)
            {
                CardSlotUI slot = P1BoardSlots[i];
                if (slot == null || slot.CurrentCard == null)
                    continue;

                if (int.TryParse(slot.CurrentCard.cardId, out int cardId) && cardId == id)
                    return slot.CurrentCard;
            }

            return null;
        }

        private void ClearAttackStatesFromSlots()
        {
            if (P1BoardSlots == null)
                return;

            for (int i = 0; i < P1BoardSlots.Count; i++)
            {
                CardSlotUI slot = P1BoardSlots[i];
                if (slot == null || slot.CurrentCard == null)
                    continue;

                CardUI card = slot.CurrentCard;
                SetAttackState(card, false);
                card.SetSelected(false);
                card.ClearAttackOrder();
                card.ResetAttackState();
            }
        }

        public void UpdateAttackStateForSelection(CardUI card, bool selected)
        {
            if (card == null)
                return;

            if (!selected)
            {
                SetAttackState(card, false);
                return;
            }

            if (PhaseService.Instance == null || PhaseService.Instance.CurrentPhase != GamePhase.ATTACK)
            {
                SetAttackState(card, false);
                return;
            }

            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            if (slot != null && !slot.isOpponentSlot)
                SetAttackState(card, true);
            else
                SetAttackState(card, false);
        }

        public void SetOpponentAttackState(CardUI card, bool active)
        {
            SetAttackState(card, active);
        }

        private void SetAttackState(CardUI card, bool active)
        {
            if (card == null)
            {
                Debug.LogWarning("[AttackUI] SetAttackState called with NULL card");
                return;
            }

            Debug.Log($"[AttackUI] SetAttackState called on card '{card.cardName}' (ID: {card.cardId}) with active={active}");

            GameObject state = card.GetAttackState();
            if (state != null)
            {
                state.SetActive(active);
                Debug.Log($"[AttackUI] AttackState state {(active ? "activated" : "deactivated")} for card '{card.cardName}'");
            }
            else
            {
                Debug.LogWarning($"[AttackUI] AttackState state is NULL for card '{card.cardName}'");
            }
        }

        private void OnCardClickedHandler(CardUI card)
        {
            HandleCardClicked(card);
        }

        private void SelectAttacker(CardUI card)
        {
            if (card == null)
                return;

            // Toggle attack mode: select or deselect
            if (card.IsSelected)
            {
                // Already in attack mode, deselect
                card.SetSelected(false);
                card.SetAttackedThisPhase(false); // Reset the attack flag to allow reactivation
                card.ClearAttackOrder(); // Clear the attack order display
                
                // Remove from attacking cards list
                attackingCards.Remove(card);
                
                // Update attack order for all remaining cards
                UpdateAttackOrderDisplay();
                
                if (selectedAttacker == card)
                {
                    selectedAttacker = null;
                }
                
                Debug.Log($"[AttackUI] '{card.cardName}' retiré du mode attaque. Cartes restantes: {attackingCards.Count}");
            }
            else
            {
                // Check if the card has already attacked in this phase
                if (card.HasAttackedThisPhase)
                {
                    Debug.LogWarning($"[AttackUI] Cette carte a déjà attaqué cette phase");
                    return;
                }

                // Put card in attack mode
                card.SetSelected(true);
                card.SetAttackedThisPhase(true);
                
                // Add to attacking cards list
                attackingCards.Add(card);
                
                // Display the order number based on position in list
                int attackOrder = attackingCards.Count;
                card.ShowAttackOrder(attackOrder);
                
                selectedAttacker = card;
                
                Debug.Log($"[AttackUI] '{card.cardName}' est en mode attaque (ATK={card.attack}) - Ordre: {attackOrder}");
            }
        }

        /// <summary>
        /// Met à jour l'affichage de l'ordre d'attaque pour toutes les cartes dans la liste
        /// </summary>
        private void UpdateAttackOrderDisplay()
        {
            for (int i = 0; i < attackingCards.Count; i++)
            {
                CardUI card = attackingCards[i];
                if (card != null)
                {
                    card.ShowAttackOrder(i + 1); // Position in list (1-indexed)
                }
            }
        }


    }
}
