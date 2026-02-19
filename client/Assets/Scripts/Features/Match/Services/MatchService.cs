using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using System.Linq; 
using System.Text;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    /// <summary>
    /// Legacy MatchService - Keeps only complex battle/attack/defense logic for Phase 2 migration
    /// Simple events (draw, play, phase) are now handled by MatchService → MatchEvents → UI components
    /// </summary>
    public class MatchService : MonoBehaviour
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
                Debug.LogError("[MatchService] SignalRClient.Instance NULL");
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
            Debug.Log($"[MatchService] GameStarted phase={r.CurrentPhase} turn={r.TurnNumber}");
            _gameStarted = true;
  			EnsureLocalSlots();
        }

        private void HandleAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchService] HandleAttackEngage ids={string.Join(",", attackIds)}");
            AttackUI.Instance?.AttackService?.ApplyAttackStateFromServer(attackIds);
        }

        private void HandleOpponentAttackEngage(List<int> attackIds)
        {
            Debug.Log($"[MatchService] HandleOpponentAttackEngage ids={string.Join(",", attackIds)}");
            OpponentBoardUI.Instance?.OpponentBoardService?.ApplyOpponentAttackState(attackIds);
        }

        private void HandleDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchService] HandleDefenseEngage defenses={(data?.DefenseCards?.Count ?? 0)}");
            DefenseUI.Instance?.DefenseService?.ApplyDefenseStateFromServer(data);
        }

        private void HandleOpponentDefenseEngage(DefenseDataResponseDto data)
        {
            Debug.Log($"[MatchService] HandleOpponentDefenseEngage defenses={(data?.DefenseCards?.Count ?? 0)}");
            OpponentBoardUI.Instance?.OpponentBoardService?.ApplyOpponentDefenseState(data);
        }

        private void HandleBattleResolution(BattlesDataDto data, bool localIsAttacker)
        {
            if (!_gameStarted)
            {
                Debug.LogWarning("[MatchService] BattleResolution ignored: game not started");
                return;
            }

            if (data?.battles == null)
            {
                Debug.LogWarning("[MatchService] BattleResolution ignored: data.battles is NULL");
                return;
            }

            if (data.battles.Count == 0)
            {
                Debug.LogWarning("[MatchService] BattleResolution ignored: battles.Count == 0");
                return;
            }

            Debug.Log($"[MatchService] BattleResolution RECEIVED battles={data.battles.Count} localIsAttacker={localIsAttacker}");
            for (int i = 0; i < data.battles.Count; i++)
            {
                BattleDataDto b = data.battles[i];
                if (b == null)
                {
                    Debug.LogWarning($"[MatchService] Battle[{i}] = NULL");
                    continue;
                }

                Debug.Log($"[MatchService] Battle[{i}] isAgainstChamp={b.isAgainstChamp} hasAgainstChamp={(b.againstChamp != null)} hasAgainstCard={(b.againstCard != null)}");
            }

            if (_battleRoutine != null) StopCoroutine(_battleRoutine);
            _battleRoutine = StartCoroutine(ResolveBattles(data, localIsAttacker));
        }


        private IEnumerator ResolveBattles(BattlesDataDto data, bool localIsAttacker)
        {
            Debug.Log($"[MatchService] ResolveBattles START battles={data?.battles?.Count ?? -1} localIsAttacker={localIsAttacker}");

            for (int i = 0; i < data.battles.Count; i++)
            {
                BattleDataDto b = data.battles[i];
                if (b == null)
                {
                    Debug.LogWarning($"[MatchService] ResolveBattles skip Battle[{i}] NULL");
                    continue;
                }

                Debug.Log($"[MatchService] ResolveBattles Battle[{i}] BEGIN isAgainstChamp={b.isAgainstChamp}");

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
                    Debug.LogWarning($"[MatchService] ResolveBattles Battle[{i}] invalid payload (missing againstChamp/againstCard)");
                }

                LogLocalBoardState($"AFTER Battle[{i}]");
                OpponentBoardUI.Instance?.OpponentBoardService?.LogBoardState($"AFTER Battle[{i}]");
                Debug.Log($"[MatchService] ResolveBattles Battle[{i}] END");
            }

            AttackUI.Instance?.AttackService?.ClearSelections();
            DefenseUI.Instance?.DefenseService?.ClearAllDefense();

            Debug.Log("[MatchService] ResolveBattles END -> cleared selections/defense");
        }



        private IEnumerator ResolveAgainstCard(BattleAgainstCardDataDto b, bool localIsAttacker)
        {
            bool attackerIsLocal = localIsAttacker;
            bool defenderIsLocal = !localIsAttacker;

            Debug.Log("[MatchService] ResolveAgainstCard BEGIN " +
                      $"localIsAttacker={localIsAttacker} " +
                      $"attackerIsLocal={attackerIsLocal} defenderIsLocal={defenderIsLocal} " +
                      $"attacker={CardStr(b?.attackerCard)} defender={CardStr(b?.defenderCard)} " +
                      $"deadA={b?.isAttackerDead} deadD={b?.isDefenderDead}");

            ApplyCardSnapshot(b.attackerCard, attackerIsLocal);
            ApplyCardSnapshot(b.defenderCard, defenderIsLocal);

            if (b.isAttackerDead && b.attackerCard != null)
            {
                Debug.Log("[MatchService] ResolveAgainstCard -> remove ATTACKER cardId=" + b.attackerCard.GameCardId + " (ownerLocal=" + attackerIsLocal + ")");
                RemoveCard(b.attackerCard.GameCardId, attackerIsLocal);
            }

            if (b.isDefenderDead && b.defenderCard != null)
            {
                Debug.Log("[MatchService] ResolveAgainstCard -> remove DEFENDER cardId=" + b.defenderCard.GameCardId + " (ownerLocal=" + defenderIsLocal + ")");
                RemoveCard(b.defenderCard.GameCardId, defenderIsLocal);
            }

            Debug.Log("[MatchService] ResolveAgainstCard END");
            yield return new WaitForSeconds(0.25f);
        }
        private IEnumerator ResolveAgainstChamp(BattlaAgainstChampDataDto b, bool localIsAttacker)
        {
            bool attackerIsLocal = localIsAttacker;

            Debug.Log("[MatchService] ResolveAgainstChamp BEGIN " +
                      $"localIsAttacker={localIsAttacker} attackerIsLocal={attackerIsLocal} " +
                      $"attacker={CardStr(b?.attackerCard)} " +
                      $"isCardDead={b?.isCardDead} isChampDead={b?.isChampDead}");

            ApplyCardSnapshot(b.attackerCard, attackerIsLocal);

            if (b.isCardDead && b.attackerCard != null)
            {
                Debug.Log("[MatchService] ResolveAgainstChamp -> remove ATTACKER cardId=" + b.attackerCard.GameCardId + " (ownerLocal=" + attackerIsLocal + ")");
                RemoveCard(b.attackerCard.GameCardId, attackerIsLocal);
            }

            if (b.isChampDead)
                Debug.LogWarning("[MatchService] ResolveAgainstChamp -> Champion DEAD (TODO endgame UI)");

            Debug.Log("[MatchService] ResolveAgainstChamp END");
            yield return new WaitForSeconds(0.25f);
        }

       
        private void ApplyCardSnapshot(GameCardDto dto, bool isLocalOwner)
        {
            if (dto == null)
            {
                Debug.LogWarning("[MatchService] ApplyCardSnapshot dto=NULL");
                return;
            }

            Debug.Log("[MatchService] ApplyCardSnapshot -> " +
                      (isLocalOwner ? "LOCAL" : "OPPONENT") + " " + CardStr(dto));

            if (isLocalOwner)
                UpdateLocalCardSnapshot(dto);
            else
                OpponentBoardUI.Instance?.OpponentBoardService?.UpdateOpponentCardSnapshot(dto);
        }


        private void RemoveCard(int gameCardId, bool isLocalOwner)
        {
            if (gameCardId < 0) return;

            if (isLocalOwner)
                RemoveLocalCard(gameCardId);
            else
                OpponentBoardUI.Instance?.OpponentBoardService?.RemoveOpponentCard(gameCardId);
        }

        private void UpdateLocalCardSnapshot(GameCardDto dto)
        {
            if (dto == null) return;

            CardUI card = FindLocalCard(dto.GameCardId);
            if (card == null)
            {
                Debug.LogWarning("[MatchService] UpdateLocalCardSnapshot: local card NOT FOUND id=" + dto.GameCardId);
                LogLocalBoardState("LOCAL CARD NOT FOUND (debug)");
                return;
            }

            Debug.Log("[MatchService] UpdateLocalCardSnapshot APPLY -> id=" + dto.GameCardId + " to cardObj=" + card.name);

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
                Debug.LogWarning("[MatchService] RemoveLocalCard: local card NOT FOUND id=" + gameCardId);
                LogLocalBoardState("REMOVE LOCAL NOT FOUND (debug)");
                return;
            }

            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            Debug.Log("[MatchService] RemoveLocalCard -> id=" + gameCardId +
                      " cardObj=" + card.name +
                      " slot=" + (slot != null ? slot.name : "NULL"));

            if (slot != null && slot.CurrentCard == card)
                slot.CurrentCard = null;

            Destroy(card.gameObject);

            Debug.Log("[MatchService] RemoveLocalCard destroyed id=" + gameCardId);
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
   			 Debug.Log($"[MatchService] EnsureLocalSlots -> localSlots={_localSlots?.Length ?? 0}");
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
                Debug.Log("[MatchService] LocalBoardState " + label + ": _localSlots=NULL");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[MatchService] LocalBoardState " + label + ": slots=" + _localSlots.Length);

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
