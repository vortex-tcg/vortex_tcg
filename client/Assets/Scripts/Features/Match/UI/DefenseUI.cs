using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.MatchScene
{
    /// <summary>
    /// Gère l'interaction UI pour la défense
    /// Wrapper MonoBehaviour autour d'DefenseService
    /// À placer dans la scène Unity
    /// </summary>
    public class DefenseUI : MonoBehaviour
    {
        public static DefenseUI Instance { get; private set; }
        public DefenseService DefenseService { get; private set; }

        [SerializeField] private List<CardSlotUI> P1BoardSlots = new List<CardSlotUI>();
        [SerializeField] private List<CardSlotUI> P2BoardSlots = new List<CardSlotUI>();

        private DefenseService defenseLogic;
        private CardUI selectedDefender;

        private void Awake()
        {
            Instance = this;
            DefenseService = new DefenseService(P1BoardSlots, P2BoardSlots);
            defenseLogic = DefenseService;
        }

        private void Start()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterDefense += OnEnterDefense;
                PhaseService.Instance.OnEnterStandBy += OnEnterStandBy;
            }

            defenseLogic.RegisterExistingCardsFromSlots();
            ClearDefenseStatesFromSlots();
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnDefenseEngage += OnDefenseEngage;

            // Subscribe to card clicks for defense flow
            MatchEvents.OnCardClicked += OnCardClickedHandler;
        }

        private void OnDestroy()
        {
            if (PhaseService.Instance != null)
            {
                PhaseService.Instance.OnEnterDefense -= OnEnterDefense;
                PhaseService.Instance.OnEnterStandBy -= OnEnterStandBy;
            }

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnDefenseEngage -= OnDefenseEngage;

            // Unsubscribe from card clicks
            MatchEvents.OnCardClicked -= OnCardClickedHandler;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnterDefense()
        {
            // Defense state must remain visible during the whole Defense/EndTurn flow.
        }

        private void OnEnterStandBy()
        {
            defenseLogic.ClearAllDefense();
            selectedDefender = null;
        }

        private void OnCardClickedHandler(CardUI card)
        {
            if (card == null)
                return;

            if (PhaseService.Instance == null)
                return;

            // Only handle during defense phase
            if (PhaseService.Instance.CurrentPhase != GamePhase.DEFENSE)
                return;

            if (OpponentBoardUI.Instance == null)
                return;

            bool isOpponentCard = OpponentBoardUI.Instance.IsCardOnOpponentBoard(card);

            Debug.Log($"[DefenseUI] OnCardClickedHandler invoked - card='{card.cardName}' opponent? {isOpponentCard}");

            // If my card: check sleepy, then already defending or select as defender
            if (!isOpponentCard)
            {
                if (card.IsSleepy)
                {
                    Debug.LogWarning($"[DefenseUI] Card '{card.cardName}' is sleepy and cannot defend");
                    return;
                }

                if (card.HasAttackedThisPhase)
                {
                    Debug.LogWarning($"[DefenseUI] Card '{card.cardName}' is already in attack mode and cannot defend this turn");
                    return;
                }

                // If card is already defending, second click toggles defense off.
                if (card.IsDefenseSelected())
                {
                    RemoveDefense(card);
                    return;
                }

                if (defenseLogic.IsDefenseLocked(card))
                {
                    Debug.Log($"[DefenseUI] Card '{card.cardName}' has already defended this turn and cannot be re-selected.");
                    return;
                }

                SelectDefender(card);
                return;
            }

            // If opponent card: assign defense if a defender is selected
            if (isOpponentCard && selectedDefender != null)
            {
                AssignDefense(card);
                return;
            }

            if (isOpponentCard && selectedDefender == null)
            {
                Debug.Log($"[DefenseUI] Aucun defenseur selectionne: si l'attaque de '{card.cardName}' aboutit, le champion recevra les degats.");
            }
        }

        private void SelectDefender(CardUI defender)
        {
            if (defender == null)
                return;

            // Verify defender is on P1 board (not in hand)
            if (!IsCardOnP1Board(defender))
            {
                Debug.LogWarning("[DefenseUI] Seules les cartes posées sur le board peuvent défendre");
                return;
            }

            // remember the defender locally
            selectedDefender = defender;

            Debug.Log($"[DefenseUI] defender '{defender.cardName}' selected for potential defence");
        }

        private void AssignDefense(CardUI attackingCard)
        {
            if (selectedDefender == null || attackingCard == null)
                return;

            // Verify opponent card is in attack mode
            bool attackerIsInAttackMode = attackingCard.HasAttackedThisPhase || attackingCard.IsAttackingOutlineActive();
            if (!attackerIsInAttackMode)
            {
                Debug.LogWarning("[DefenseUI] Cette carte adverse n'est pas en mode attaque");
                return;
            }

            // Activate DefenseState only now that the defense target is chosen.
            defenseLogic.SelectDefender(selectedDefender);
            selectedDefender.SetDefenseSelected(true);

            Debug.Log($"[DefenseUI] Defense assignee: '{selectedDefender.cardName}' (id={selectedDefender.cardId}) defendra contre '{attackingCard.cardName}' (id={attackingCard.cardId}).");

            // Assign defense and send to server
            defenseLogic.SelectDefender(selectedDefender);
            _ = defenseLogic.TryAssignDefenseAndSend(attackingCard);
        }

        private void RemoveDefense(CardUI defenseCard)
        {
            if (defenseCard == null)
                return;

            _ = defenseLogic.RemoveDefenseAndSend(defenseCard);
            
            if (selectedDefender == defenseCard)
            {
                selectedDefender = null;
            }
        }

        public void RegisterCard(CardUI card)
        {
            defenseLogic.RegisterCard(card);
            SetDefenseState(card, false);
            if (SleepManager.IsSleeping && card != null)
                card.SetSleepy(true);
        }

        public bool IsP1BoardSlot(CardSlotUI slot) => defenseLogic.IsP1BoardSlot(slot);
        public bool IsP2BoardSlot(CardSlotUI slot) => defenseLogic.IsP2BoardSlot(slot);

        public bool IsCardOnP1Board(CardUI card) => defenseLogic.IsCardOnP1Board(card);
        public bool IsCardOnP2Board(CardUI card) => defenseLogic.IsCardOnP2Board(card);



        public void ClearAllDefense() => defenseLogic.ClearAllDefense();

        private void OnDefenseEngage(DefenseDataResponseDto data) => defenseLogic.ApplyDefenseStateFromServer(data);

        private void ClearDefenseStatesFromSlots()
        {
            ClearDefenseStatesInSlots(P1BoardSlots);
            ClearDefenseStatesInSlots(P2BoardSlots);
        }

        private void ClearDefenseStatesInSlots(List<CardSlotUI> slots)
        {
            if (slots == null)
                return;

            for (int i = 0; i < slots.Count; i++)
            {
                CardSlotUI slot = slots[i];
                if (slot == null || slot.CurrentCard == null)
                    continue;

                SetDefenseState(slot.CurrentCard, false);
            }
        }

        public void SetDefenseState(CardUI card, bool active)
        {
            if (card == null)
                return;

            GameObject outline = card.GetDefenseState();
            if (outline != null)
                outline.SetActive(active);
        }
    }
}
