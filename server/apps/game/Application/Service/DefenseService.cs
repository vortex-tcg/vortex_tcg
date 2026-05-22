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
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;

namespace game.Application.Service;

public static class DefenseService
{
    public static async Task ToggleDefenseCardAsync(
        UserId userId,
        int defensePosition,
        int attackPosition,
        CancellationToken ct = default)
    {
        RoomManager rm = RoomManager.Instance;

        Match? match = rm.GetMatchByUserId(userId);
        if (match == null)
            throw new InvalidOperationException("Match not found.");

        Player current = match.GetCurrentPlayer();
        if (!current.UserId.Equals(userId))
            throw new InvalidOperationException("Not your turn.");

        HandleDefenseService.ToggleDefenseCard(match, userId, defensePosition, attackPosition, ct);

        IReadOnlyList<IEvent> events = match.PullEvents();

        Guid p1 = (Guid)match.Player1.UserId;
        Guid p2 = (Guid)match.Player2.UserId;

        Guid caller = (Guid)userId;
        Guid other = (caller == p1) ? p2 : p1;

        foreach (IEvent ev in events)
        {
            switch (ev.Name)
            {
                case DefenseEvent.DEFENSE_UPDATED:
                {
                    DefenseUpdatedDto d = ev.GetData<DefenseUpdatedDto>();

                    responseDTO<DefenseUpdatedDto, DefenseUpdatedDto> payload =
                        new responseDTO<DefenseUpdatedDto, DefenseUpdatedDto>
                        {
                            userId = caller,
                            opponentId = other,
                            success = true,
                            code = ResponseCode.SUCCESS_DEFENSE_UPDATED,
                            data = d,
                            opponentData = d
                        };

                    await CallManager.Instance.CallAsync(payload, ct);
                    break;
                }
            }
        }
    }
}