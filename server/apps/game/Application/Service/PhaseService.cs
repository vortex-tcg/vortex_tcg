using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using game.Application.Dto;
using game.Application.Enum;
using game.Domaine.Interface;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.DTO;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Event.Action;
using game.Domaine.Match.Interface;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public static class PhaseService
{
    public static async Task ChangePhaseAsync(UserId userId, CancellationToken ct = default)
    {
        RoomManager rm = RoomManager.Instance;

        Match? match = rm.GetMatchByUserId(userId);
        if (match == null)
            throw new InvalidOperationException("Match not found.");

        Player current = match.GetCurrentPlayer();
        if (!current.UserId.Equals(userId))
            throw new InvalidOperationException("Not your turn.");

        match.NextPhase(ct);

        IReadOnlyList<IEvent> events = match.PullEvents();
        if (events.Count == 0) return;

        Guid p1 = (Guid)match.Player1.UserId;
        Guid p2 = (Guid)match.Player2.UserId;

        Guid caller = (Guid)userId;
        Guid other  = (caller == p1) ? p2 : p1;

        foreach (IEvent ev in events)
        {
            switch (ev.Name)
            {
                case MatchEvent.PHASE_CHANGED:
                {
                    DomainEvent.PhaseChangedData d = ev.GetData<DomainEvent.PhaseChangedData>();

                    responseDTO<PhaseChangedDto, PhaseChangedDto> payload = new responseDTO<PhaseChangedDto, PhaseChangedDto>
                    {
                        userId = caller,
                        opponentId = other,
                        success = true,
                        code = ResponseCode.SUCCESS_PHASE_CHANGED,

                        data = new PhaseChangedDto
                        {
                            matchId = d.MatchId,
                            currentPlayerPosition = d.CurrentPlayerPosition,
                            phase = d.Phase
                        },

                        opponentData = new PhaseChangedDto
                        {
                            matchId = d.MatchId,
                            currentPlayerPosition = d.CurrentPlayerPosition,
                            phase = d.Phase
                        }
                    };

                    await CallManager.Instance.CallAsync(payload, ct);
                    break;
                }

                case PhaseEvent.STANDBY_STARTED:
                {
                    StandByPhaseData d = ev.GetData<StandByPhaseData>();
                    StandByPhaseData opponentView = new StandByPhaseData(
                        matchId: d.MatchId,
                        currentPlayerUserId: d.CurrentPlayerUserId,
                        playerGold: d.PlayerGold,
                        opponentGold: d.OpponentGold,
                        playerHandCount: d.PlayerHandCount,
                        opponentHandCount: d.OpponentHandCount,
                        drawnCard: null
                    );
                    

                    responseDTO<StandByPhaseData, StandByPhaseData> payload = new responseDTO<StandByPhaseData, StandByPhaseData>
                    {
                        userId = caller,
                        opponentId = other,
                        success = true,
                        code = ResponseCode.SUCCESS_STANDBY_STARTED,

                        data = d,
                        opponentData = opponentView
                    };

                    await CallManager.Instance.CallAsync(payload, ct);
                    break;
                }
            }
        }
    }
}