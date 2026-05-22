using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using VortexTCG.Scripts.DTOs;
using VortexTCG.Scripts.Features.Match.Events;
using VortexTCG.Scripts.MatchScene;

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
            Debug.Log($"[SignalRClient] matchFound - Initial drawn cards count: {_initialDrawnCards.Count}");
            for (int i = 0; i < _initialDrawnCards.Count; i++)
            {
                Debug.Log($"[SignalRClient]   Card {i}: GameCardId={_initialDrawnCards[i].GameCardId}, Name={_initialDrawnCards[i].Name}");
            }
            _opponentHandSize = dto.OpponentHandSize;
            _playerChampion = dto.Self.Champion;
            _playerGold = dto.Self.Gold;
            _secondaryCurrencyName = dto.Self.SecondaryCurrencyName;
            _playerSecondaryCurrency = dto.Self.SecondaryCurrency;
            _opponentChampion = dto.Opponent.Champion;
            _opponentGold = dto.Opponent.Gold;
            _opponentSecondaryCurrencyName = dto.Opponent.SecondaryCurrencyName;
            _opponentSecondaryCurrency = dto.Opponent.SecondaryCurrency;
            _position1Champion = null;
            _position2Champion = null;

            if (dto.Self.Position == 1)
                _position1Champion = dto.Self.Champion;
            else if (dto.Self.Position == 2)
                _position2Champion = dto.Self.Champion;

            if (dto.Opponent.Position == 1)
                _position1Champion = dto.Opponent.Champion;
            else if (dto.Opponent.Position == 2)
                _position2Champion = dto.Opponent.Champion;

            networkRef?.SetMatch(key, pos);
            OnMatched?.Invoke(key);
            OnLog?.Invoke($"Match trouvé ! Salle: {key} (pos={pos}) - Cartes initiales: {_initialDrawnCards.Count}");
            
            // Manually trigger GameStarted with initial phase since server may not send it immediately
            PhaseChangeResultDTO initialPhaseDto = new PhaseChangeResultDTO
            {
                CurrentPhase = GamePhase.STAND_BY,
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

        // New backend contract (CallManager mappings)
        _conn.On<JsonElement>("successPoseCarte", payload => HandleSuccessPoseCarte(payload));
        _conn.On<JsonElement>("opponentPoseCarte", payload => HandleOpponentPoseCarte(payload));
        _conn.On<JsonElement>("successMatchEnded", payload => HandleMatchEnded("successMatchEnded", payload));
        _conn.On<JsonElement>("opponentMatchEnded", payload => HandleMatchEnded("opponentMatchEnded", payload));

        _conn.On<JsonElement>("GameStarted", payload => HandlePhaseEvent("GameStarted", payload, true));
        _conn.On<JsonElement>("PhaseChanged", payload => HandlePhaseEvent("PhaseChanged", payload, false));
        _conn.On<JsonElement>("successPhaseChanged", payload => HandlePhaseEvent("successPhaseChanged", payload, false));
        _conn.On<JsonElement>("opponentPhaseChanged", payload => HandlePhaseEvent("opponentPhaseChanged", payload, false));
        // In the current server flow, the caller who advances phase into EndTurn is the attacking side.
        _conn.On<JsonElement>("successEndPhaseResolved", payload => HandleEndPhaseResolved("successEndPhaseResolved", payload, true));
        _conn.On<JsonElement>("opponentEndPhaseResolved", payload => HandleEndPhaseResolved("opponentEndPhaseResolved", payload, false));
        _conn.On<JsonElement>("successAttackOrderUpdated", payload => HandleAttackOrderUpdated("successAttackOrderUpdated", payload, false));
        _conn.On<JsonElement>("opponentAttackOrderUpdated", payload => HandleAttackOrderUpdated("opponentAttackOrderUpdated", payload, true));
        _conn.On<JsonElement>("successDefenseUpdated", payload => HandleDefenseUpdated("successDefenseUpdated", payload));

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
            string localUserId = ResolveLocalUserIdFromJwt();
            if (!string.IsNullOrWhiteSpace(localUserId) &&
                !string.IsNullOrWhiteSpace(r?.PlayerId) &&
                !string.Equals(localUserId, r.PlayerId, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[SignalRClient] Ignoring CardsDrawn for non-local player. local={localUserId} payload={r.PlayerId}");
                return;
            }

            if (r?.DrawnCards != null)
            {
                foreach (DrawnCardDto card in r.DrawnCards)
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
            Debug.LogError($"[SignalR] StartAsync FAILED — url={hubUrl} type={ex.GetType().Name} msg={ex.Message}");
            if (ex.InnerException != null)
                Debug.LogError($"[SignalR]  InnerException: {ex.InnerException.GetType().Name} — {ex.InnerException.Message}");
            Enqueue(() => OnStatus?.Invoke("Erreur de connexion (voir Console)."));
        }
    }

    private void HandlePhaseEvent(string eventName, JsonElement payload, bool isGameStarted)
    {
        PhaseChangeResultDTO result = ParsePhaseChangePayload(payload);
        ApplyStandByGoldState(payload);

        Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] ✅ {eventName} reçu - phase={result.CurrentPhase} turn={result.TurnNumber} canAct={result.CanAct}");

            if (result.DrawnCard != null)
            {
                string playerId = result.ActivePlayerId != Guid.Empty
                    ? result.ActivePlayerId.ToString()
                    : ResolveLocalUserIdFromJwt();

                DrawResultForPlayerDto drawResult = new DrawResultForPlayerDto
                {
                    PlayerId = playerId,
                    DrawnCards = new List<DrawnCardDto> { result.DrawnCard }
                };

                OnCardsDrawn?.Invoke(drawResult);
                MatchEvents.FirePlayerCardsDrawn(drawResult);
                Debug.Log($"[SignalRClient] ✅ {eventName} contained drawnCard -> relayed to OnCardsDrawn (gameCardId={result.DrawnCard.GameCardId})");
            }

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

    private void ApplyStandByGoldState(JsonElement payload)
    {
        if (!TryReadInt(payload, "playerGold", out int playerGold) ||
            !TryReadInt(payload, "opponentGold", out int opponentGold))
        {
            return;
        }

        Guid currentPlayerId = ReadGuid(payload, "currentPlayerUserId");
        if (currentPlayerId == Guid.Empty)
        {
            return;
        }

        string localUserId = ResolveLocalUserIdFromJwt();
        if (!Guid.TryParse(localUserId, out Guid localPlayerId))
        {
            return;
        }

        if (localPlayerId == currentPlayerId)
        {
            _playerGold = playerGold;
            _opponentGold = opponentGold;
        }
        else
        {
            _playerGold = opponentGold;
            _opponentGold = playerGold;
        }
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

    private void HandleAttackOrderUpdated(string eventName, JsonElement payload, bool isOpponentEvent)
    {
        List<int> orderedAttackCardIds = ParseAttackOrderCardIds(payload);

        Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] {eventName} received cards=[{string.Join(",", orderedAttackCardIds)}] opponentEvent={isOpponentEvent}");

            if (isOpponentEvent)
            {
                OnOpponentAttackEngage?.Invoke(orderedAttackCardIds);
            }
            else
            {
                OnAttackEngage?.Invoke(orderedAttackCardIds);
            }
        });
    }

    private void HandleDefenseUpdated(string eventName, JsonElement payload)
    {
        DefenseDataResponseDto result = ParseDefenseUpdatedPayload(payload);

        Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] {eventName} received defenseCards={(result?.DefenseCards?.Count ?? 0)} attackCards={(result?.AttackCardsId?.Count ?? 0)}");
            OnDefenseEngage?.Invoke(result);
            OnOpponentDefenseEngage?.Invoke(result);
        });
    }

    private static List<int> ParseAttackOrderCardIds(JsonElement payload)
    {
        List<int> ids = new List<int>();

        if (!TryGetPropertyInsensitive(payload, "engagedCards", out JsonElement engagedCards) ||
            engagedCards.ValueKind != JsonValueKind.Array)
        {
            // Legacy contract fallback: attackCardsId = [int]
            if (TryGetPropertyInsensitive(payload, "attackCardsId", out JsonElement attackCardsId) &&
                attackCardsId.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement idEl in attackCardsId.EnumerateArray())
                {
                    if (idEl.TryGetInt32(out int legacyId))
                        ids.Add(legacyId);
                }
            }

            return ids;
        }

        List<(int attackOrder, int gameCardId)> entries = new List<(int attackOrder, int gameCardId)>();
        foreach (JsonElement engaged in engagedCards.EnumerateArray())
        {
            if (!TryGetPropertyInsensitive(engaged, "gameCardId", out JsonElement gameCardIdEl) ||
                !gameCardIdEl.TryGetInt32(out int gameCardId))
            {
                continue;
            }

            int attackOrder = int.MaxValue;
            if (TryGetPropertyInsensitive(engaged, "attackOrder", out JsonElement attackOrderEl) &&
                attackOrderEl.TryGetInt32(out int parsedOrder))
            {
                attackOrder = parsedOrder;
            }

            entries.Add((attackOrder, gameCardId));
        }

        foreach ((int attackOrder, int gameCardId) item in entries.OrderBy(e => e.attackOrder).ThenBy(e => e.gameCardId))
        {
            ids.Add(item.gameCardId);
        }

        return ids;
    }

    private static DefenseDataResponseDto ParseDefenseUpdatedPayload(JsonElement payload)
    {
        DefenseDataResponseDto dto = new DefenseDataResponseDto();

        if (TryGetPropertyInsensitive(payload, "engagedCards", out JsonElement engagedCards) &&
            engagedCards.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement engaged in engagedCards.EnumerateArray())
            {
                int defenderGameCardId = -1;
                int attackPosition = -1;

                if (TryGetPropertyInsensitive(engaged, "gameCardId", out JsonElement cardIdEl) &&
                    cardIdEl.TryGetInt32(out int parsedCardId))
                {
                    defenderGameCardId = parsedCardId;
                }

                if (TryGetPropertyInsensitive(engaged, "positionOpponentCard", out JsonElement oppPosEl) &&
                    oppPosEl.TryGetInt32(out int parsedOppPos))
                {
                    attackPosition = parsedOppPos;
                }

                if (defenderGameCardId >= 0 && attackPosition >= 0)
                {
                    dto.DefenseCards.Add(new DefenseCardDataDto
                    {
                        cardId = defenderGameCardId,
                        opponentCardId = attackPosition
                    });
                    
                    // Try to resolve attacker's GameCardId from its position
                    int attackerGameCardId = attackPosition; // default: assume position is ID for legacy compat
                    if (OpponentBoardUI.Instance != null)
                    {
                        CardUI attackCard = OpponentBoardUI.Instance.GetCardAtSlotIndex(attackPosition);
                        if (attackCard != null && int.TryParse(attackCard.cardId, out int resolvedId))
                        {
                            attackerGameCardId = resolvedId;
                            Debug.Log("[ParseDefenseUpdatedPayload] Resolved attacker position " + attackPosition + " to GameCardId " + resolvedId);
                        }
                    }
                    
                    dto.AttackCardsId.Add(attackerGameCardId);
                }
            }

            return dto;
        }

        if (TryGetPropertyInsensitive(payload, "defenseCards", out JsonElement legacyDefenseCards) &&
            legacyDefenseCards.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement pair in legacyDefenseCards.EnumerateArray())
            {
                if (!TryGetPropertyInsensitive(pair, "cardId", out JsonElement cardIdEl) ||
                    !cardIdEl.TryGetInt32(out int defenderGameCardId))
                {
                    continue;
                }

                if (!TryGetPropertyInsensitive(pair, "opponentCardId", out JsonElement oppPosEl) ||
                    !oppPosEl.TryGetInt32(out int attackPosition))
                {
                    continue;
                }

                dto.DefenseCards.Add(new DefenseCardDataDto
                {
                    cardId = defenderGameCardId,
                    opponentCardId = attackPosition
                });
                dto.AttackCardsId.Add(attackPosition);
            }
        }

        return dto;
    }

    private static bool TryGetPropertyInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        value = default;

        if (element.ValueKind != JsonValueKind.Object)
            return false;

        if (element.TryGetProperty(propertyName, out value))
            return true;

        foreach (JsonProperty prop in element.EnumerateObject())
        {
            if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }

        return false;
    }

    private void HandleSuccessPoseCarte(JsonElement payload)
    {
        PlayCardSignalDto signal = ParsePlayCardSignalPayload(payload);

        Enqueue(() =>
        {
            _playerGold = signal.Self?.Gold ?? _playerGold;
            _opponentGold = signal.Opponent?.Gold ?? _opponentGold;

            int location = signal.Self?.Position ?? -1;
            int gameCardId = signal.Self?.GameCardId ?? -1;

            PlayCardPlayerResultDto result = new PlayCardPlayerResultDto
            {
                location = location,
                canPlayed = true,
                PlayedCard = gameCardId > 0 ? new GameCardDto { GameCardId = gameCardId } : null,
                Champion = new PlayCardChampionDto
                {
                    Gold = _playerGold
                }
            };

            Debug.Log($"[SignalRClient] ✅ successPoseCarte reçu - gameCardId={gameCardId} location={location} playerGold={_playerGold} opponentGold={_opponentGold}");
            OnPlayCardResult?.Invoke(result);
            MatchEvents.FirePlayerCardPlayed(result);
        });
    }

    private void HandleOpponentPoseCarte(JsonElement payload)
    {
        PlayCardSignalDto signal = ParsePlayCardSignalPayload(payload);

        Enqueue(() =>
        {
            _playerGold = signal.Self?.Gold ?? _playerGold;
            _opponentGold = signal.Opponent?.Gold ?? _opponentGold;

            MatchInitCardDto card = signal.Opponent?.Card;
            int location = signal.Self?.Position ?? -1;

            PlayCardOpponentResultDto result = new PlayCardOpponentResultDto
            {
                location = location,
                PlayedCard = card == null
                    ? null
                    : new GameCardDto
                    {
                        GameCardId = card.GameCardId,
                        Name = card.Name,
                        Hp = card.Hp,
                        Attack = card.Attack,
                        Cost = card.Cost,
                        Description = card.Description,
                        CardType = (CardType)card.CardType
                    },
                Champion = new PlayCardChampionDto
                {
                    Gold = _opponentGold
                }
            };

            Debug.Log($"[SignalRClient] ✅ opponentPoseCarte reçu - location={location} cardId={result.PlayedCard?.GameCardId} name={result.PlayedCard?.Name}");
            OnOpponentPlayCardResult?.Invoke(result);
            MatchEvents.FireOpponentCardPlayed(result);
        });
    }

    private void HandleMatchEnded(string eventName, JsonElement payload)
    {
        MatchEndedDataDto result = ParseMatchEndedPayload(payload);

        Enqueue(() =>
        {
            Debug.Log($"[SignalRClient] ✅ {eventName} reçu - matchId={result.MatchId} winner={result.WinnerUserId} loser={result.LoserUserId} reason={result.Reason}");
            OnMatchEnded?.Invoke(result);
            OnOpponentLeft?.Invoke();
            OnLog?.Invoke($"{eventName}: reason={result.Reason}");
        });
    }

    private static PlayCardSignalDto ParsePlayCardSignalPayload(JsonElement payload)
    {
        try
        {
            return JsonSerializer.Deserialize<PlayCardSignalDto>(payload.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new PlayCardSignalDto();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SignalRClient] Failed to parse play card payload: {ex.Message}");
            return new PlayCardSignalDto();
        }
    }

    private static MatchEndedDataDto ParseMatchEndedPayload(JsonElement payload)
    {
        MatchEndedDataDto dto = new MatchEndedDataDto();

        if (TryGetPropertyInsensitive(payload, "matchId", out JsonElement matchIdEl) &&
            Guid.TryParse(matchIdEl.ToString(), out Guid matchId))
        {
            dto.MatchId = matchId;
        }

        if (TryGetPropertyInsensitive(payload, "winnerUserId", out JsonElement winnerEl) &&
            Guid.TryParse(winnerEl.ToString(), out Guid winnerUserId))
        {
            dto.WinnerUserId = winnerUserId;
        }

        if (TryGetPropertyInsensitive(payload, "loserUserId", out JsonElement loserEl) &&
            Guid.TryParse(loserEl.ToString(), out Guid loserUserId))
        {
            dto.LoserUserId = loserUserId;
        }

        if (TryGetPropertyInsensitive(payload, "reason", out JsonElement reasonEl))
        {
            dto.Reason = reasonEl.GetString() ?? string.Empty;
        }

        return dto;
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
            TimerEndTime = ReadNullableLong(payload, "timerEndTime"),
            DrawnCard = ReadDrawnCard(payload)
        };

        if (result.ActivePlayerId == Guid.Empty)
        {
            result.ActivePlayerId = ReadGuid(payload, "currentPlayerUserId");
        }

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

    private static DrawnCardDto ReadDrawnCard(JsonElement payload)
    {
        if (!TryReadProperty(payload, "drawnCard", out JsonElement drawnCardElement) ||
            drawnCardElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DrawnCardDto>(drawnCardElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SignalRClient] Failed to parse drawnCard from phase payload: {ex.Message}");
            return null;
        }
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

    private static string ResolveLocalUserIdFromJwt()
    {
        if (Jwt.I == null)
            return null;

        string[] claimKeys =
        {
            "id",
            "userId",
            "sub",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        };

        for (int i = 0; i < claimKeys.Length; i++)
        {
            if (Jwt.I.TryGetClaim(claimKeys[i], out string value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private static GamePhase ReadGamePhase(JsonElement element)
    {
        if (!TryReadProperty(element, "currentPhase", out JsonElement currentPhase) && !TryReadProperty(element, "phase", out currentPhase))
        {
            return GamePhase.STAND_BY;
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

        return GamePhase.STAND_BY;
    }

    private static bool TryParseGamePhase(string rawPhase, out GamePhase phase)
    {
        phase = GamePhase.STAND_BY;

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
            phase = GamePhase.STAND_BY;
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
