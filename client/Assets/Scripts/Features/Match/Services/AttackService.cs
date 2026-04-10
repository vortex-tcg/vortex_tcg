using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    public class AttackService
    {
        private readonly Dictionary<int, CardUI> boardCardsById;
        private readonly List<CardUI> selectedCards;
        private readonly List<CardSlotUI> p1BoardSlots;

        public AttackService(List<CardSlotUI> p1BoardSlots)
        {
            this.p1BoardSlots = p1BoardSlots;
            this.boardCardsById = new Dictionary<int, CardUI>();
            this.selectedCards = new List<CardUI>();
        }

        public void RegisterCard(CardUI card)
        {
            if (card == null) return;

            if (!int.TryParse(card.cardId, out int id))
            {
                Debug.LogError($"[AttackService] RegisterCard: cardId invalide '{card.cardId}'");
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
            => slot != null && p1BoardSlots != null && p1BoardSlots.Contains(slot);

        public bool IsCardOnP1Board(CardUI card)
        {
            if (card == null) return false;
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            return IsP1BoardSlot(slot);
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

        public void ResetAllCardAttackStates()
        {
            // Reset attack state for all cards on the board
            foreach (CardUI card in boardCardsById.Values)
            {
                if (card != null)
                {
                    card.ResetAttackState();
                }
            }
        }

        public void ApplyAttackStateFromServer(List<int> attackIds)
        {
            ClearSelections();

            if (attackIds == null)
            {
                Debug.Log("[AttackService] HandleAttackEngage reçu: NULL");
                return;
            }

            Debug.Log($"[AttackService] HandleAttackEngage reçu: count={attackIds.Count}");

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

        public void RegisterExistingCardsFromSlots()
        {
            if (p1BoardSlots == null) return;

            for (int i = 0; i < p1BoardSlots.Count; i++)
            {
                CardSlotUI slot = p1BoardSlots[i];
                if (slot == null) continue;
                if (slot.CurrentCard == null) continue;

                RegisterCard(slot.CurrentCard);
            }
        }

        public async Task<bool> ToggleCardAttackOnServer(SignalRClient client, int attackPosition, CardUI card)
        {
            if (client == null)
            {
                Debug.LogError("[AttackService] ToggleCardAttackOnServer: SignalRClient is NULL");
                return false;
            }

            if (card == null)
            {
                Debug.LogError("[AttackService] ToggleCardAttackOnServer: card is NULL");
                return false;
            }

            if (!int.TryParse(card.cardId, out int gameCardId))
            {
                Debug.LogError($"[AttackService] ToggleCardAttackOnServer: invalid cardId '{card.cardId}'");
                return false;
            }

            bool sent = await client.ToggleAttackCardCompat(attackPosition, gameCardId);
            if (!sent)
            {
                Debug.LogError($"[AttackService] Failed to send attack toggle for position={attackPosition} gameCardId={gameCardId}");
            }

            return sent;
        }

        private CardUI FindOrRegisterBoardCardById(int id)
        {
            if (boardCardsById.TryGetValue(id, out CardUI found) && found != null)
                return found;

            if (p1BoardSlots != null)
            {
                for (int i = 0; i < p1BoardSlots.Count; i++)
                {
                    CardSlotUI slot = p1BoardSlots[i];
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
