using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using System.Linq; 
using System.Text;
using VortexTCG.Scripts.Features.Match.Services;

namespace VortexTCG.Scripts.MatchScene
{
    /// <summary>
    /// Legacy MatchController - Keeps only complex battle/attack/defense logic for Phase 2 migration
    /// Simple events (draw, play, phase) are now handled by MatchService → MatchEvents → UI components
    /// </summary>
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private CardSlotUI[] _localSlots; 
        private bool _localSlotsCached;
        private SignalRClient client;
        private bool _gameStarted;
        private Coroutine _battleRoutine;

        private void OnEnable()
        {
            client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[MatchController] SignalRClient.Instance NULL");
                return;
            }

            // Subscribe only to complex events not yet migrated to UI components
            client.OnGameStarted += HandleGameStartedMinimal;
            client.OnAttackEngage += HandleAttackEngage;
            client.OnOpponentAttackEngage += HandleOpponentAttackEngage;
            client.OnBattleResolution += HandleBattleResolution;
            client.OnDefenseEngage += HandleDefenseEngage;
            client.OnOpponentDefenseEngage += HandleOpponentDefenseEngage;
        }

        private void OnDisable()
        {
            if (client != null)
            {
                client.OnGameStarted -= HandleGameStartedMinimal;
                client.OnBattleResolution -= HandleBattleResolution;
                client.OnAttackEngage -= HandleAttackEngage;
                client.OnDefenseEngage -= HandleDefenseEngage;
                client.OnOpponentAttackEngage -= HandleOpponentAttackEngage;
                client.OnOpponentDefenseEngage -= HandleOpponentDefenseEngage;
            }
        }

        private void HandleGameStartedMinimal(PhaseChangeResultDTO r)
        {
            Debug.Log($"[MatchController] GameStarted phase={r.CurrentPhase} turn={r.TurnNumber}");
            _gameStarted = true;
  			EnsureLocalSlots();
        }

        private void HandleAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchController] HandleAttackEngage ids={string.Join(",", attackIds)}");
            AttackManager.Instance?.ApplyAttackStateFromServer(attackIds);
        }

        private void HandleOpponentAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchController] HandleOpponentAttackEngage ids={string.Join(",", attackIds)}");
            OpponentBoardManager.Instance?.ApplyOpponentAttackState(attackIds);
        }

        private void HandleDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchController] HandleDefenseEngage defenses={(data?.DefenseCards?.Count ?? 0)}");
            DefenseManager.Instance?.ApplyDefenseStateFromServer(data);
        }

        private void HandleOpponentDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchController] HandleOpponentDefenseEngage defenses={(data?.DefenseCards?.Count ?? 0)}");
            OpponentBoardManager.Instance?.ApplyOpponentDefenseState(data);
        }

        private void HandleBattleResolution(BattlesDataDto data, bool localIsAttacker)
        {
            if (!_gameStarted)
            {
                Debug.LogWarning("[MatchController] BattleResolution ignored: game not started");
                return;
            }

            if (data?.battles == null)
            {
                Debug.LogWarning("[MatchController] BattleResolution ignored: data.battles is NULL");
                return;
            }

            if (data.battles.Count == 0)
            {
                Debug.LogWarning("[MatchController] BattleResolution ignored: battles.Count == 0");
                return;
            }

            Debug.Log($"[MatchController] BattleResolution RECEIVED battles={data.battles.Count} localIsAttacker={localIsAttacker}");
            for (int i = 0; i < data.battles.Count; i++)
            {
                BattleDataDto b = data.battles[i];
                if (b == null)
                {
                    Debug.LogWarning($"[MatchController] Battle[{i}] = NULL");
                    continue;
                }

                Debug.Log($"[MatchController] Battle[{i}] isAgainstChamp={b.isAgainstChamp} hasAgainstChamp={(b.againstChamp != null)} hasAgainstCard={(b.againstCard != null)}");
            }

            if (_battleRoutine != null) StopCoroutine(_battleRoutine);
            _battleRoutine = StartCoroutine(ResolveBattles(data, localIsAttacker));
        }


        private IEnumerator ResolveBattles(BattlesDataDto data, bool localIsAttacker)
        {
            Debug.Log($"[MatchController] ResolveBattles START battles={data?.battles?.Count ?? -1} localIsAttacker={localIsAttacker}");

            for (int i = 0; i < data.battles.Count; i++)
            {
                BattleDataDto b = data.battles[i];
                if (b == null)
                {
                    Debug.LogWarning($"[MatchController] ResolveBattles skip Battle[{i}] NULL");
                    continue;
                }

                Debug.Log($"[MatchController] ResolveBattles Battle[{i}] BEGIN isAgainstChamp={b.isAgainstChamp}");

                if (b.isAgainstChamp && b.againstChamp != null)
                {
                    yield return ResolveAgainstChamp(b.againstChamp, localIsAttacker);
                }
                else if (!b.isAgainstChamp && b.againstCard != null)
                {
                    yield return ResolveAgainstCard(b.againstCard, localIsAttacker);
                }
                else
                {
                    Debug.LogWarning($"[MatchController] ResolveBattles Battle[{i}] invalid payload (missing againstChamp/againstCard)");
                }

                LogLocalBoardState($"AFTER Battle[{i}]");
                OpponentBoardManager.Instance?.LogBoardStatePublic($"AFTER Battle[{i}]");
                Debug.Log($"[MatchController] ResolveBattles Battle[{i}] END");
            }

            AttackManager.Instance?.ClearSelections();
            DefenseManager.Instance?.ClearAllDefense();

            Debug.Log("[MatchController] ResolveBattles END -> cleared selections/defense");
        }



        private IEnumerator ResolveAgainstCard(BattleAgainstCardDataDto b, bool localIsAttacker)
        {
            bool attackerIsLocal = localIsAttacker;
            bool defenderIsLocal = !localIsAttacker;

            Debug.Log("[MatchController] ResolveAgainstCard BEGIN " +
                      $"localIsAttacker={localIsAttacker} " +
                      $"attackerIsLocal={attackerIsLocal} defenderIsLocal={defenderIsLocal} " +
                      $"attacker={CardStr(b?.attackerCard)} defender={CardStr(b?.defenderCard)} " +
                      $"deadA={b?.isAttackerDead} deadD={b?.isDefenderDead}");

            ApplyCardSnapshot(b.attackerCard, attackerIsLocal);
            ApplyCardSnapshot(b.defenderCard, defenderIsLocal);

            if (b.isAttackerDead && b.attackerCard != null)
            {
                Debug.Log("[MatchController] ResolveAgainstCard -> remove ATTACKER cardId=" + b.attackerCard.GameCardId + " (ownerLocal=" + attackerIsLocal + ")");
                RemoveCard(b.attackerCard.GameCardId, attackerIsLocal);
            }

            if (b.isDefenderDead && b.defenderCard != null)
            {
                Debug.Log("[MatchController] ResolveAgainstCard -> remove DEFENDER cardId=" + b.defenderCard.GameCardId + " (ownerLocal=" + defenderIsLocal + ")");
                RemoveCard(b.defenderCard.GameCardId, defenderIsLocal);
            }

            Debug.Log("[MatchController] ResolveAgainstCard END");
            yield return new WaitForSeconds(0.25f);
        }
        private IEnumerator ResolveAgainstChamp(BattlaAgainstChampDataDto b, bool localIsAttacker)
        {
            bool attackerIsLocal = localIsAttacker;

            Debug.Log("[MatchController] ResolveAgainstChamp BEGIN " +
                      $"localIsAttacker={localIsAttacker} attackerIsLocal={attackerIsLocal} " +
                      $"attacker={CardStr(b?.attackerCard)} " +
                      $"isCardDead={b?.isCardDead} isChampDead={b?.isChampDead}");

            ApplyCardSnapshot(b.attackerCard, attackerIsLocal);

            if (b.isCardDead && b.attackerCard != null)
            {
                Debug.Log("[MatchController] ResolveAgainstChamp -> remove ATTACKER cardId=" + b.attackerCard.GameCardId + " (ownerLocal=" + attackerIsLocal + ")");
                RemoveCard(b.attackerCard.GameCardId, attackerIsLocal);
            }

            if (b.isChampDead)
                Debug.LogWarning("[MatchController] ResolveAgainstChamp -> Champion DEAD (TODO endgame UI)");

            Debug.Log("[MatchController] ResolveAgainstChamp END");
            yield return new WaitForSeconds(0.25f);
        }

       
        private void ApplyCardSnapshot(GameCardDto dto, bool isLocalOwner)
        {
            if (dto == null)
            {
                Debug.LogWarning("[MatchController] ApplyCardSnapshot dto=NULL");
                return;
            }

            Debug.Log("[MatchController] ApplyCardSnapshot -> " +
                      (isLocalOwner ? "LOCAL" : "OPPONENT") + " " + CardStr(dto));

            if (isLocalOwner)
                UpdateLocalCardSnapshot(dto);
            else
                OpponentBoardManager.Instance?.UpdateOpponentCardSnapshot(dto);
        }


        private void RemoveCard(int gameCardId, bool isLocalOwner)
        {
            if (gameCardId < 0) return;

            if (isLocalOwner)
                RemoveLocalCard(gameCardId);
            else
                OpponentBoardManager.Instance?.RemoveOpponentCard(gameCardId);
        }

        private void UpdateLocalCardSnapshot(GameCardDto dto)
        {
            if (dto == null) return;

            CardUI card = FindLocalCard(dto.GameCardId);
            if (card == null)
            {
                Debug.LogWarning("[MatchController] UpdateLocalCardSnapshot: local card NOT FOUND id=" + dto.GameCardId);
                LogLocalBoardState("LOCAL CARD NOT FOUND (debug)");
                return;
            }

            Debug.Log("[MatchController] UpdateLocalCardSnapshot APPLY -> id=" + dto.GameCardId + " to cardObj=" + card.name);

            card.ApplyDTO(
                dto.GameCardId.ToString(),
                dto.Name,
                dto.Hp,
                dto.Attack,
                dto.Cost,
                dto.Description,
                ""
            );
        }


        private void RemoveLocalCard(int gameCardId)
        {
            CardUI card = FindLocalCard(gameCardId);
            if (card == null)
            {
                Debug.LogWarning("[MatchController] RemoveLocalCard: local card NOT FOUND id=" + gameCardId);
                LogLocalBoardState("REMOVE LOCAL NOT FOUND (debug)");
                return;
            }

            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            Debug.Log("[MatchController] RemoveLocalCard -> id=" + gameCardId +
                      " cardObj=" + card.name +
                      " slot=" + (slot != null ? slot.name : "NULL"));

            if (slot != null && slot.CurrentCard == card)
                slot.CurrentCard = null;

            Destroy(card.gameObject);

            Debug.Log("[MatchController] RemoveLocalCard destroyed id=" + gameCardId);
        }


        private CardUI FindLocalCard(int gameCardId)
        {
            EnsureLocalSlots();
            if (_localSlots == null) return null;

            for (int i = 0; i < _localSlots.Length; i++)
            {
                CardSlotUI s = _localSlots[i];
                if (s == null || s.CurrentCard == null) continue;

                if (int.TryParse(s.CurrentCard.cardId, out int id) && id == gameCardId)
                    return s.CurrentCard;
            }

            return null;
        }

		private void EnsureLocalSlots()
		{
    		if (_localSlotsCached && _localSlots != null && _localSlots.Length > 0)
        		return;
		    if (_localSlots == null || _localSlots.Length == 0)
    		{
        		_localSlots = FindObjectsOfType<CardSlotUI>(true)
            		.Where(s => s != null && !s.isOpponentSlot)
            		.OrderBy(s => s.slotIndex)
            		.ToArray();
    		}

    		_localSlotsCached = true;
   			 Debug.Log($"[MatchController] EnsureLocalSlots -> localSlots={_localSlots?.Length ?? 0}");
		}
        private static string CardStr(GameCardDto c)
        {
            if (c == null) return "null";
            return $"id={c.GameCardId} name='{c.Name}' hp={c.Hp} atk={c.Attack} cost={c.Cost}";
        }

        private void LogLocalBoardState(string label)
        {
            EnsureLocalSlots();

            if (_localSlots == null)
            {
                Debug.Log("[MatchController] LocalBoardState " + label + ": _localSlots=NULL");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[MatchController] LocalBoardState " + label + ": slots=" + _localSlots.Length);

            for (int i = 0; i < _localSlots.Length; i++)
            {
                CardSlotUI s = _localSlots[i];
                if (s == null)
                {
                    sb.AppendLine("  [" + i + "] NULL");
                    continue;
                }

                string cc = (s.CurrentCard != null)
                    ? s.CurrentCard.cardId + "('" + s.CurrentCard.name + "')"
                    : "null";

                sb.AppendLine("  [" + i + "] slotIndex=" + s.slotIndex + " slot='" + s.name + "' CurrentCard=" + cc);
            }

            Debug.Log(sb.ToString());
        }

    }
}