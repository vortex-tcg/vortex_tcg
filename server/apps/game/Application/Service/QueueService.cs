using System;
using System.Collections.Generic;
using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Infrastructure.Manager;
using Microsoft.Extensions.Logging;

namespace game.Application.Service;

public class QueueService
{
    private static ILogger? _logger;

    public static void SetLogger(ILogger logger) => _logger = logger;

    public static async Task JoinQueueAsync(UserId userId, DeckId deckId, CancellationToken ct = default)
    {
        RoomManager rm = RoomManager.Instance;

        _logger?.LogInformation("[QUEUE] JoinQueueAsync — userId={UserId} deckId={DeckId}", userId, deckId);

        try
        {
            await rm.Matchmaker.JoinQueueAsync(userId, deckId, ct);

            _logger?.LogDebug("[QUEUE] Matchmaker appelé — récupération des events...");

            IReadOnlyList<IEvent> events = rm.MatchmakerEventContainer.PullEvents(ct);

            _logger?.LogDebug("[QUEUE] PullEvents retourné — {EventCount} event(s)", events.Count);

            foreach (IEvent ev in events)
            {
                if (ev.Name != MatchmakerEvent.FOUND)
                {
                    _logger?.LogDebug("[QUEUE] Event ignoré: {EventName}", ev.Name);
                    continue;
                }

                _logger?.LogInformation("[QUEUE] Event MATCH_FOUND reçu — traitement du match...");

                MatchFoundData data = ev.GetData<MatchFoundData>();
                await HandleMatchFoundAsync(rm, data, ct);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[QUEUE] ERREUR dans JoinQueueAsync — userId={UserId}", userId);
            throw;
        }
    }

    private static async Task HandleMatchFoundAsync(RoomManager rm, MatchFoundData data, CancellationToken ct)
    {
        _logger?.LogInformation("[QUEUE] HandleMatchFound — création du match: {P1} vs {P2}", data.players[0].userId, data.players[1].userId);

        Match match;
        try
        {
            match = await rm.CreateMatchAsync(data.players, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[QUEUE] ERREUR CreateMatchAsync — {P1} vs {P2}", data.players[0].userId, data.players[1].userId);
            throw;
        }

        _logger?.LogInformation("[QUEUE] Match créé — matchId={MatchId}", match.MatchId);

        _logger?.LogDebug("[QUEUE] Initialisation du match...");
        match.InitMatch();
        _logger?.LogDebug("[QUEUE] Match initialisé");

        IReadOnlyList<IEvent> matchEvents = match.PullEvents();
        _logger?.LogDebug("[QUEUE] Events match: {EventCount} event(s)", matchEvents.Count);

        MatchInitData init;
        try
        {
            init = matchEvents.First(me => me.Name == MatchEvent.MATCH_INIT).GetData<MatchInitData>();
            _logger?.LogDebug("[QUEUE] MatchInitData OK — p1Cards={P1Cards} p2Cards={P2Cards}", init.Player1DrawnCards.Count, init.Player2DrawnCards.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[QUEUE] ERREUR: event MATCH_INIT introuvable parmi les {EventCount} events du match", matchEvents.Count);
            throw;
        }

        UserId p1 = data.players[0].userId;
        UserId p2 = data.players[1].userId;

        int p1Gold = match.Player1.Champion.Gold.Value;
        int p2Gold = match.Player2.Champion.Gold.Value;

        int p1Secondary = match.Player1.Champion.SecondaryCurrency.Value;
        int p2Secondary = match.Player2.Champion.SecondaryCurrency.Value;

        string secondaryName = match.Player1.Champion.SecondaryCurrencyName.Value;

        List<MatchInitCardDto> p1Cards = new List<MatchInitCardDto>(init.Player1DrawnCards.Count);
        foreach (GameCardDto c in init.Player1DrawnCards)
        {
            p1Cards.Add(MatchInitDtoMapper.ToCardDto(c));
        }

        List<MatchInitCardDto> p2Cards = new List<MatchInitCardDto>(init.Player2DrawnCards.Count);
        foreach (GameCardDto c in init.Player2DrawnCards)
        {
            p2Cards.Add(MatchInitDtoMapper.ToCardDto(c));
        }

        MatchInitChampionDto p1Champ = MatchInitDtoMapper.ToChampionDto(match.Player1.Champion);
        MatchInitChampionDto p2Champ = MatchInitDtoMapper.ToChampionDto(match.Player2.Champion);

        responseDTO<MatchInitUserDto, MatchInitUserDto> payload =
            new responseDTO<MatchInitUserDto, MatchInitUserDto>
            {
                userId = (Guid)p1,
                opponentId = (Guid)p2,
                success = true,
                code = ResponseCode.MATCH_FOUND,

                data = new MatchInitUserDto
                {
                    matchId = init.MatchId,
                    opponentHandSize = init.Player2DrawnCards.Count,

                    self = new MatchInitSideDto
                    {
                        position = init.Player1Position,
                        champion = p1Champ,
                        gold = p1Gold,
                        secondaryCurrencyName = secondaryName,
                        secondaryCurrency = p1Secondary,
                        drawnCards = p1Cards
                    },

                    opponent = new MatchInitSideDto
                    {
                        position = init.Player2Position,
                        champion = p2Champ,
                        gold = p2Gold,
                        secondaryCurrencyName = secondaryName,
                        secondaryCurrency = p2Secondary,
                        drawnCards = Array.Empty<MatchInitCardDto>()
                    }
                },

                opponentData = new MatchInitUserDto
                {
                    matchId = init.MatchId,
                    opponentHandSize = init.Player1DrawnCards.Count,

                    self = new MatchInitSideDto
                    {
                        position = init.Player2Position,
                        champion = p2Champ,
                        gold = p2Gold,
                        secondaryCurrencyName = secondaryName,
                        secondaryCurrency = p2Secondary,
                        drawnCards = p2Cards
                    },

                    opponent = new MatchInitSideDto
                    {
                        position = init.Player1Position,
                        champion = p1Champ,
                        gold = p1Gold,
                        secondaryCurrencyName = secondaryName,
                        secondaryCurrency = p1Secondary,
                        drawnCards = Array.Empty<MatchInitCardDto>()
                    }
                }
            };

        _logger?.LogInformation("[QUEUE] Envoi payload matchFound — p1={P1} p2={P2} matchId={MatchId}", p1, p2, init.MatchId);
        try
        {
            await CallManager.Instance.CallAsync(payload, ct);
            _logger?.LogInformation("[QUEUE] Payload matchFound envoyé avec succès aux deux joueurs");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "[QUEUE] ERREUR CallAsync — matchId={MatchId} p1={P1} p2={P2}", init.MatchId, p1, p2);
            throw;
        }
    }

    public static Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
    {
        _logger?.LogInformation("[QUEUE] LeaveQueueAsync — userId={UserId}", userId);
        return RoomManager.Instance.Matchmaker.LeaveQueueAsync(userId, ct);
    }
}