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
            Debug.Log("[OpponentBoardService] Registered opponent card id=" + id + id +
                      " dictCount=" + opponentCardsById.Count);
        }

        public void ApplyOpponentAttackState(List<int> attackIds)
        {
            lastOpponentAttackIds = attackIds;
            lastOpponentDefenseState = null;
            Debug.Log("[OpponentBoardService] ApplyOpponentAttackState ids=" +
                      (attackIds == null ? "NULL" : string.Join(",", attackIds)));

            ClearOpponentAttackOutline();

            if (attackIds == null || attackIds.Count == 0) return;

            int found = 0;
            int missing = 0;

            for (int i = 0; i < attackIds.Count; i++)
            {
                int id = attackIds[i];

                if (opponentCardsById.TryGetValue(id, out CardUI card) && card != null)
                {
                    card.SetOpponentAttacking(true);
                    found++;
                    Debug.Log("[OpponentBoardService] Attack OUTLINE ON for opponent card id=" + id + " name=" + card.name);
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

            ClearOpponentAttackOutline();

            if (data == null || data.AttackCardsId == null || data.AttackCardsId.Count == 0)
                return;

            int found = 0;
            int missing = 0;

            for (int i = 0; i < data.AttackCardsId.Count; i++)
            {
                int id = data.AttackCardsId[i];

                if (opponentCardsById.TryGetValue(id, out CardUI card) && card != null)
                {
                    card.SetOpponentAttacking(true);
                    found++;
                    Debug.Log("[OpponentBoardService] (Defense) Attack OUTLINE ON for opponent card id=" + id + " name=" + card.name);
                }
                else
                {
                    missing++;
                    Debug.LogWarning("[OpponentBoardService] (Defense) attack card id not found on opponent board: " + id);
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
                    kvp.Value.SetOpponentAttacking(false);
            }
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
