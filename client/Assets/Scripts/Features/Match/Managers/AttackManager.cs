using System;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.MatchScene
{
    public class AttackManager : MonoBehaviour
    {
        public static AttackManager Instance { get; private set; }

        [Header("Player 1 Cards on Board")]
        [SerializeField] private List<CardSlotUI> P1BoardSlots = new List<CardSlotUI>();

        private readonly Dictionary<int, CardUI> boardCardsById = new Dictionary<int, CardUI>();
        private readonly List<CardUI> selectedCards = new List<CardUI>();

        private void Awake()
        {
            Instance = this;
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
                SignalRClient.Instance.OnAttackEngage += ApplyAttackStateFromServer;

            RegisterExistingCardsFromSlots();
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
                SignalRClient.Instance.OnAttackEngage -= ApplyAttackStateFromServer;
        }

        private void RegisterExistingCardsFromSlots()
        {
            if (P1BoardSlots == null) return;

            for (int i = 0; i < P1BoardSlots.Count; i++)
            {
                CardSlotUI slot = P1BoardSlots[i];
                if (slot == null) continue;
                if (slot.CurrentCard == null) continue;

                RegisterCard(slot.CurrentCard);
            }
        }

        private void OnEnterAttackPhase() => ClearSelections();
        private void OnEnterDefensePhase() { /* optionnel */ }
        private void OnEndDefensePhase() => ClearSelections();

        public void RegisterCard(CardUI card)
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

        public bool IsP1BoardSlot(CardSlotUI slot)
            => slot != null && P1BoardSlots != null && P1BoardSlots.Contains(slot);

        public bool IsCardOnP1Board(CardUI card)
        {
            if (card == null) return false;
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            return IsP1BoardSlot(slot);
        }

        public async void HandleCardClicked(CardUI card)
        {
            if (card == null)
            {
                Debug.LogWarning("[AttackManager] HandleCardClicked: card is NULL");
                return;
            }

            if (PhaseService.Instance == null)
            {
                Debug.LogWarning("[AttackManager] HandleCardClicked: PhaseService.Instance is NULL");
                return;
            }

            Debug.Log($"[AttackManager] Click card name='{card.name}' cardId='{card.cardId}' " +
                      $"phase={PhaseService.Instance.CurrentPhase} onP1Board={IsCardOnP1Board(card)}");

            if (PhaseService.Instance.CurrentPhase != GamePhase.ATTACK)
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

        private void ToggleCard(CardUI card)
        {
            if (selectedCards.Contains(card))
                DeselectCard(card);
            else
                SelectCard(card);

            UpdateAttackOrderLabels();
        }

        private void SelectCard(CardUI card)
        {
            selectedCards.Add(card);
            card.SetSelected(true);
        }

        private void DeselectCard(CardUI card)
        {
            selectedCards.Remove(card);
            card.SetSelected(false);
            card.ClearAttackOrder();
        }

        private void UpdateAttackOrderLabels()
        {
            for (int i = 0; i < selectedCards.Count; i++)
            {
                CardUI c = selectedCards[i];
                if (c == null) continue;
                c.ShowAttackOrder(i + 1);
            }
        }

        public void ClearSelections()
        {
            for (int i = 0; i < selectedCards.Count; i++)
            {
                CardUI c = selectedCards[i];
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
                CardUI card = FindOrRegisterBoardCardById(cardId);
                if (card == null) continue;

                selectedCards.Add(card);
                card.SetSelected(true);
                card.ShowAttackOrder(i + 1);
            }
        }
        
        private CardUI FindOrRegisterBoardCardById(int id)
        {
            if (boardCardsById.TryGetValue(id, out CardUI found) && found != null)
                return found;

            if (P1BoardSlots != null)
            {
                for (int i = 0; i < P1BoardSlots.Count; i++)
                {
                    CardSlotUI slot = P1BoardSlots[i];
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
