using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                PhaseService.Instance.OnEnterStandBy += OnEndDefensePhase;
            }
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage += OnAttackEngage;

            // Subscribe to card clicks for attack flow
            MatchEvents.OnCardClicked += OnCardClickedHandler;

            attackLogic.RegisterExistingCardsFromSlots();
            ClearAttackStatesFromSlots();
        }

        private void OnDestroy()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterAttack -= OnEnterAttackPhase;
                PhaseService.Instance.OnEnterDefense -= OnEnterDefensePhase;
                PhaseService.Instance.OnEnterStandBy -= OnEndDefensePhase;
            }

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage -= OnAttackEngage;

            // Unsubscribe from card clicks
            MatchEvents.OnCardClicked -= OnCardClickedHandler;
        }

        private void OnEnterAttackPhase()
        {
            attackLogic.ClearSelections();
            attackLogic.ResetAllCardAttackStates();
            attackingCards.Clear(); // Clear the attacking cards list
        }
        private void OnEnterDefensePhase() { }
        private void OnEndDefensePhase()
        {
            attackLogic.ClearSelections();
            attackingCards.Clear(); // Clear the attacking cards list at the end of defense phase
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

            if (!int.TryParse(card.cardId, out int cardIdInt))
            {
                Debug.LogError($"[AttackUI] card.cardId not int! value='{card.cardId}'");
                return;
            }

            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[AttackUI] SignalRClient.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackUI] -> calling Hub HandleAttackPos(cardId={cardIdInt})");
            ToggleCard(card);

            _ = SendAttackToServer(client, cardIdInt, card);
        }

        private async Task SendAttackToServer(SignalRClient client, int cardIdInt, CardUI card)
        {
            try
            {
                await client.HandleAttackPos(cardIdInt);
                Debug.Log($"[AttackUI] Hub call HandleAttackPos DONE cardId={cardIdInt}");
            }
            catch (Exception ex)
            {
                ToggleCard(card);
                Debug.LogError($"[AttackUI] Hub call HandleAttackPos FAILED cardId={cardIdInt} ex={ex}");
            }
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

        private void OnAttackEngage(List<int> attackIds) => attackLogic.ApplyAttackStateFromServer(attackIds);

        private void ClearAttackStatesFromSlots()
        {
            if (P1BoardSlots == null)
                return;

            for (int i = 0; i < P1BoardSlots.Count; i++)
            {
                CardSlotUI slot = P1BoardSlots[i];
                if (slot == null || slot.CurrentCard == null)
                    continue;

                SetAttackState(slot.CurrentCard, false);
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
            if (card == null)
                return;

            if (PhaseService.Instance == null)
                return;
            
            if (PhaseService.Instance.CurrentPhase != GamePhase.ATTACK)
                return;

            // Check if card is on the player's board
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            if (slot == null)
            {
                Debug.LogWarning($"[AttackUI] Card '{card.cardName}' is not in a CardSlotUI - likely in hand or elsewhere");
                return;
            }

            bool isOnP1Board = IsCardOnP1Board(card);
            
            Debug.Log($"[AttackUI] OnCardClickedHandler: card='{card.cardName}' isOnP1Board={isOnP1Board} slot={slot.name}");

            // Only allow selecting cards from the player's board for attack
            if (isOnP1Board)
            {
                SelectAttacker(card);
            }
            else
            {
                Debug.LogWarning($"[AttackUI] ❌ Card '{card.cardName}' is not on your board - cannot attack with it");
            }
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
