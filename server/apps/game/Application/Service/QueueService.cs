using System;
using System.Collections.Generic;
using System.Diagnostics;
using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.ValueObject;
using game.Domaine.Matchmaking;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public class QueueService
{
    public static async Task JoinQueueAsync(UserId userId, DeckId deckId, CancellationToken ct = default)
    {
        RoomManager rm = RoomManager.Instance;

        Debug.WriteLine($"Joining queue for user {userId} with deck {deckId}");

        await rm.Matchmaker.JoinQueueAsync(userId, deckId, ct);
        IReadOnlyList<IEvent> events = rm.MatchmakerEventContainer.PullEvents(ct);

        foreach (IEvent ev in events)
        {
            if (ev.Name != MatchmakerEvent.FOUND)
            {
                continue;
            }

            MatchFoundData data = ev.GetData<MatchFoundData>();
            await HandleMatchFoundAsync(rm, data, ct);
        }
    }

    private static async Task HandleMatchFoundAsync(RoomManager rm, MatchFoundData data, CancellationToken ct)
    {
        Match match = await rm.CreateMatchAsync(data.players, ct);

        match.InitMatch();

        IReadOnlyList<IEvent> matchEvents = match.PullEvents();

        MatchInitData init = matchEvents.First(me => me.Name == MatchEvent.MATCH_INIT).GetData<MatchInitData>();

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

        await CallManager.Instance.CallAsync(payload, ct);
    }

    public static Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
    {
        return RoomManager.Instance.Matchmaker.LeaveQueueAsync(userId, ct);
    }
}