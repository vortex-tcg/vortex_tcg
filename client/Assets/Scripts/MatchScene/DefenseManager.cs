using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{

    public class DefenseManager : MonoBehaviour
    {
        public static DefenseManager Instance { get; private set; }

        [Header("Board Slots")] [SerializeField]
        private List<CardSlot> P1BoardSlots = new List<CardSlot>();

        [SerializeField] private List<CardSlot> P2BoardSlots = new List<CardSlot>();

        private Card currentDefender;

        private readonly Dictionary<Card, Card>
            defenseAssignments = new Dictionary<Card, Card>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnEnterDefense += OnEnterDefense;
                PhaseManager.Instance.OnEnterStandBy += OnExitDefense;
                PhaseManager.Instance.OnEnterAttack += OnExitDefense;
            }
        }

        private void OnDestroy()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnEnterDefense -= OnEnterDefense;
                PhaseManager.Instance.OnEnterStandBy -= OnExitDefense;
                PhaseManager.Instance.OnEnterAttack -= OnExitDefense;
            }
        }

        private void OnEnterDefense()
        {
            ClearAllDefense();
        }

        private void OnExitDefense()
        {
            ClearAllDefense();
        }

        public bool IsP1BoardSlot(CardSlot slot)
        {
            return slot != null && P1BoardSlots.Contains(slot);
        }

        public bool IsP2BoardSlot(CardSlot slot)
        {
            return slot != null && P2BoardSlots.Contains(slot);
        }

        public void HandleCardClicked(Card card)
        {
            if (card == null) return;
            if (PhaseManager.Instance == null || PhaseManager.Instance.CurrentPhase != GamePhase.DEFENSE) return;

            CardSlot slot = card.GetComponentInParent<CardSlot>();
            if (slot == null) return;

            if (IsP1BoardSlot(slot))
            {
                SelectDefender(card);
            }
            else if (IsP2BoardSlot(slot))
            {
                TryAssignDefense(card);
            }
        }

        private void SelectDefender(Card defender)
        {
            if (currentDefender == defender) return;

            currentDefender = defender;
            currentDefender.SetDefenseSelected(true);
        }

        private void TryAssignDefense(Card targetAttacker)
        {
            if (currentDefender == null) return;

            if (!targetAttacker.IsAttackingOutlineActive()) return;

            if (defenseAssignments.ContainsKey(currentDefender))
            {
                defenseAssignments.Remove(currentDefender);
            }

            defenseAssignments[currentDefender] = targetAttacker;
        }

        private void ClearAllDefense()
        {
            if (currentDefender != null)
            {
                currentDefender.SetDefenseSelected(false);
                currentDefender = null;
            }

            foreach (KeyValuePair<Card, Card> kvp in defenseAssignments)
            {
                if (kvp.Key != null)
                    kvp.Key.SetDefenseSelected(false);
            }

            defenseAssignments.Clear();
        }

        private string CardLabel(Card c)
        {
            if (c == null) return "null";
            return string.IsNullOrEmpty(c.cardId) ? c.cardName : $"{c.cardName} ({c.cardId})";
        }
    }
}
