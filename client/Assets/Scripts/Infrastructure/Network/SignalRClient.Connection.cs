using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;

public partial class SignalRClient
{
    public async Task ConnectAndIdentify(string playerName)
    {
        await EnsureConnected(playerName);
        await SafeSend("SetName", string.IsNullOrWhiteSpace(playerName) ? defaultPlayerName : playerName);
    }

    public async Task EnsureConnected(string playerName)
    {
        if (_conn != null && _conn.State == HubConnectionState.Connected) return;
        _conn = BuildConnection();
        _conn.On<string>("Connected", id => Enqueue(() =>
        {
            OnStatus?.Invoke($"Connecté: {id}");
            OnLog?.Invoke("Connecté au hub.");
        }));
        _conn.On("Waiting", () => Enqueue(() => OnStatus?.Invoke("En attente d'un adversaire...")));
        _conn.On<MatchInitUserDto>("matchFound", dto => Enqueue(() =>
        {
            string key = dto.MatchId.ToString();
            int pos = dto.Self.Position;
            Debug.Log($"[SignalRClient] matchFound key={key} pos={pos} networkRefNull={(networkRef == null)}");

            _currentKeyOrCode = key;
            _playerPosition = pos;
            _initialDrawnCards = dto.Self.DrawnCards?.ToList() ?? new List<MatchInitCardDto>();
            _opponentHandSize = dto.OpponentHandSize;
            _playerChampion = dto.Self.Champion;
            _playerGold = dto.Self.Gold;
            _secondaryCurrencyName = dto.Self.SecondaryCurrencyName;
            _playerSecondaryCurrency = dto.Self.SecondaryCurrency;
            _opponentChampion = dto.Opponent.Champion;
            _opponentGold = dto.Opponent.Gold;
            _opponentSecondaryCurrencyName = dto.Opponent.SecondaryCurrencyName;
            _opponentSecondaryCurrency = dto.Opponent.SecondaryCurrency;

            networkRef?.SetMatch(key, pos);
            OnMatched?.Invoke(key);
            OnLog?.Invoke($"Match trouvé ! Salle: {key} (pos={pos}) - Cartes initiales: {_initialDrawnCards.Count}");
            
            // Manually trigger GameStarted with initial phase since server may not send it immediately
            var initialPhaseDto = new PhaseChangeResultDTO
            {
                CurrentPhase = GamePhase.PLACEMENT,
                ActivePlayerId = Guid.Empty, // Will be set by actual GameStarted if received
                TurnNumber = 0,
                AutoChanged = false,
                AutoChangeReason = null,
                CanAct = pos == 1, // Player 1 can act first
                TimerEndTime = null
            };
            Debug.Log($"[SignalRClient] ✅ Triggering initial GameStarted - phase={initialPhaseDto.CurrentPhase}");
            OnGameStarted?.Invoke(initialPhaseDto);
            MatchEvents.FireGameStarted(initialPhaseDto);
        }));

        _conn.On<PlayCardPlayerResultDto>("PlayCardResult", dto => Enqueue(() =>
        {
            Debug.Log("[SignalRClient] PlayCardResult reçu");
            OnPlayCardResult?.Invoke(dto);
            
            // Relay to MatchEvents so HandUI can react
            Debug.Log($"[SignalRClient] ✅ Firing MatchEvents.FirePlayerCardPlayed for card location={dto?.location}");
            MatchEvents.FirePlayerCardPlayed(dto);
        }));

        _conn.On<PlayCardOpponentResultDto>("OpponentPlayCardResult", dto => Enqueue(() =>
        {
            Debug.Log("[SignalRClient] OpponentPlayCardResult reçu");
            OnOpponentPlayCardResult?.Invoke(dto);
            
            // Relay to MatchEvents so OpponentUI and OpponentBoardUI can react
            Debug.Log($"[SignalRClient] ✅ Firing MatchEvents.FireOpponentCardPlayed for card location={dto?.location}");
            MatchEvents.FireOpponentCardPlayed(dto);
        }));

        _conn.On<PhaseChangeResultDTO>("GameStarted", r => Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] ✅ GameStarted reçu - phase={r.CurrentPhase}");
            OnGameStarted?.Invoke(r);
            MatchEvents.FireGameStarted(r);
            OnLog?.Invoke($"GameStarted: phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct}");
        }));

        _conn.On<PhaseChangeResultDTO>("PhaseChanged", r => Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] ✅ PhaseChanged reçu - phase={r.CurrentPhase}");
            OnPhaseChanged?.Invoke(r);
            MatchEvents.FirePhaseChanged(r);
            OnLog?.Invoke($"PhaseChanged: phase={r.CurrentPhase} turn={r.TurnNumber} canAct={r.CanAct} auto={r.AutoChanged}");
        }));

        _conn.On<string, string, string>("ReceiveRoomMessage", (key, from, text) =>
            Enqueue(() => OnLog?.Invoke($"{from}: {text}")));

        _conn.On<string>("RoomCreated", code => Enqueue(() =>
        {
            _mode = "code";
            _currentKeyOrCode = code;
            OnLog?.Invoke($"Salle créée. Code: {code}");
            OnStatus?.Invoke($"Salle {code} créée. En attente d'un joueur...");
        }));

        _conn.On<string>("RoomCreateError", reason => Enqueue(() =>
            OnStatus?.Invoke(reason == "CODE_TAKEN" ? "Code déjà pris." : "Erreur création salle.")));

        _conn.On<string>("RoomJoinError", reason => Enqueue(() =>
            OnStatus?.Invoke(reason == "ROOM_FULL" ? "Salle pleine." : "Salle introuvable.")));

        _conn.On<string>("OpponentLeft", _ => Enqueue(() =>
        {
            OnOpponentLeft?.Invoke();
            networkRef?.ResetMatch();
            _startGameRequested = false;
            OnLog?.Invoke("L'adversaire a quitté.");
        }));

        _conn.On<BattlesDataDto>("BattleResolution_Attacker", dto =>
        {
            Debug.Log($"[SignalRClient] BattleResolution_Attacker reçu battles={(dto?.battles?.Count ?? 0)}");
            Enqueue(() => OnBattleResolution?.Invoke(dto, true));
        });

        _conn.On<BattlesDataDto>("BattleResolution_Defender", dto =>
        {
            Debug.Log($"[SignalRClient] BattleResolution_Defender reçu battles={(dto?.battles?.Count ?? 0)}");
            Enqueue(() => OnBattleResolution?.Invoke(dto, false));
        });

        _conn.On<DrawResultForPlayerDto>("CardsDrawn", r => Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] ✅✅✅ CardsDrawn reçu. cards={r?.DrawnCards?.Count ?? -1}");
            if (r?.DrawnCards != null)
            {
                foreach (var card in r.DrawnCards)
                {
                    Debug.Log($"[SignalRClient] - Card: ID={card.GameCardId}, Name='{card.Name}'");
                }
            }

            OnCardsDrawn?.Invoke(r);
            MatchEvents.FirePlayerCardsDrawn(r);
            Debug.Log($"[SignalRClient] ✅ OnCardsDrawn event invoked, subscribers={OnCardsDrawn?.GetInvocationList().Length ?? 0}");
        }));

        _conn.On<DrawResultForOpponentDto>("OpponentCardsDrawn", r => Enqueue(() =>
        {
            Debug.Log("[SignalRClient] OpponentCardsDrawn reçu - Invoking OnOpponentCardsDrawn");
            OnOpponentCardsDrawn?.Invoke(r);
            MatchEvents.FireOpponentCardsDrawn(r);
            OnLog?.Invoke($"OpponentCardsDrawn reçu: {r?.CardsDrawnCount ?? 0} cartes");
            Debug.Log("[SignalRClient] ✅ OnOpponentCardsDrawn event invoked");
        }));

        _conn.On<string>("Error", msg => Enqueue(() =>
        {
            Debug.LogError("[Hub Error] " + msg);
            OnStatus?.Invoke(msg);
        }));

        _conn.On<List<int>>("HandleAttackEngage", ids =>
        {
            Debug.Log($"[SignalRClient] HandleAttackEngage ids=[{string.Join(",", ids)}]");
            Enqueue(() => OnAttackEngage?.Invoke(ids));
        });

        _conn.On<List<int>>("HandleOpponentAttackEngage", ids =>
        {
            Debug.Log($"[SignalRClient] HandleOpponentAttackEngage ids=[{string.Join(",", ids)}]");
            Enqueue(() =>
            {
                Debug.Log($"[SignalRClient] Enqueued action executing for HandleOpponentAttackEngage. OnOpponentAttackEngage has {(OnOpponentAttackEngage?.GetInvocationList()?.Length ?? 0)} subscribers");
                OnOpponentAttackEngage?.Invoke(ids);
                Debug.Log($"[SignalRClient] OnOpponentAttackEngage.Invoke completed");
            });
        });

        _conn.On<DefenseDataResponseDto>("HandleDefenseEngage", dto =>
        {
            Debug.Log($"[SignalRClient] HandleDefenseEngage count={(dto?.DefenseCards?.Count ?? 0)}");
            Enqueue(() => OnDefenseEngage?.Invoke(dto));
        });

        _conn.On<DefenseDataResponseDto>("HandleOpponentDefenseEngage", dto =>
        {
            Debug.Log($"[SignalRClient] HandleOpponentDefenseEngage count={(dto?.DefenseCards?.Count ?? 0)}");
            Enqueue(() => OnOpponentDefenseEngage?.Invoke(dto));
        });

        try
        {
            Debug.Log("[SignalR] Connecting… " + hubUrl);
            await _conn.StartAsync();
            Debug.Log("[SignalR] Connected. ✅ All handlers registered and ready for events.");
        }
        catch (Exception ex)
        {
            Enqueue(() =>
            {
                OnStatus?.Invoke("Erreur de connexion (voir Console).");
                Debug.LogError("[SignalR] StartAsync FAILED: " + ex);
            });
        }
    }
}
