using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using game.Application.Dto;
using game.Application.Enum;
namespace game.Infrastructure.Manager;

public sealed class CallManager : ICallManager
{
    private static readonly Lazy<CallManager> _instance =
        new(() => new CallManager(null!));

    public static CallManager Instance => _instance.Value;

    private readonly IHubContext<GameHubClean> _hubContext;
    private ILogger? _logger;
    private static readonly string[][] mapCodesToSignalRCallPlayer =
    {
        new[] { nameof(ResponseCode.SUCCESS_POSE_CARTE), "successPoseCarte" },
        new[] { nameof(ResponseCode.SUCCESS_PHASE_CHANGED), "successPhaseChanged" },
        new[] { nameof(ResponseCode.SUCCESS_STANDBY_STARTED), "successPhaseChanged" }, 
        new[] { nameof(ResponseCode.MATCH_FOUND), "matchFound" },
        new[] { nameof(ResponseCode.SUCCESS_ATTACK_ORDER_UPDATED), "successAttackOrderUpdated" },
        new[] { nameof(ResponseCode.SUCCESS_DEFENSE_UPDATED), "successDefenseUpdated" },
        new[] { nameof(ResponseCode.SUCCESS_END_PHASE_RESOLVED), "successEndPhaseResolved" },
        new[] { nameof(ResponseCode.SUCCESS_MATCH_ENDED), "successMatchEnded" },
    };

    private static readonly string[][] mapCodesToSignalRCallOpponent =
    {
        new[] { nameof(ResponseCode.SUCCESS_POSE_CARTE), "opponentPoseCarte" },
        new[] { nameof(ResponseCode.SUCCESS_PHASE_CHANGED), "opponentPhaseChanged" },
        new[] { nameof(ResponseCode.SUCCESS_STANDBY_STARTED), "opponentPhaseChanged" }, 
        new[] { nameof(ResponseCode.MATCH_FOUND), "matchFound" },
        new[] { nameof(ResponseCode.SUCCESS_ATTACK_ORDER_UPDATED), "opponentAttackOrderUpdated" },
        new[] { nameof(ResponseCode.SUCCESS_DEFENSE_UPDATED), "successDefenseUpdated" },
        new[] { nameof(ResponseCode.SUCCESS_END_PHASE_RESOLVED), "opponentEndPhaseResolved" },
        new[] { nameof(ResponseCode.SUCCESS_MATCH_ENDED), "opponentMatchEnded" },

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

    public static void Configure(IHubContext<GameHubClean> hubContext)
    {
        _instance.Value._setHubContext(hubContext);
    }

    public static void SetLogger(ILogger logger)
    {
        _instance.Value._logger = logger;
    }

    private void _setHubContext(IHubContext<GameHubClean> hubContext)
    {
        typeof(CallManager)
            .GetField("_hubContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(this, hubContext);
    }

    private CallManager(IHubContext<GameHubClean> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task CallAsync<TSelf, TOpponent>(
        responseDTO<TSelf, TOpponent> response,
        CancellationToken ct = default)
    {
        if (response is null) return;

        if (_hubContext == null)
            throw new InvalidOperationException("CallManager is not configured. Call CallManager.Configure(...) at startup.");

        _logger?.LogDebug("[CALL] CallAsync — code={Code} userId={UserId} opponentId={OpponentId} success={Success}", response.code, response.userId, response.opponentId, response.success);

        IClientProxy? player = response.userId != Guid.Empty
            ? _hubContext.Clients.User(response.userId.ToString())
            : null;

        IClientProxy? opponent = response.opponentId != Guid.Empty
            ? _hubContext.Clients.User(response.opponentId.ToString())
            : null;

        if (player == null)
            _logger?.LogWarning("[CALL] Player proxy null — userId={UserId}", response.userId);
        if (opponent == null && response.opponentId != Guid.Empty)
            _logger?.LogWarning("[CALL] Opponent proxy null — opponentId={OpponentId}", response.opponentId);

        if (!response.success)
        {
            string errorMsg = Resolve(mapCodesToMsgError, response.code);
            _logger?.LogWarning("[CALL] Erreur envoyée au joueur — userId={UserId} code={Code} msg={Msg}", response.userId, response.code, errorMsg);
            if (player != null)
                await player.SendAsync("Error", errorMsg, ct);
            return;
        }

        string playerCall = Resolve(mapCodesToSignalRCallPlayer, response.code);
        string opponentCall = Resolve(mapCodesToSignalRCallOpponent, response.code);

        if (player != null && !string.IsNullOrWhiteSpace(playerCall))
        {
            _logger?.LogDebug("[CALL] SendAsync joueur — event={Event} userId={UserId}", playerCall, response.userId);
            await player.SendAsync(playerCall, response.data, ct);
        }
        if (opponent != null && !string.IsNullOrWhiteSpace(opponentCall))
        {
            _logger?.LogDebug("[CALL] SendAsync adversaire — event={Event} opponentId={OpponentId}", opponentCall, response.opponentId);
            await opponent.SendAsync(opponentCall, response.opponentData, ct);
        }
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