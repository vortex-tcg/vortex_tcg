using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using System.Linq; 
using System.Text;
using VortexTCG.Scripts.Features.Match.Services;
using VortexTCG.Scripts.Features.Match.UI;
using VortexTCG.Scripts.MatchScene;

namespace VortexTCG.Scripts.Features.Match.Services
{
    public class MatchService : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance != null)
                return;

            MatchService existing = FindObjectOfType<MatchService>();
            if (existing != null)
            {
                Instance = existing;
                return;
            }

            GameObject go = new GameObject("MatchService");
            go.AddComponent<MatchService>();
        }

        public static MatchService Instance { get; private set; }
        
        [SerializeField] private CardSlotUI[] _localSlots; 
        private bool _localSlotsCached;
        private SignalRClient client;
        private bool _gameStarted;
        private Coroutine _battleRoutine;
        private Coroutine _endPhaseRoutine;
        private int? _lastSyncedLocalChampionHp;
        private int? _lastSyncedOpponentChampionHp;
        private bool? _pendingEndScreenLocalWon;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[MatchService] Duplicate instance detected, destroying this one");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MatchService] Singleton instance created");
        }

        private void OnEnable()
        {
            Debug.Log("[MatchService] OnEnable called");
            client = SignalRClient.Instance;
            if (client == null)
            {
                Debug.LogError("[MatchService] SignalRClient.Instance NULL");
                StartCoroutine(WaitForSignalRClient());
                return;
            }

            Debug.Log("[MatchService] Subscribing to SignalR events...");
            client.OnGameStarted += HandleGameStartedMinimal;
            client.OnOpponentAttackEngage += HandleOpponentAttackEngage;
            client.OnBattleResolution += HandleBattleResolution;
            client.OnEndPhaseResolved += HandleEndPhaseResolved;
            client.OnOpponentDefenseEngage += HandleOpponentDefenseEngage;
            Debug.Log("[MatchService] Successfully subscribed to all SignalR events including OnOpponentAttackEngage");
        }

        private IEnumerator WaitForSignalRClient()
        {
            while (client == null)
            {
                client = SignalRClient.Instance;
                yield return null;
            }

            Debug.Log("[MatchService] SignalRClient now available, subscribing...");
            Debug.Log("[MatchService] Subscribing to SignalR events...");
            client.OnGameStarted += HandleGameStartedMinimal;
            client.OnOpponentAttackEngage += HandleOpponentAttackEngage;
            client.OnBattleResolution += HandleBattleResolution;
            client.OnEndPhaseResolved += HandleEndPhaseResolved;
            client.OnOpponentDefenseEngage += HandleOpponentDefenseEngage;
            Debug.Log("[MatchService] Successfully subscribed to all SignalR events including OnOpponentAttackEngage");
        }

        private void OnDisable()
        {
            Debug.Log("[MatchService] OnDisable called");
            if (client != null)
            {
                client.OnGameStarted -= HandleGameStartedMinimal;
                client.OnBattleResolution -= HandleBattleResolution;
                client.OnEndPhaseResolved -= HandleEndPhaseResolved;
                client.OnOpponentAttackEngage -= HandleOpponentAttackEngage;
                client.OnOpponentDefenseEngage -= HandleOpponentDefenseEngage;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Debug.Log("[MatchService] Singleton instance cleared");
            }
        }

        private void HandleGameStartedMinimal(PhaseChangeResultDTO r)
        {
            Debug.Log($"[MatchService] GameStarted phase={r.CurrentPhase} turn={r.TurnNumber}");
            _gameStarted = true;
            AttackUI.Instance?.ResetBoard();
            DefenseUI.Instance?.ClearAllDefense();
            OpponentBoardUI.Instance?.ResetBoard();
            OpponentUI.Instance?.ResetBoard();
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
            
            if (OpponentBoardUI.Instance == null)
            {
                Debug.LogError("[MatchService] OpponentBoardUI.Instance is NULL!");
                return;
            }
            
            if (OpponentBoardUI.Instance.OpponentBoardService == null)
            {
                Debug.LogError("[MatchService] OpponentBoardUI.Instance.OpponentBoardService is NULL!");
                return;
            }
            
            Debug.Log($"[MatchService] Calling OpponentBoardService.ApplyOpponentAttackState with {attackIds.Count} IDs");
            OpponentBoardUI.Instance.OpponentBoardService.ApplyOpponentAttackState(attackIds);
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

        private void HandleEndPhaseResolved(EndPhaseResolutionDto data, bool localIsAttacker)
        {
            if (!_gameStarted)
            {
                Debug.LogWarning("[MatchService] EndPhaseResolved ignored: game not started");
                return;
            }

            if (data == null)
            {
                Debug.LogWarning("[MatchService] EndPhaseResolved ignored: data is NULL");
                return;
            }

            Debug.Log($"[MatchService] EndPhaseResolved received battles={data.Battles?.Count ?? 0} deadCards={data.DeadCardIds?.Count ?? 0} localIsAttacker={localIsAttacker}");

            if (_endPhaseRoutine != null)
            {
                StopCoroutine(_endPhaseRoutine);
            }

            _endPhaseRoutine = StartCoroutine(ResolveEndPhaseSequentially(data, localIsAttacker));
        }

        private IEnumerator ResolveEndPhaseSequentially(EndPhaseResolutionDto data, bool localIsAttacker)
        {
            bool attackerIsLocal = localIsAttacker;
            bool defenderIsLocal = !localIsAttacker;
            SyncChampionHpFromEndPhase(data, localIsAttacker);
            _pendingEndScreenLocalWon = ResolveOutcomeFromEndPhasePayload(data, localIsAttacker);

            if (data.Battles != null)
            {
                for (int i = 0; i < data.Battles.Count; i++)
                {
                    EndPhaseCardBattleResultDto battle = data.Battles[i];
                    if (battle == null) continue;

                    CardUI attackerCard = FindCardBySlotIndex(battle.AttackerPosition, attackerIsLocal);
                    CardUI defenderCard = FindCardBySlotIndex(battle.DefenderPosition, defenderIsLocal);

                    if (battle.DamageToAttacker > 0)
                        attackerCard?.SetDamageReceivedState(true);

                    if (battle.DamageToDefender > 0)
                        defenderCard?.SetDamageReceivedState(true);

                    if (battle.DamageToAttacker > 0 || battle.DamageToDefender > 0)
                        yield return new WaitForSeconds(0.35f);

                    ApplyCardSnapshotBySlotIndex(battle.AttackerPosition, attackerIsLocal, battle.AttackerRemainingHp);
                    ApplyCardSnapshotBySlotIndex(battle.DefenderPosition, defenderIsLocal, battle.DefenderRemainingHp);

                    yield return new WaitForSeconds(0.15f);

                    attackerCard?.SetDamageReceivedState(false);
                    defenderCard?.SetDamageReceivedState(false);

                    yield return new WaitForSeconds(0.15f);
                }
            }

            if (data.DirectChampionDamages != null)
            {
                for (int i = 0; i < data.DirectChampionDamages.Count; i++)
                {
                    EndPhaseDirectChampionDamageDto damage = data.DirectChampionDamages[i];
                    if (damage == null) continue;

                    Debug.Log($"[MatchService] Champion receives {damage.Damage} damage from attackerCardId={damage.AttackerCardId}");
                    yield return new WaitForSeconds(0.2f);
                }
            }

            if (data.DeadCardIds != null)
            {
                for (int i = 0; i < data.DeadCardIds.Count; i++)
                {
                    int deadCardId = data.DeadCardIds[i];
                    yield return RemoveCardWithDeathState(deadCardId, true);
                    yield return RemoveCardWithDeathState(deadCardId, false);
                }
            }

            AttackUI.Instance?.ResetAllAttackStates();
            OpponentBoardUI.Instance?.OpponentBoardService?.ClearCombatState();
            PhaseUI.Instance?.RefreshChampionHpDisplay();
            TryShowEndingScreen();

            Debug.Log($"[MatchService] EndPhaseResolution applied currentHp={data.CurrentPlayerChampionHp} opponentHp={data.OpponentPlayerChampionHp}");
            _endPhaseRoutine = null;
        }

        private void SyncChampionHpFromEndPhase(EndPhaseResolutionDto data, bool localIsAttacker)
        {
            if (client == null || data == null)
            {
                return;
            }

            int currentHp = Mathf.Max(0, data.CurrentPlayerChampionHp);
            int opponentHp = Mathf.Max(0, data.OpponentPlayerChampionHp);

            // In this UI, P1 slot is local champion and P2 slot is opponent champion.
            MatchInitChampionDto championP1 = client.Position1Champion;
            MatchInitChampionDto championP2 = client.Position2Champion;

            MatchInitChampionDto localChampion = client.PlayerChampion;
            MatchInitChampionDto opponentChampion = client.OpponentChampion;

            int baselineLocalHp = localChampion?.Hp ?? championP1?.Hp ?? currentHp;
            int baselineRemoteHp = opponentChampion?.Hp ?? championP2?.Hp ?? opponentHp;

            // Candidate A: payload already local/opponent.
            int localHpAsIs = currentHp;
            int remoteHpAsIs = opponentHp;

            // Candidate B: payload is attacker/defender relative and needs swap for local view.
            int localHpSwapped = opponentHp;
            int remoteHpSwapped = currentHp;

            int scoreAsIs = Mathf.Abs(baselineLocalHp - localHpAsIs) + Mathf.Abs(baselineRemoteHp - remoteHpAsIs);
            int scoreSwapped = Mathf.Abs(baselineLocalHp - localHpSwapped) + Mathf.Abs(baselineRemoteHp - remoteHpSwapped);

            bool useSwappedMapping = scoreSwapped < scoreAsIs;
            if (scoreSwapped == scoreAsIs)
            {
                // Tie-breaker: keep previous behavior hint from event direction.
                useSwappedMapping = !localIsAttacker;
            }

            int localHp = useSwappedMapping ? localHpSwapped : localHpAsIs;
            int remoteHp = useSwappedMapping ? remoteHpSwapped : remoteHpAsIs;

            if (championP1 != null)
            {
                championP1.Hp = localHp;
            }

            if (championP2 != null)
            {
                championP2.Hp = remoteHp;
            }

            if (localChampion != null)
            {
                localChampion.Hp = localHp;
            }

            if (opponentChampion != null)
            {
                opponentChampion.Hp = remoteHp;
            }

            Debug.Log($"[MatchService] SyncChampionHpFromEndPhase localIsAttacker={localIsAttacker} currentHp={currentHp} opponentHp={opponentHp} baselineLocal={baselineLocalHp} baselineRemote={baselineRemoteHp} scoreAsIs={scoreAsIs} scoreSwapped={scoreSwapped} useSwapped={useSwappedMapping} => localHp={localHp} remoteHp={remoteHp}");

            _lastSyncedLocalChampionHp = localHp;
            _lastSyncedOpponentChampionHp = remoteHp;
        }

        private static bool? ResolveOutcomeFromEndPhasePayload(EndPhaseResolutionDto data, bool localIsAttacker)
        {
            if (data == null)
            {
                return null;
            }

            if (data.DirectChampionDamages != null)
            {
                for (int i = 0; i < data.DirectChampionDamages.Count; i++)
                {
                    EndPhaseDirectChampionDamageDto damage = data.DirectChampionDamages[i];
                    if (damage == null) continue;

                    if (damage.ChampionRemainingHp <= 0)
                    {
                        // During EndPhaseResolved, localIsAttacker indicates whether local damaged opponent champion.
                        return localIsAttacker;
                    }
                }
            }

            return null;
        }

        private void TryShowEndingScreen()
        {
            if (client == null)
            {
                return;
            }

            int localPosition = client.PlayerPosition;
            MatchInitChampionDto localChampion = client.PlayerChampion;
            MatchInitChampionDto opponentChampion = client.OpponentChampion;

            if (localChampion == null || opponentChampion == null)
            {
                MatchInitChampionDto championP1 = client.Position1Champion;
                MatchInitChampionDto championP2 = client.Position2Champion;

                if (localPosition == 1)
                {
                    localChampion ??= championP1;
                    opponentChampion ??= championP2;
                }
                else if (localPosition == 2)
                {
                    localChampion ??= championP2;
                    opponentChampion ??= championP1;
                }
            }

            if (localChampion == null || opponentChampion == null)
            {
                Debug.LogWarning("[MatchService] TryShowEndingScreen aborted: champion references are missing");
                return;
            }

            if (_pendingEndScreenLocalWon.HasValue)
            {
                bool forcedLocalWon = _pendingEndScreenLocalWon.Value;
                _pendingEndScreenLocalWon = null;

                MatchEndingScreenUI forcedScreen = MatchEndingScreenUI.Instance;
                if (forcedScreen == null)
                {
                    forcedScreen = FindFirstObjectByType<MatchEndingScreenUI>(FindObjectsInactive.Include);
                }

                if (forcedScreen == null)
                {
                    Debug.LogError("[MatchService] Ending screen not found for forced end-phase outcome");
                    return;
                }

                if (!forcedScreen.gameObject.activeSelf)
                {
                    forcedScreen.gameObject.SetActive(true);
                }

                forcedScreen.ShowEndingScreen(forcedLocalWon);
                Debug.Log($"[MatchService] Ending screen shown (forced from end-phase payload) localWon={forcedLocalWon}");
                return;
            }

            int localHp = _lastSyncedLocalChampionHp ?? localChampion.Hp;
            int opponentHp = _lastSyncedOpponentChampionHp ?? opponentChampion.Hp;

            bool localLost = localHp <= 0;
            bool localWon = opponentHp <= 0 && !localLost;

            if (!localWon && !localLost)
            {
                return;
            }

            MatchEndingScreenUI endingScreen = MatchEndingScreenUI.Instance;
            if (endingScreen == null)
            {
                endingScreen = FindFirstObjectByType<MatchEndingScreenUI>(FindObjectsInactive.Include);
            }

            if (endingScreen == null)
            {
                Debug.LogError($"[MatchService] Ending screen not found. localHp={localHp} opponentHp={opponentHp}");
                return;
            }

            if (!endingScreen.gameObject.activeSelf)
            {
                endingScreen.gameObject.SetActive(true);
            }

            endingScreen.ShowEndingScreen(localWon);
            Debug.Log($"[MatchService] Ending screen shown localWon={localWon} localHp={localHp} opponentHp={opponentHp}");
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

            AttackUI.Instance?.ResetAllAttackStates();
            OpponentBoardUI.Instance?.OpponentBoardService?.ClearCombatState();
            PhaseUI.Instance?.RefreshChampionHpDisplay();
            TryShowEndingScreen();

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

            // Log damage dealt to defender
            if (b.attackerCard != null && b.defenderCard != null)
            {
                int damageDealt = b.attackerCard.Attack > 0 ? b.attackerCard.Attack : 0;
                int defenderRemainingHp = b.defenderCard.Hp;
                Debug.Log($"[MatchService] 🛡️ DAMAGE: '{b.defenderCard.Name}' receives {damageDealt} damage from '{b.attackerCard.Name}' (Remaining HP: {defenderRemainingHp})");
            }

            if (b.isAttackerDead && b.attackerCard != null)
            {
                Debug.Log("[MatchService] ResolveAgainstCard -> remove ATTACKER cardId=" + b.attackerCard.GameCardId + " (ownerLocal=" + attackerIsLocal + ")");
                yield return RemoveCardWithDeathState(b.attackerCard.GameCardId, attackerIsLocal);
            }

            if (b.isDefenderDead && b.defenderCard != null)
            {
                Debug.Log("[MatchService] ResolveAgainstCard -> remove DEFENDER cardId=" + b.defenderCard.GameCardId + " (ownerLocal=" + defenderIsLocal + ")");
                yield return RemoveCardWithDeathState(b.defenderCard.GameCardId, defenderIsLocal);
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
                yield return RemoveCardWithDeathState(b.attackerCard.GameCardId, attackerIsLocal);
            }

            if (b.isChampDead)
            {
                Debug.LogWarning("[MatchService] ResolveAgainstChamp -> Champion DEAD detected (waiting consolidated end-phase sync before showing ending screen)");
            }

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

        private void ApplyCardSnapshotBySlotIndex(int slotIndex, bool isLocalOwner, int remainingHp)
        {
            CardUI card = FindCardBySlotIndex(slotIndex, isLocalOwner);
            if (card == null)
            {
                Debug.LogWarning($"[MatchService] ApplyCardSnapshotBySlotIndex: card not found at slot={slotIndex} owner={(isLocalOwner ? "LOCAL" : "OPPONENT")}");
                return;
            }

            card.ApplyDTO(
                card.cardId,
                card.cardName,
                remainingHp,
                card.attack,
                card.cost,
                card.description,
                card.imageUrl
            );
        }

        private CardUI FindCardBySlotIndex(int slotIndex, bool isLocalOwner)
        {
            if (isLocalOwner)
            {
                EnsureLocalSlots();
                if (_localSlots != null)
                {
                    for (int i = 0; i < _localSlots.Length; i++)
                    {
                        CardSlotUI slot = _localSlots[i];
                        if (slot != null && slot.slotIndex == slotIndex)
                        {
                            return slot.CurrentCard;
                        }
                    }
                }

                return null;
            }

            return OpponentBoardUI.Instance != null ? OpponentBoardUI.Instance.GetCardAtSlotIndex(slotIndex) : null;
        }


        private void RemoveCard(int gameCardId, bool isLocalOwner)
        {
            if (gameCardId < 0) return;

            if (isLocalOwner)
                RemoveLocalCard(gameCardId);
            else
                RemoveOpponentCard(gameCardId);
        }

        private IEnumerator RemoveCardWithDeathState(int gameCardId, bool isLocalOwner)
        {
            if (gameCardId < 0)
                yield break;

            CardUI card = isLocalOwner
                ? FindLocalCard(gameCardId)
                : OpponentBoardUI.Instance?.FindOpponentCardByGameCardId(gameCardId);

            if (card == null)
            {
                RemoveCard(gameCardId, isLocalOwner);
                yield break;
            }

            card.SetDamageReceivedState(false);
            card.SetDeathState(true);
            yield return new WaitForSeconds(3f);

            RemoveCard(gameCardId, isLocalOwner);
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
                slot.SetCurrentCard(null);

            Destroy(card.gameObject);

            Debug.Log("[MatchService] RemoveLocalCard destroyed id=" + gameCardId);
        }

        private void RemoveOpponentCard(int gameCardId)
        {
            CardUI card = OpponentBoardUI.Instance?.FindOpponentCardByGameCardId(gameCardId);
            OpponentBoardUI.Instance?.OpponentBoardService?.RemoveOpponentCard(gameCardId);

            if (card == null)
            {
                Debug.LogWarning("[MatchService] RemoveOpponentCard: opponent card NOT FOUND id=" + gameCardId);
                return;
            }

            CardSlotUI slot = card.GetComponentInParent<CardSlotUI>();
            if (slot != null && slot.CurrentCard == card)
                slot.SetCurrentCard(null);

            Destroy(card.gameObject);
            Debug.Log("[MatchService] RemoveOpponentCard destroyed id=" + gameCardId);
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
        	_localSlots = FindObjectsByType<CardSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None)
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
