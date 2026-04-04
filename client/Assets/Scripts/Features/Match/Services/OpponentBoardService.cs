using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    public class OpponentBoardService
    {
        private readonly Dictionary<int, CardUI> opponentCardsById;
        private List<int> lastOpponentAttackIds;
        private DefenseDataResponseDto lastOpponentDefenseState;

        public OpponentBoardService()
        {
            this.opponentCardsById = new Dictionary<int, CardUI>();
            this.lastOpponentAttackIds = null;
            this.lastOpponentDefenseState = null;
        }

        public void RegisterOpponentCard(int id, CardUI card)
        {
            if (card == null) return;
            opponentCardsById[id] = card;
            Debug.Log("[OpponentBoardService] Registered opponent card id=" + id +
                      " dictCount=" + opponentCardsById.Count);

            // If we already received attack/defense state before this card was registered, reapply it now.
            if (lastOpponentDefenseState != null)
                ApplyOpponentDefenseState(lastOpponentDefenseState);
            else if (lastOpponentAttackIds != null)
                ApplyOpponentAttackState(lastOpponentAttackIds);
        }

        public void ApplyOpponentAttackState(List<int> attackIds)
        {
            lastOpponentAttackIds = attackIds;
            lastOpponentDefenseState = null;
            Debug.Log("[OpponentBoardService] ApplyOpponentAttackState ids=" +
                      (attackIds == null ? "NULL" : string.Join(",", attackIds)));
            Debug.Log("[OpponentBoardService] Current registered cards count: " + opponentCardsById.Count);
            Debug.Log("[OpponentBoardService] Registered IDs: " + string.Join(",", opponentCardsById.Keys));

            ClearOpponentAttackOutline();

            if (attackIds == null || attackIds.Count == 0) return;

            int found = 0;
            int missing = 0;

            for (int i = 0; i < attackIds.Count; i++)
            {
                int id = attackIds[i];

                if ((!opponentCardsById.TryGetValue(id, out CardUI card) || card == null) && OpponentBoardUI.Instance != null)
                {
                    CardUI recovered = OpponentBoardUI.Instance.FindOpponentCardByGameCardId(id)
                                       ?? OpponentBoardUI.Instance.GetCardAtSlotIndex(id);
                    if (recovered != null)
                    {
                        opponentCardsById[id] = recovered;
                        card = recovered;
                        Debug.Log("[OpponentBoardService] Recovered opponent card from board for id=" + id);
                    }
                }

                if (card != null)
                {
                    card.SetSelected(true);
                    card.SetAttackedThisPhase(true);
                    card.ShowAttackOrder(i + 1);
                    card.SetOpponentAttacking(true);
                    found++;
                }
                else
                {
                    missing++;
                    Debug.LogWarning("[OpponentBoardService] Attack card id not found on opponent board: " + id);
                }
            }

            Debug.Log("[OpponentBoardService] ApplyOpponentAttackState done found=" + found + " missing=" + missing);
        }

        public void ApplyOpponentDefenseState(DefenseDataResponseDto data)
        {
            lastOpponentDefenseState = data;

            Debug.Log("[OpponentBoardService] ApplyOpponentDefenseState defenses=" +
                      (data?.DefenseCards == null ? "NULL" : data.DefenseCards.Count.ToString()));

            // Defense updates only contain currently engaged defense pairs, not the full attacker list.
            // Keep using the last known full opponent attack list when available.
            List<int> attackIdsToDisplay =
                (lastOpponentAttackIds != null && lastOpponentAttackIds.Count > 0)
                    ? new List<int>(lastOpponentAttackIds)
                    : data?.AttackCardsId;

            if (attackIdsToDisplay != null && attackIdsToDisplay.Count > 0)
            {
                lastOpponentAttackIds = new List<int>(attackIdsToDisplay);
            }

            ClearOpponentAttackOutline();

            if (attackIdsToDisplay == null || attackIdsToDisplay.Count == 0)
                return;

            int found = 0;
            int missing = 0;

            for (int i = 0; i < attackIdsToDisplay.Count; i++)
            {
                int positionOrId = attackIdsToDisplay[i];
                CardUI card = null;

                // First try: lookup by GameCardId (if it's a real ID)
                if (opponentCardsById.TryGetValue(positionOrId, out card) && card != null)
                {
                    // Found by GameCardId - continue
                }
                else if (OpponentBoardUI.Instance != null)
                {
                    // Second try: lookup by GameCardId on board
                    CardUI recovered = OpponentBoardUI.Instance.FindOpponentCardByGameCardId(positionOrId);
                    if (recovered != null)
                    {
                        opponentCardsById[positionOrId] = recovered;
                        card = recovered;
                        Debug.Log("[OpponentBoardService] Recovered opponent defense card from board by GameCardId=" + positionOrId);
                    }
                    else
                    {
                        // Third try: lookup by slot position (from new protocol with positions)
                        recovered = OpponentBoardUI.Instance.GetCardAtSlotIndex(positionOrId);
                        if (recovered != null)
                        {
                            card = recovered;
                            // Register it by its GameCardId for future lookups
                            if (int.TryParse(recovered.cardId, out int gameCardId))
                            {
                                opponentCardsById[gameCardId] = recovered;
                            }
                            Debug.Log("[OpponentBoardService] Recovered opponent attack card from board by SlotIndex=" + positionOrId);
                        }
                    }
                }

                if (card != null)
                {
                    card.SetSelected(true);
                    card.SetAttackedThisPhase(true);
                    card.ShowAttackOrder(i + 1);
                    card.SetOpponentAttacking(true);
                    found++;
                    Debug.Log("[OpponentBoardService] (Defense) Attack OUTLINE ON for opponent card position=" + positionOrId + " name=" + card.name + " order=" + (i + 1));
                }
                else
                {
                    missing++;
                    Debug.LogWarning("[OpponentBoardService] (Defense) attack card not found on opponent board: positionOrId=" + positionOrId);
                }
            }

            Debug.Log("[OpponentBoardService] ApplyOpponentDefenseState done found=" + found + " missing=" + missing);
        }

        public void ClearOpponentAttackOutline()
        {
            Debug.Log("[OpponentBoardService] ClearOpponentAttackOutline()");

            foreach (var kvp in opponentCardsById)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetOpponentAttacking(false);
                    kvp.Value.SetSelected(false);
                    kvp.Value.ClearAttackOrder();
                    kvp.Value.ResetAttackState();
                }
            }
        }

        public void ClearCombatState()
        {
            // End turn resolution is authoritative: clear cached combat payloads so outlines are not re-applied.
            lastOpponentAttackIds = null;
            lastOpponentDefenseState = null;
            ClearOpponentAttackOutline();
        }

        public void UpdateOpponentCardSnapshot(GameCardDto dto)
        {
            if (dto == null) return;

            int id = dto.GameCardId;
            if (!opponentCardsById.TryGetValue(id, out CardUI card) || card == null)
            {
                Debug.LogWarning("[OpponentBoardService] UpdateOpponentCardSnapshot: card not found id=" + id);
                return;
            }

            card.ApplyDTO(
                dto.GameCardId.ToString(),
                dto.Name,
                dto.Hp,
                dto.Attack,
                dto.Cost,
                dto.Description,
                ""
            );

            if (lastOpponentDefenseState != null)
                ApplyOpponentDefenseState(lastOpponentDefenseState);
            else if (lastOpponentAttackIds != null)
                ApplyOpponentAttackState(lastOpponentAttackIds);
        }

        public void RemoveOpponentCard(int gameCardId)
        {
            if (!opponentCardsById.TryGetValue(gameCardId, out CardUI card) || card == null)
            {
                Debug.LogWarning("[OpponentBoardService] RemoveOpponentCard: card not found id=" + gameCardId);
                return;
            }

            opponentCardsById.Remove(gameCardId);
            Debug.Log("[OpponentBoardService] RemoveOpponentCard destroyed id=" + gameCardId +
                      " remaining=" + opponentCardsById.Count);
        }

        public void ClearBoard()
        {
            opponentCardsById.Clear();
            lastOpponentAttackIds = null;
            lastOpponentDefenseState = null;
        }

        public void LogBoardState(string label)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[OpponentBoardService] BoardState " + label + ": opponentCardsById.Count=" + opponentCardsById.Count);
            foreach (var kvp in opponentCardsById)
            {
                CardUI card = kvp.Value;
                string cardInfo = card != null ? $"'{card.name}'" : "null";
                sb.AppendLine($"  Card id={kvp.Key} {cardInfo}");
            }
            Debug.Log(sb.ToString());
        }
    }
}
