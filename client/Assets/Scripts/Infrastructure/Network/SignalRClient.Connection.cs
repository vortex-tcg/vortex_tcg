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

        _conn.On<JsonElement>("GameStarted", payload => HandlePhaseEvent("GameStarted", payload, true));
        _conn.On<JsonElement>("PhaseChanged", payload => HandlePhaseEvent("PhaseChanged", payload, false));
        _conn.On<JsonElement>("successPhaseChanged", payload => HandlePhaseEvent("successPhaseChanged", payload, false));
        _conn.On<JsonElement>("opponentPhaseChanged", payload => HandlePhaseEvent("opponentPhaseChanged", payload, false));
        _conn.On<JsonElement>("successEndPhaseResolved", payload => HandleEndPhaseResolved("successEndPhaseResolved", payload, false));
        _conn.On<JsonElement>("opponentEndPhaseResolved", payload => HandleEndPhaseResolved("opponentEndPhaseResolved", payload, true));

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

    private void HandlePhaseEvent(string eventName, JsonElement payload, bool isGameStarted)
    {
        PhaseChangeResultDTO result = ParsePhaseChangePayload(payload);

        Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] ✅ {eventName} reçu - phase={result.CurrentPhase} turn={result.TurnNumber} canAct={result.CanAct}");

            if (isGameStarted)
            {
                OnGameStarted?.Invoke(result);
                MatchEvents.FireGameStarted(result);
            }
            else
            {
                OnPhaseChanged?.Invoke(result);
                MatchEvents.FirePhaseChanged(result);
            }

            OnLog?.Invoke($"{eventName}: phase={result.CurrentPhase} turn={result.TurnNumber} canAct={result.CanAct} auto={result.AutoChanged}");
        });
    }

    private void HandleEndPhaseResolved(string eventName, JsonElement payload, bool localIsAttacker)
    {
        EndPhaseResolutionDto result = ParseEndPhaseResolutionPayload(payload);

        Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] ✅ {eventName} reçu - battles={result.Battles?.Count ?? 0} deadCards={result.DeadCardIds?.Count ?? 0}");
            OnEndPhaseResolved?.Invoke(result, localIsAttacker);
            OnLog?.Invoke($"{eventName}: battles={result.Battles?.Count ?? 0} deadCards={result.DeadCardIds?.Count ?? 0} p1Hp={result.CurrentPlayerChampionHp} p2Hp={result.OpponentPlayerChampionHp}");
        });
    }

    private PhaseChangeResultDTO ParsePhaseChangePayload(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return new PhaseChangeResultDTO();
        }

        if (TryGetPropertyIgnoreCase(payload, "activePlayerResult", out JsonElement activePlayerResult))
        {
            return ParsePhaseChangeResult(activePlayerResult);
        }

        return ParsePhaseChangeResult(payload);
    }

    private PhaseChangeResultDTO ParsePhaseChangeResult(JsonElement payload)
    {
        PhaseChangeResultDTO result = new PhaseChangeResultDTO
        {
            CurrentPhase = ReadGamePhase(payload),
            ActivePlayerId = ReadGuid(payload, "activePlayerId"),
            TurnNumber = ReadInt(payload, "turnNumber"),
            AutoChanged = ReadBool(payload, "autoChanged"),
            AutoChangeReason = ReadString(payload, "autoChangeReason"),
            TimerEndTime = ReadNullableLong(payload, "timerEndTime")
        };

        if (TryReadBool(payload, "canAct", out bool canAct))
        {
            result.CanAct = canAct;
        }
        else if (TryReadInt(payload, "currentPlayerPosition", out int currentPlayerPosition))
        {
            _lastServerCurrentPlayerPosition = currentPlayerPosition;
            int localPlayerPosition = LocalPlayerPosition;
            result.CanAct = localPlayerPosition > 0 && currentPlayerPosition == localPlayerPosition;
        }

        return result;
    }

    private int LocalPlayerPosition => networkRef != null ? networkRef.PlayerPosition : _playerPosition;
    private int _lastServerCurrentPlayerPosition = -1;

    private static EndPhaseResolutionDto ParseEndPhaseResolutionPayload(JsonElement payload)
    {
        try
        {
            return JsonSerializer.Deserialize<EndPhaseResolutionDto>(payload.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new EndPhaseResolutionDto();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SignalRClient] Failed to parse end phase resolution payload: {ex.Message}");
            return new EndPhaseResolutionDto();
        }
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static bool TryReadProperty(JsonElement element, string propertyName, out JsonElement value)
        => TryGetPropertyIgnoreCase(element, propertyName, out value);

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (TryReadProperty(element, propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String)
        {
            return value.GetString();
        }

        return null;
    }

    private static bool ReadBool(JsonElement element, string propertyName)
    {
        return TryReadBool(element, propertyName, out bool value) && value;
    }

    private static bool TryReadBool(JsonElement element, string propertyName, out bool value)
    {
        if (!TryReadProperty(element, propertyName, out JsonElement property))
        {
            value = default;
            return false;
        }

        if (property.ValueKind == JsonValueKind.True)
        {
            value = true;
            return true;
        }

        if (property.ValueKind == JsonValueKind.False)
        {
            value = false;
            return true;
        }

        if (property.ValueKind == JsonValueKind.String && bool.TryParse(property.GetString(), out bool parsedBool))
        {
            value = parsedBool;
            return true;
        }

        value = default;
        return false;
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        return TryReadInt(element, propertyName, out int value) ? value : 0;
    }

    private static bool TryReadInt(JsonElement element, string propertyName, out int value)
    {
        if (TryReadProperty(element, propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
            {
                return true;
            }

            if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }

    private static long? ReadNullableLong(JsonElement element, string propertyName)
    {
        if (!TryReadProperty(element, propertyName, out JsonElement property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Null || property.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out long value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out long parsedValue))
        {
            return parsedValue;
        }

        return null;
    }

    private static Guid ReadGuid(JsonElement element, string propertyName)
    {
        if (!TryReadProperty(element, propertyName, out JsonElement property))
        {
            return Guid.Empty;
        }

        if (property.ValueKind == JsonValueKind.String && Guid.TryParse(property.GetString(), out Guid value))
        {
            return value;
        }

        return Guid.Empty;
    }

    private static GamePhase ReadGamePhase(JsonElement element)
    {
        if (!TryReadProperty(element, "currentPhase", out JsonElement currentPhase) && !TryReadProperty(element, "phase", out currentPhase))
        {
            return GamePhase.PLACEMENT;
        }

        if (currentPhase.ValueKind == JsonValueKind.Number && currentPhase.TryGetInt32(out int numericPhase) && Enum.IsDefined(typeof(GamePhase), numericPhase))
        {
            return (GamePhase)numericPhase;
        }

        if (currentPhase.ValueKind == JsonValueKind.String)
        {
            string rawPhase = currentPhase.GetString();
            if (TryParseGamePhase(rawPhase, out GamePhase parsedPhase))
            {
                return parsedPhase;
            }
        }

        return GamePhase.PLACEMENT;
    }

    private static bool TryParseGamePhase(string rawPhase, out GamePhase phase)
    {
        phase = GamePhase.PLACEMENT;

        if (string.IsNullOrWhiteSpace(rawPhase))
        {
            return false;
        }

        string normalizedPhase = rawPhase.Trim();

        if (Enum.TryParse(normalizedPhase, true, out phase))
        {
            return true;
        }

        string loweredPhase = normalizedPhase.ToLowerInvariant();
        if (loweredPhase == "standby" || loweredPhase == "stand_by" || loweredPhase == "placement")
        {
            phase = GamePhase.PLACEMENT;
            return true;
        }

        if (loweredPhase == "attack")
        {
            phase = GamePhase.ATTACK;
            return true;
        }

        if (loweredPhase == "defense")
        {
            phase = GamePhase.DEFENSE;
            return true;
        }

        if (loweredPhase == "endturn" || loweredPhase == "end_turn" || loweredPhase == "end turn")
        {
            phase = GamePhase.END_TURN;
            return true;
        }

        return false;
    }
}
