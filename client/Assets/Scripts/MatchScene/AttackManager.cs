using System;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.MatchScene
{
    public class AttackManager : MonoBehaviour
    {
        public static AttackManager Instance { get; private set; }

        [Header("Player 1 Cards on Board")]
        [SerializeField] private List<CardSlot> P1BoardSlots = new List<CardSlot>();

        private readonly Dictionary<int, Card> boardCardsById = new Dictionary<int, Card>();
        private readonly List<Card> selectedCards = new List<Card>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnEnterAttack += OnEnterAttackPhase;
                PhaseManager.Instance.OnEnterDefense += OnEnterDefensePhase;
                PhaseManager.Instance.OnEnterStandBy += OnEndDefensePhase;
            }
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.OnAttackEngage += ApplyAttackStateFromServer;

            RegisterExistingCardsFromSlots();
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

        private void RegisterExistingCardsFromSlots()
        {
            if (P1BoardSlots == null) return;

            for (int i = 0; i < P1BoardSlots.Count; i++)
            {
                CardSlot slot = P1BoardSlots[i];
                if (slot == null) continue;
                if (slot.CurrentCard == null) continue;

                RegisterCard(slot.CurrentCard);
            }
        }

        private void OnEnterAttackPhase() => ClearSelections();
        private void OnEnterDefensePhase() { /* optionnel */ }
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

            if (!int.TryParse(card.cardId, out int cardIdInt))
            {
                Debug.LogError($"[AttackManager] card.cardId not int! value='{card.cardId}'");
                return;
            }

            SignalRClient client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[AttackManager] SignalRClient.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackManager] -> calling Hub HandleAttackPos(cardId={cardIdInt})");
            ToggleCard(card);

            try
            {
                await client.HandleAttackPos(cardIdInt);
                Debug.Log($"[AttackManager] Hub call HandleAttackPos DONE cardId={cardIdInt}");
            }
            catch (Exception ex)
            {
                // rollback
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
                Card c = selectedCards[i];
                if (c == null) continue;
                c.ShowAttackOrder(i + 1);
            }
        }

        public void ClearSelections()
        {
            for (int i = 0; i < selectedCards.Count; i++)
            {
                Card c = selectedCards[i];
                if (c == null) continue;
                c.SetSelected(false);
                c.ClearAttackOrder();
            }
            selectedCards.Clear();
        }
        public void ApplyAttackStateFromServer(List<int> attackIds)
        {
            ClearSelections();

            if (attackIds == null)
            {
                Debug.Log("[AttackManager] HandleAttackEngage reçu: NULL");
                return;
            }

            Debug.Log($"[AttackManager] HandleAttackEngage reçu: count={attackIds.Count}");

            for (int i = 0; i < attackIds.Count; i++)
            {
                int cardId = attackIds[i];
                Card card = FindOrRegisterBoardCardById(cardId);
                if (card == null) continue;

                selectedCards.Add(card);
                card.SetSelected(true);
                card.ShowAttackOrder(i + 1);
            }
        }

        private Card FindOrRegisterBoardCardById(int id)
        {
            if (boardCardsById.TryGetValue(id, out Card found) && found != null)
                return found;

            if (P1BoardSlots != null)
            {
                for (int i = 0; i < P1BoardSlots.Count; i++)
                {
                    CardSlot slot = P1BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;

                    if (int.TryParse(slot.CurrentCard.cardId, out int cid) && cid == id)
                    {
                        RegisterCard(slot.CurrentCard);
                        return slot.CurrentCard;
                    }
                }
            }

            return null;
        }
    }
}
