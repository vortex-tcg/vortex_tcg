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
            List<int> previousAttackIds = lastOpponentAttackIds != null
                ? new List<int>(lastOpponentAttackIds)
                : new List<int>();
            lastOpponentAttackIds = attackIds != null ? new List<int>(attackIds) : null;
            Debug.Log("[OpponentBoardService] ApplyOpponentAttackState ids=" +
                      (attackIds == null ? "NULL" : string.Join(",", attackIds)));
            Debug.Log("[OpponentBoardService] Current registered cards count: " + opponentCardsById.Count);
            Debug.Log("[OpponentBoardService] Registered IDs: " + string.Join(",", opponentCardsById.Keys));

            if (attackIds == null || attackIds.Count == 0)
            {
                for (int i = 0; i < previousAttackIds.Count; i++)
                {
                    if (opponentCardsById.TryGetValue(previousAttackIds[i], out CardUI previousCard) && previousCard != null)
                        ClearAttackVisualState(previousCard);
                }
                return;
            }

            HashSet<int> nextAttackIds = new HashSet<int>(attackIds);

            for (int i = 0; i < previousAttackIds.Count; i++)
            {
                int previousId = previousAttackIds[i];
                if (nextAttackIds.Contains(previousId))
                    continue;

                if (opponentCardsById.TryGetValue(previousId, out CardUI previousCard) && previousCard != null)
                {
                    ClearAttackVisualState(previousCard);
                }
            }

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
                    ApplyAttackVisualState(card, i + 1);
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
            Debug.Log("[OpponentBoardService] ApplyOpponentDefenseState defenses=" +
                      (data?.DefenseCards == null ? "NULL" : data.DefenseCards.Count.ToString()));

            DefenseDataResponseDto previousDefenseState = lastOpponentDefenseState;
            lastOpponentDefenseState = data;

            if (previousDefenseState?.DefenseCards != null)
            {
                for (int i = 0; i < previousDefenseState.DefenseCards.Count; i++)
                {
                    DefenseCardDataDto previousDefense = previousDefenseState.DefenseCards[i];
                    if (previousDefense == null)
                        continue;

                    CardUI previousDefender = null;
                    if (!opponentCardsById.TryGetValue(previousDefense.cardId, out previousDefender) || previousDefender == null)
                    {
                        previousDefender = OpponentBoardUI.Instance != null
                            ? OpponentBoardUI.Instance.FindOpponentCardByGameCardId(previousDefense.cardId)
                            : null;
                    }

                    if (previousDefender != null)
                    {
                        previousDefender.SetDefenseSelected(false);
                        previousDefender.SetDefendingState(false);
                    }
                }
            }

            if (data?.DefenseCards == null || data.DefenseCards.Count == 0)
                return;

            int found = 0;
            int missing = 0;

            for (int i = 0; i < data.DefenseCards.Count; i++)
            {
                DefenseCardDataDto defense = data.DefenseCards[i];
                if (defense == null)
                    continue;

                CardUI defenderCard = null;
                if (!opponentCardsById.TryGetValue(defense.cardId, out defenderCard) || defenderCard == null)
                {
                    defenderCard = OpponentBoardUI.Instance != null
                        ? OpponentBoardUI.Instance.FindOpponentCardByGameCardId(defense.cardId)
                        : null;
                }

                if (defenderCard == null)
                {
                    missing++;
                    Debug.LogWarning("[OpponentBoardService] (Defense) defender card not found on opponent board: cardId=" + defense.cardId);
                    continue;
                }

                ApplyDefenseVisualState(defenderCard);
                found++;
            }

            Debug.Log("[OpponentBoardService] ApplyOpponentDefenseState done found=" + found + " missing=" + missing);
        }

        public void ClearOpponentAttackOutline()
        {
            Debug.Log("[OpponentBoardService] ClearOpponentAttackOutline()");

            foreach (KeyValuePair<int, CardUI> kvp in opponentCardsById)
            {
                if (kvp.Value != null)
                {
                    ClearAttackVisualState(kvp.Value);
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
            foreach (KeyValuePair<int, CardUI> kvp in opponentCardsById)
            {
                CardUI card = kvp.Value;
                string cardInfo = card != null ? $"'{card.name}'" : "null";
                sb.AppendLine($"  Card id={kvp.Key} {cardInfo}");
            }
            Debug.Log(sb.ToString());
        }

        private void ApplyAttackVisualState(CardUI card, int attackOrder)
        {
            if (card == null)
                return;

            card.SetSelected(true);
            card.SetAttackedThisPhase(true);
            card.ShowAttackOrder(attackOrder);
            card.SetOpponentAttacking(true);
        }

        private void ClearAttackVisualState(CardUI card)
        {
            if (card == null)
                return;

            card.SetOpponentAttacking(false);
            card.SetSelected(false);
            card.ClearAttackOrder();
            card.ResetAttackState();
        }

        private void ApplyDefenseVisualState(CardUI card)
        {
            if (card == null)
                return;

            card.SetSelected(false);
            card.ClearAttackOrder();
            card.ResetAttackState();
            card.SetDefenseSelected(true);
            card.SetDefendingState(true);
        }
    }
}
