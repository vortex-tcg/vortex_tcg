using Microsoft.AspNetCore.SignalR;
using game.Application.Dto;
using game.Application.Enum;
using game.Hubs;

namespace game.Infrastructure.Manager;



public sealed class CallManager : ICallManager
{
    private readonly IHubContext<GameHub> _hubContext;

    private static readonly string[][] mapCodesToSignalRCallPlayer =
    {
        new[] { nameof(ResponseCode.SUCCESS_POSE_CARTE), "successPoseCarte" },
        new[] { nameof(ResponseCode.SUCCESS_PHASE_CHANGED), "successPhaseChanged" }
    };

    private static readonly string[][] mapCodesToSignalRCallOpponent =
    {
        new[] { nameof(ResponseCode.SUCCESS_POSE_CARTE), "opponentPoseCarte" }, 
        new[] { nameof(ResponseCode.SUCCESS_PHASE_CHANGED), "opponentPhaseChanged" }
    };




    private static readonly string[][] mapCodesToMsgError =
    {
        new[] { nameof(ResponseCode.CODE_TAKEN), "Le code de salon est déjà pris." },
        new[] { nameof(ResponseCode.ROOM_FULL), "Le salon est complet." },
        new[] { nameof(ResponseCode.NOT_FOUND), "Salon introuvable." },
        new[] { nameof(ResponseCode.NOT_IN_ROOM), "Vous n'êtes pas dans un salon." },
        new[] { nameof(ResponseCode.NOT_YOUR_TURN), "Ce n'est pas votre tour." },
        new[] { nameof(ResponseCode.UNKNOWN_ERROR), "Une erreur est survenue." }
    };

    public CallManager(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }
    
    public async Task CallAsync<T>(responseDTO<T> response, CancellationToken ct = default)
    {
        if (response is null) return;

        IClientProxy? player = response.userId != Guid.Empty
            ? _hubContext.Clients.User(response.userId.ToString())
            : null;

        IClientProxy? opponent = response.opponentId != Guid.Empty
            ? _hubContext.Clients.User(response.opponentId.ToString())
            : null;

        if (!response.success)
        {
            string errorMsg = Resolve(mapCodesToMsgError, response.code);

            if (player != null)
                await player.SendAsync("Error", errorMsg, ct);

            return;
        }

        string playerCall = Resolve(mapCodesToSignalRCallPlayer, response.code);
        string opponentCall = Resolve(mapCodesToSignalRCallOpponent, response.code);

        if (player != null && !string.IsNullOrWhiteSpace(playerCall))
            await player.SendAsync(playerCall, response, ct);

        if (opponent != null && !string.IsNullOrWhiteSpace(opponentCall))
            await opponent.SendAsync(opponentCall, response, ct);
    }

    private static string Resolve(string[][] map, ResponseCode code)
    {
        string key = code.ToString();
        for (int i = 0; i < map.Length; i++)
        {
            string[] row = map[i];
            if (row.Length >= 2 && row[0] == key)
                return row[1];
        }
        return "";
    }
    
}
