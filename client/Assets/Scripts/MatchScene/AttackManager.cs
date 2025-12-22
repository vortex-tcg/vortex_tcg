using System.Collections.Generic;
using UnityEngine;

namespace VortexTCG.Scripts.MatchScene
{
    public class AttackManager : MonoBehaviour
    {
        public static AttackManager Instance { get; private set; }

        [Header("Player 1 Cards on Board")] [SerializeField]
        private List<CardSlot> P1BoardSlots;

        private readonly List<Card> selectedCards = new List<Card>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            PhaseManager.Instance.OnEnterAttack += OnEnterAttackPhase;
            PhaseManager.Instance.OnEnterDefense += OnEnterDefensePhase;
            PhaseManager.Instance.OnEnterStandBy += OnEndDefensePhase;

            foreach (CardSlot slot in P1BoardSlots)
            {
                if (slot.CurrentCard != null)
                {
                    RegisterCard(slot.CurrentCard);
                }
            }
        }

        private void OnDestroy()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnEnterAttack -= OnEnterAttackPhase;
                PhaseManager.Instance.OnEnterDefense -= OnEnterDefensePhase;
                PhaseManager.Instance.OnEnterStandBy -= OnEndDefensePhase;
            }
        }

        private void OnEnterAttackPhase()
        {
            ClearSelections();
        }

        private void OnEnterDefensePhase()
        {
        }

        private void OnEndDefensePhase()
        {
            ClearSelections();
        }

        public void RegisterCard(Card card)
        {
            if (card == null) return;

            Collider col = card.GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider bc = card.gameObject.AddComponent<BoxCollider>();
                bc.size = Vector3.one;
            }
        }

        public bool IsP1BoardSlot(CardSlot slot)
        {
            return slot != null && P1BoardSlots != null && P1BoardSlots.Contains(slot);
        }

        public bool IsCardOnP1Board(Card card)
        {
            if (card == null) return false;
            CardSlot slot = card.GetComponentInParent<CardSlot>();
            return IsP1BoardSlot(slot);
        }

        public void HandleCardClicked(Card card)
        {
            if (card == null) return;
            if (PhaseManager.Instance == null) return;
            if (PhaseManager.Instance.CurrentPhase != GamePhase.Attack) return;
            if (!IsCardOnP1Board(card)) return;

            ToggleCard(card);
        }

        private void ToggleCard(Card card)
        {
            if (selectedCards.Contains(card))
                DeselectCard(card);
            else
                SelectCard(card);

            UpdateAttackOrderLabels();
        }

        private void SelectCard(Card card)
        {
            selectedCards.Add(card);
            card.SetSelected(true);
        }

        private void DeselectCard(Card card)
        {
            selectedCards.Remove(card);
            card.SetSelected(false);
            card.ClearAttackOrder();
        }

        private void UpdateAttackOrderLabels()
        {
            for (int i = 0; i < selectedCards.Count; i++)
            {
                Card card = selectedCards[i];
                card.ShowAttackOrder(i + 1);
            }
        }

        private void ClearSelections()
        {
            foreach (Card card in selectedCards)
            {
                card.SetSelected(false);
                card.ClearAttackOrder();
            }

            selectedCards.Clear();
        }
    }
}
