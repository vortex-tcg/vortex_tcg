using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Interface;
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

        await rm.Matchmaker.JoinQueueAsync(userId, deckId, ct);
        IReadOnlyList<IEvent> events = rm.MatchmakerEventContainer.PullEvents(ct);

        foreach (IEvent ev in events)
        {
            if (ev.Name != MatchmakerEvent.FOUND) continue;

            MatchFoundData data = ev.GetData<MatchFoundData>();
            Match match = await rm.CreateMatchAsync(data.players, ct);
            UserId p1 = data.players[0].userId;
            UserId p2 = data.players[1].userId;
            ChampionId p1ChampionId =  new ChampionId(match.Player1.Champion.Id.Value);
            ChampionId p2ChampionId = new ChampionId(match.Player2.Champion.Id.Value);
            //TODO : init match, avec toute la data 
            await CallManager.Instance.CallAsync(new responseDTO<MatchFoundSelfDto, MatchFoundOpponentDto>
            {
                userId = (Guid)p1,
                opponentId = (Guid)p2,
                success = true,
                code = ResponseCode.MATCH_FOUND,
                data = new MatchFoundSelfDto
                {
                    matchId = match.MatchId.Value,
                    championId = p1ChampionId,
                },
                opponentData = new MatchFoundOpponentDto
                {
                    opponentHandSize = 5,
                    championId = p2ChampionId,
                }
            }, ct);

            await CallManager.Instance.CallAsync(new responseDTO<MatchFoundSelfDto, MatchFoundOpponentDto>
            {
                userId = (Guid)p2,
                opponentId = (Guid)p1,
                success = true,
                code = ResponseCode.MATCH_FOUND,
                data = new MatchFoundSelfDto
                {
                    matchId = match.MatchId.Value,
                    championId =  p2ChampionId,
               },
                opponentData = new MatchFoundOpponentDto
                {
                    opponentHandSize = 6,
                    championId = p1ChampionId,
                }
            }, ct);

        }

    }

    public static Task LeaveQueueAsync(UserId userId, CancellationToken ct = default)
        => RoomManager.Instance.Matchmaker.LeaveQueueAsync(userId, ct);
}