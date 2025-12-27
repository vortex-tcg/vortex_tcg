using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using System;

namespace VortexTCG.Scripts.MatchScene
{
    public class AttackManager : MonoBehaviour
    {
        public static AttackManager Instance { get; private set; }

        [Header("Player 1 Cards on Board")]
        [SerializeField] private List<CardSlot> P1BoardSlots;

        private readonly Dictionary<int, Card> boardCardsById = new();
        private readonly List<Card> selectedCards = new();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            PhaseManager.Instance.OnEnterAttack += OnEnterAttackPhase;
            PhaseManager.Instance.OnEnterDefense += OnEnterDefensePhase;
            PhaseManager.Instance.OnEnterStandBy += OnEndDefensePhase;

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage += ApplyAttackStateFromServer;

            foreach (CardSlot slot in P1BoardSlots)
                if (slot.CurrentCard != null) RegisterCard(slot.CurrentCard);
        }

        private void OnDestroy()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnEnterAttack -= OnEnterAttackPhase;
                PhaseManager.Instance.OnEnterDefense -= OnEnterDefensePhase;
                PhaseManager.Instance.OnEnterStandBy -= OnEndDefensePhase;
            }

            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage -= ApplyAttackStateFromServer;
        }

        private void OnEnterAttackPhase() => ClearSelections();
        private void OnEnterDefensePhase() { }
        private void OnEndDefensePhase() => ClearSelections();

        public void RegisterCard(Card card)
        {
            if (card == null) return;

            if (!int.TryParse(card.cardId, out int id))
            {
                Debug.LogError($"[AttackManager] RegisterCard: cardId invalide '{card.cardId}'");
                return;
            }

            boardCardsById[id] = card;

            Collider col = card.GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider bc = card.gameObject.AddComponent<BoxCollider>();
                bc.size = Vector3.one;
            }
        }

        public bool IsP1BoardSlot(CardSlot slot)
            => slot != null && P1BoardSlots != null && P1BoardSlots.Contains(slot);

        public bool IsCardOnP1Board(Card card)
        {
            if (card == null) return false;
            CardSlot slot = card.GetComponentInParent<CardSlot>();
            return IsP1BoardSlot(slot);
        }

        public async void HandleCardClicked(Card card)
        {
            if (card == null)
            {
                Debug.LogWarning("[AttackManager] HandleCardClicked: card is NULL");
                return;
            }

            if (PhaseManager.Instance == null)
            {
                Debug.LogWarning("[AttackManager] HandleCardClicked: PhaseManager.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackManager] Click card name='{card.name}' cardId='{card.cardId}' " +
                      $"phase={PhaseManager.Instance.CurrentPhase} onP1Board={IsCardOnP1Board(card)}");

            if (PhaseManager.Instance.CurrentPhase != GamePhase.ATTACK)
            {
                Debug.LogWarning("[AttackManager] Not in ATTACK phase -> ignore click");
                return;
            }

            if (!IsCardOnP1Board(card))
            {
                Debug.LogWarning("[AttackManager] Card is NOT on P1 board -> ignore click");
                return;
            }

            string idStr = card.cardId;

            if (!int.TryParse(idStr, out int cardIdInt))
            {
                Debug.LogError($"[AttackManager] card.cardId not int! value='{idStr}'");
                return;
            }

            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[AttackManager] SignalRClient.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackManager] -> calling Hub HandleAttackPos(cardId={cardIdInt}) " +
                      $"(from string='{idStr}', type={cardIdInt.GetType().Name})");

            ToggleCard(card);

            try
            {
                await client.HandleAttackPos(cardIdInt);
                Debug.Log($"[AttackManager] Hub call HandleAttackPos DONE cardId={cardIdInt}");
            }
            catch (Exception ex)
            {
                ToggleCard(card);
                Debug.LogError($"[AttackManager] Hub call HandleAttackPos FAILED cardId={cardIdInt} ex={ex}");
            }
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

        private void ApplyAttackStateFromServer(AttackResponseDto dto)
        {
            ClearSelections();
            for (int i = 0; i < dto.AttackCardsId.Count; i++)
            {
                int cardId = dto.AttackCardsId[i];
                if (!boardCardsById.TryGetValue(cardId, out Card card) || card == null)
                    continue;

                selectedCards.Add(card);
                card.SetSelected(true);
                card.ShowAttackOrder(i + 1);
            }
        }
    }
}
