using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using System.Linq; 
using System.Text;

namespace VortexTCG.Scripts.MatchScene
{
    public class MatchController : MonoBehaviour
    {
        [SerializeField] private HandManager handManager;
        [SerializeField] private GraveyardManager graveyardManager;
        [SerializeField] private OpponentHandManager opponentHandManager;
        [SerializeField] private CardSlot[] _localSlots; 
        private bool _localSlotsCached;
        private SignalRClient client;
        private bool _gameStarted;
        private Coroutine _battleRoutine;


        private readonly List<DrawResultForPlayerDto> _bufferedDraws = new List<DrawResultForPlayerDto>();
        private readonly List<DrawResultForOpponentDto> _bufferedOpponentDraws = new List<DrawResultForOpponentDto>();

        private void OnEnable()
        {
            client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[MatchController] SignalRClient.Instance NULL");
                return;
            }

            if (handManager == null) handManager = HandManager.Instance;
            if (graveyardManager == null) graveyardManager = GraveyardManager.Instance;
            if (opponentHandManager == null) opponentHandManager = OpponentHandManager.Instance;

            client.OnGameStarted += HandleGameStarted;
            client.OnPhaseChanged += HandlePhaseChanged;
            client.OnCardsDrawn += HandleCardsDrawn;
            client.OnOpponentCardsDrawn += HandleOpponentCardsDrawn;
            client.OnPlayCardResult += HandlePlayCardResult;
            client.OnOpponentPlayCardResult += HandleOpponentPlayCardResult;
            client.OnStatus += HandleStatus;
            client.OnAttackEngage += HandleAttackEngage;
            client.OnOpponentAttackEngage += HandleOpponentAttackEngage;
            client.OnBattleResolution += HandleBattleResolution;
            client.OnDefenseEngage += HandleDefenseEngage;
            client.OnOpponentDefenseEngage += HandleOpponentDefenseEngage;


            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnRequestChangePhase += HandleRequestChangePhase;

            StartCoroutine(BindPhaseManagerWhenReady());
        }

        private IEnumerator BindPhaseManagerWhenReady()
        {
            while (PhaseManager.Instance == null)
                yield return null;

            Debug.Log("[MatchController] Bind OnRequestChangePhase");
            PhaseManager.Instance.OnRequestChangePhase -= HandleRequestChangePhase;
            PhaseManager.Instance.OnRequestChangePhase += HandleRequestChangePhase;
        }

        private void OnDisable()
        {
            if (client != null)
            {
                client.OnGameStarted -= HandleGameStarted;
                client.OnPhaseChanged -= HandlePhaseChanged;
                client.OnCardsDrawn -= HandleCardsDrawn;
                client.OnOpponentCardsDrawn -= HandleOpponentCardsDrawn;
                client.OnBattleResolution -= HandleBattleResolution;
                client.OnAttackEngage -= HandleAttackEngage;
                client.OnDefenseEngage -= HandleDefenseEngage;
                client.OnOpponentAttackEngage -= HandleOpponentAttackEngage;
                client.OnOpponentDefenseEngage -= HandleOpponentDefenseEngage;
                client.OnPlayCardResult -= HandlePlayCardResult;
                client.OnOpponentPlayCardResult -= HandleOpponentPlayCardResult;
                client.OnStatus -= HandleStatus;
            }

            if (PhaseManager.Instance != null)
                PhaseManager.Instance.OnRequestChangePhase -= HandleRequestChangePhase;
        }

        private void HandleStatus(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            if (handManager == null || !handManager.HasPendingPlay) return;

            if (msg.Contains("Can't play", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("play the card", StringComparison.OrdinalIgnoreCase))
            {
                handManager.CancelPendingPlay("hub error: " + msg);
            }
        }

        private void HandleGameStarted(PhaseChangeResultDTO r)
        {
            Debug.Log($"[MatchController] GameStarted phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct}");
            _gameStarted = true;
  			EnsureLocalSlots();
            handManager?.SetHand(new List<DrawnCardDto>());
            graveyardManager?.ResetGraveyard();
            opponentHandManager?.ResetHand();

            PhaseManager.Instance?.ApplyServerPhase(r.CurrentPhase);
            OpponentBoardManager.Instance?.ResetBoard();

            foreach (DrawResultForPlayerDto d in _bufferedDraws) ApplyDraw(d);
            _bufferedDraws.Clear();

            foreach (DrawResultForOpponentDto od in _bufferedOpponentDraws) ApplyOpponentDraw(od);
            _bufferedOpponentDraws.Clear();
        }

        private void HandlePhaseChanged(PhaseChangeResultDTO r)
        {
            Debug.Log($"[MatchController] PhaseChanged phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct} auto={r.AutoChanged}");
            PhaseManager.Instance?.ApplyServerPhase(r.CurrentPhase);

            if (r.AutoChanged && !string.IsNullOrWhiteSpace(r.AutoChangeReason))
                Debug.Log("[MatchController] AutoChangeReason: " + r.AutoChangeReason);
        }

        private async void HandleRequestChangePhase()
        {
            Debug.Log("[MatchController] HandleRequestChangePhase() -> calling hub ChangePhase");
            try
            {
                if (client != null && client.IsConnected)
                    await client.ChangePhase();
            }
            catch (Exception ex)
            {
                Debug.LogError("[MatchController] ChangePhase failed: " + ex);
            }
        }

        private void HandleCardsDrawn(DrawResultForPlayerDto result)
        {
            if (!_gameStarted)
            {
                _bufferedDraws.Add(result);
                return;
            }
            ApplyDraw(result);
        }

        private void ApplyDraw(DrawResultForPlayerDto result)
        {
            if (result == null) return;

            if (result.SentToGraveyard != null && result.SentToGraveyard.Count > 0)
                graveyardManager?.AddCards(result.SentToGraveyard);

            if (result.DrawnCards != null && result.DrawnCards.Count > 0)
                handManager?.AddCards(result.DrawnCards);
        }

        private void HandleOpponentCardsDrawn(DrawResultForOpponentDto result)
        {
            if (!_gameStarted)
            {
                _bufferedOpponentDraws.Add(result);
                return;
            }
            ApplyOpponentDraw(result);
        }

        private void ApplyOpponentDraw(DrawResultForOpponentDto result)
        {
            int added = result?.CardsDrawnCount ?? 0;
            int burned = result?.CardsBurnedCount ?? 0;
            int fatigue = result?.FatigueCount ?? 0;

            Debug.Log($"[MatchController] Opponent drew +{added} (burn {burned}, fatigue {fatigue})");

            if (added > 0)
                opponentHandManager?.AddFaceDownCards(added);
        }

        private void HandlePlayCardResult(PlayCardPlayerResultDto r)
        {
            if (r == null) return;

            Debug.Log($"[MatchController] PlayCardResult canPlayed={r.canPlayed} loc={r.location} gameCardId={r.PlayedCard?.GameCardId}");

            if (r.PlayedCard == null) return;
            handManager?.ConfirmPlayFromServer(r.PlayedCard.GameCardId, r.location, r.canPlayed);
        }

        private void HandleOpponentPlayCardResult(PlayCardOpponentResultDto r)
        {
            if (r == null) return;

            Debug.Log($"[MatchController] OpponentPlayCardResult loc={r.location} gameCardId={r.PlayedCard?.GameCardId}");

            opponentHandManager?.RemoveOneCardFromHand();

            if (OpponentBoardManager.Instance != null)
                OpponentBoardManager.Instance.PlaceOpponentCard(r.location, r.PlayedCard);
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

            Card card = FindLocalCard(dto.GameCardId);
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
            Card card = FindLocalCard(gameCardId);
            if (card == null)
            {
                Debug.LogWarning("[MatchController] RemoveLocalCard: local card NOT FOUND id=" + gameCardId);
                LogLocalBoardState("REMOVE LOCAL NOT FOUND (debug)");
                return;
            }

            CardSlot slot = card.GetComponentInParent<CardSlot>();
            Debug.Log("[MatchController] RemoveLocalCard -> id=" + gameCardId +
                      " cardObj=" + card.name +
                      " slot=" + (slot != null ? slot.name : "NULL"));

            if (slot != null && slot.CurrentCard == card)
                slot.CurrentCard = null;

            Destroy(card.gameObject);

            Debug.Log("[MatchController] RemoveLocalCard destroyed id=" + gameCardId);
        }


        private Card FindLocalCard(int gameCardId)
        {
            EnsureLocalSlots();
            if (_localSlots == null) return null;

            for (int i = 0; i < _localSlots.Length; i++)
            {
                CardSlot s = _localSlots[i];
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
        		_localSlots = FindObjectsOfType<CardSlot>(true)
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
                CardSlot s = _localSlots[i];
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