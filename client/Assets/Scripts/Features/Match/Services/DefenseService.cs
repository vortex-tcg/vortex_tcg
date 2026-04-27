using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool IsDefenseLocked(CardUI defender)
        {
            return defender != null && defenseAssignments.ContainsKey(defender);
        }

        public void SelectDefender(CardUI defender)
        {
            if (defender == null) return;
            if (IsDefenseLocked(defender)) return;
            if (currentDefender == defender) return;

            // Keep already-assigned defenders highlighted when switching selection.
            if (currentDefender != null && !defenseAssignments.ContainsKey(currentDefender))
                currentDefender.SetDefenseSelected(false);

            currentDefender = defender;
            currentDefender.SetDefenseSelected(true);
        }

        public async Task TryAssignDefenseAndSend(CardUI targetAttacker)
        {
            if (currentDefender == null) return;
            if (targetAttacker == null) return;

            bool attackerIsInAttackMode = targetAttacker.HasAttackedThisPhase || targetAttacker.IsAttackingOutlineActive();
            if (!attackerIsInAttackMode)
            {
                Debug.LogWarning("[DefenseService] Impossible d'assigner la defense: la carte cible n'est plus en mode attaque.");
                return;
            }

            CardSlotUI defenderSlot = currentDefender.GetComponentInParent<CardSlotUI>();
            CardSlotUI attackerSlot = targetAttacker.GetComponentInParent<CardSlotUI>();
            if (defenderSlot == null || attackerSlot == null) return;

            int defenderPosition = defenderSlot.slotIndex;
            int attackerPosition = attackerSlot.slotIndex;

            SignalRClient client = SignalRClient.Instance;
            if (client == null) return;

            CardUI defender = currentDefender;
            currentDefender = null;

            // Server is authoritative for defense assignments.
            defender.SetDefenseSelected(false);

            try
            {
                await client.ToggleDefenseCard(defenderPosition, attackerPosition);
            }
            catch (Exception)
            {
                Debug.LogWarning("[DefenseService] ToggleDefenseCard failed; waiting for next authoritative sync.");
            }
        }

        public async Task RemoveDefenseAndSend(CardUI defender)
        {
            if (defender == null) return;

            CardSlotUI defenderSlot = defender.GetComponentInParent<CardSlotUI>();
            if (defenderSlot == null) return;

            int defenderPosition = defenderSlot.slotIndex;

            if (currentDefender == defender)
                currentDefender = null;

            bool hadAssignment = defenseAssignments.ContainsKey(defender);

            if (!hadAssignment)
                return;

            SignalRClient client = SignalRClient.Instance;
            if (client == null) return;

            try
            {
                await client.ToggleDefenseCard(defenderPosition, -1);
            }
            catch (Exception)
            {
                // Let the next server sync restore the authoritative state if needed.
            }
        }

        public void ApplyDefenseStateFromServer(DefenseDataResponseDto dto)
        {
            if (dto == null) return;
            if (dto.DefenseCards == null) return;

            Dictionary<int, (CardUI defenderCard, CardUI attackerCard)> uniqueAssignmentsByAttack = new();

            if (currentDefender != null)
            {
                currentDefender.SetDefenseSelected(false);
                currentDefender = null;
            }

            foreach (KeyValuePair<CardUI, CardUI> kvp in defenseAssignments)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.SetDefenseSelected(false);
                    kvp.Key.SetDefendingState(false);
                }
            }
            defenseAssignments.Clear();

            for (int i = 0; i < dto.DefenseCards.Count; i++)
            {
                DefenseCardDataDto pair = dto.DefenseCards[i];
                CardUI defenderCard = FindBoardCardById(pair.cardId);
                CardUI attackerCard = FindBoardCardByIdOrSlotIndex(pair.opponentCardId);

                if (defenderCard == null || attackerCard == null)
                {
                    Debug.LogWarning("[DefenseService] Ignoring unresolved defense pair from server payload.");
                    continue;
                }

                uniqueAssignmentsByAttack[pair.opponentCardId] = (defenderCard, attackerCard);
            }

            foreach ((CardUI defenderCard, CardUI attackerCard) in uniqueAssignmentsByAttack.Values)
            {
                defenderCard.SetDefenseSelected(true);
                defenderCard.SetDefendingState(true);
                defenseAssignments[defenderCard] = attackerCard;
            }
        }

        public void ClearAllDefense()
        {
            if (currentDefender != null)
            {
                currentDefender.SetDefenseSelected(false);
                currentDefender.SetDefendingState(false);
                currentDefender = null;
            }

            foreach (KeyValuePair<CardUI, CardUI> kvp in defenseAssignments)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.SetDefenseSelected(false);
                    kvp.Key.SetDefendingState(false);
                }
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

        private CardUI FindBoardCardByIdOrSlotIndex(int value)
        {
            CardUI byId = FindBoardCardById(value);
            if (byId != null)
                return byId;

            if (p1BoardSlots != null)
            {
                for (int i = 0; i < p1BoardSlots.Count; i++)
                {
                    CardSlotUI slot = p1BoardSlots[i];
                    if (slot == null || slot.CurrentCard == null) continue;
                    if (slot.slotIndex == value)
                        return slot.CurrentCard;
                }
            }

            if (p2BoardSlots != null)
            {
                for (int i = 0; i < p2BoardSlots.Count; i++)
                {
                    CardSlotUI slot = p2BoardSlots[i];
                    if (slot == null || slot.CurrentCard == null) continue;
                    if (slot.slotIndex == value)
                        return slot.CurrentCard;
                }
            }

            return null;
        }
    }
}
