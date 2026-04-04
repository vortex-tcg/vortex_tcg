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
            if (!targetAttacker.IsAttackingOutlineActive()) return;

            // Business rule: one attacker can have only one defender.
            // Refuse locally to avoid showing a fake defense effect on a second card.
            KeyValuePair<CardUI, CardUI>? existingDefenseOnTarget = defenseAssignments
                .FirstOrDefault(kvp => kvp.Value == targetAttacker);
            if (existingDefenseOnTarget.HasValue)
            {
                CardUI existingDefender = existingDefenseOnTarget.Value.Key;
                if (existingDefender != null && existingDefender != currentDefender)
                {
                    currentDefender.SetDefenseSelected(false);
                    currentDefender.SetDefendingState(false);
                    currentDefender = null;
                    Debug.LogWarning("[DefenseService] Impossible d'assigner la defense: cette carte attaquante est deja defendue par une autre carte.");
                    return;
                }
            }

            CardSlotUI defenderSlot = currentDefender.GetComponentInParent<CardSlotUI>();
            CardSlotUI attackerSlot = targetAttacker.GetComponentInParent<CardSlotUI>();
            if (defenderSlot == null || attackerSlot == null) return;

            int defenderPosition = defenderSlot.slotIndex;
            int attackerPosition = attackerSlot.slotIndex;

            SignalRClient client = SignalRClient.Instance;
            if (client == null) return;

            if (defenseAssignments.ContainsKey(currentDefender))
                defenseAssignments.Remove(currentDefender);

            defenseAssignments[currentDefender] = targetAttacker;
            currentDefender.SetDefendingState(true);

            CardUI defender = currentDefender;
            currentDefender = null;

            try
            {
                await client.ToggleDefenseCard(defenderPosition, attackerPosition);
            }
            catch (Exception)
            {
                if (defenseAssignments.ContainsKey(defender) && defenseAssignments[defender] == targetAttacker)
                {
                    defenseAssignments.Remove(defender);
                    defender.SetDefenseSelected(false);
                    defender.SetDefendingState(false);
                }
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

            bool hadAssignment = defenseAssignments.Remove(defender);
            defender.SetDefenseSelected(false);
            defender.SetDefendingState(false);

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

            // Keep local defense highlights during DEFENSE when server sends transient empty payloads.
            if (dto.DefenseCards.Count == 0 &&
                defenseAssignments.Count > 0 &&
                PhaseService.Instance != null &&
                PhaseService.Instance.CurrentPhase == GamePhase.DEFENSE)
            {
                return;
            }

            List<(CardUI defenderCard, CardUI attackerCard)> resolvedAssignments = new();

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

                if (defenderCard == null) continue;
                if (attackerCard == null) continue;

                resolvedAssignments.Add((defenderCard, attackerCard));
            }

            // Ignore malformed/incomplete payloads to avoid clearing a valid local defense highlight.
            if (dto.DefenseCards.Count > 0 && resolvedAssignments.Count != dto.DefenseCards.Count)
            {
                Debug.LogWarning("[DefenseService] Ignoring defense sync payload: no valid defender/attacker pair could be resolved.");
                return;
            }

            for (int i = 0; i < resolvedAssignments.Count; i++)
            {
                (CardUI defenderCard, CardUI attackerCard) = resolvedAssignments[i];

                defenderCard.SetDefenseSelected(true);
                defenderCard.SetDefendingState(true);
                defenseAssignments[defenderCard] = attackerCard;
            }

            currentDefender = null;
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
