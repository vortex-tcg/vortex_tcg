using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Logique métier pure pour la gestion de la défense
    /// N'a pas de dépendance UI
    /// </summary>
    public class DefenseService
    {
        private readonly Dictionary<int, CardUI> boardCardsById;
        private readonly Dictionary<CardUI, CardUI> defenseAssignments;
        private readonly List<CardSlotUI> p1BoardSlots;
        private readonly List<CardSlotUI> p2BoardSlots;
        private CardUI currentDefender;

        public DefenseService(List<CardSlotUI> p1BoardSlots, List<CardSlotUI> p2BoardSlots)
        {
            this.p1BoardSlots = p1BoardSlots;
            this.p2BoardSlots = p2BoardSlots;
            this.boardCardsById = new Dictionary<int, CardUI>();
            this.defenseAssignments = new Dictionary<CardUI, CardUI>();
            this.currentDefender = null;
        }

        public void RegisterCard(CardUI card)
        {
            if (card == null) return;
            if (!int.TryParse(card.cardId, out int id)) return;
            boardCardsById[id] = card;
        }

        public bool IsP1BoardSlot(CardSlotUI slot) => slot != null && p1BoardSlots != null && p1BoardSlots.Contains(slot);

        public bool IsP2BoardSlot(CardSlotUI slot) => slot != null && p2BoardSlots != null && p2BoardSlots.Contains(slot);

        public bool IsCardOnP1Board(CardUI card)
        {
            if (card == null) return false;
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            return IsP1BoardSlot(slot);
        }

        public bool IsCardOnP2Board(CardUI card)
        {
            if (card == null) return false;
            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            return IsP2BoardSlot(slot);
        }

        public void SelectDefender(CardUI defender)
        {
            if (defender == null) return;
            if (currentDefender == defender) return;

            if (currentDefender != null)
                currentDefender.SetDefenseSelected(false);

            currentDefender = defender;
            currentDefender.SetDefenseSelected(true);
        }

        public async Task TryAssignDefenseAndSend(CardUI targetAttacker)
        {
            if (currentDefender == null) return;
            if (targetAttacker == null) return;
            if (!targetAttacker.IsAttackingOutlineActive()) return;

            if (!int.TryParse(currentDefender.cardId, out int defenderId)) return;
            if (!int.TryParse(targetAttacker.cardId, out int attackerId)) return;

            SignalRClient client = SignalRClient.Instance;
            if (client == null) return;

            if (defenseAssignments.ContainsKey(currentDefender))
                defenseAssignments.Remove(currentDefender);

            defenseAssignments[currentDefender] = targetAttacker;

            try
            {
                await client.HandleDefensePos(defenderId, attackerId);
            }
            catch (Exception)
            {
                if (defenseAssignments.ContainsKey(currentDefender) && defenseAssignments[currentDefender] == targetAttacker)
                    defenseAssignments.Remove(currentDefender);
            }
        }

        public void ApplyDefenseStateFromServer(DefenseDataResponseDto dto)
        {
            ClearAllDefense();

            if (dto == null) return;
            if (dto.DefenseCards == null) return;

            for (int i = 0; i < dto.DefenseCards.Count; i++)
            {
                DefenseCardDataDto pair = dto.DefenseCards[i];
                CardUI defenderCard = FindBoardCardById(pair.cardId);
                CardUI attackerCard = FindBoardCardById(pair.opponentCardId);

                if (defenderCard == null) continue;
                if (attackerCard == null) continue;

                defenderCard.SetDefenseSelected(true);
                defenseAssignments[defenderCard] = attackerCard;
            }

            currentDefender = null;
        }

        public void ClearAllDefense()
        {
            if (currentDefender != null)
            {
                currentDefender.SetDefenseSelected(false);
                currentDefender = null;
            }

            foreach (KeyValuePair<CardUI, CardUI> kvp in defenseAssignments)
            {
                if (kvp.Key != null)
                    kvp.Key.SetDefenseSelected(false);
            }

            defenseAssignments.Clear();
        }

        public void RegisterExistingCardsFromSlots()
        {
            if (p1BoardSlots != null)
            {
                for (int i = 0; i < p1BoardSlots.Count; i++)
                {
                    CardSlotUI slot = p1BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;
                    RegisterCard(slot.CurrentCard);
                }
            }

            if (p2BoardSlots != null)
            {
                for (int i = 0; i < p2BoardSlots.Count; i++)
                {
                    CardSlotUI slot = p2BoardSlots[i];
                    if (slot == null) continue;
                    if (slot.CurrentCard == null) continue;
                    RegisterCard(slot.CurrentCard);
                }
            }
        }

        private CardUI FindBoardCardById(int id)
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

            if (p2BoardSlots != null)
            {
                for (int i = 0; i < p2BoardSlots.Count; i++)
                {
                    CardSlotUI slot = p2BoardSlots[i];
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
