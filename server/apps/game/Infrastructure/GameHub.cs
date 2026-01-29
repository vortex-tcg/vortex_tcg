// =============================================
// FICHIER: GameHub_clean.cs
// (Version "clean" : seules fonctions avec logique dans le Hub :
//  - GetAuthenticatedUserId
//  - OnConnectedAsync
//  - OnDisconnectedAsync
// Tout le reste = APPELS vers des services.
// Les fonctions supprimées :
//  - SetName
//  - SendRoomMessage
//  - SendRoomMessageByCode
// =============================================
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using game.Services;
using VortexTCG.Game.DTO;

namespace game.Hubs;

public class GameHubClean : Hub
{
    private readonly Matchmaker _matchmaker;
    private readonly RoomService _rooms;

    public GameHubClean(Matchmaker matchmaker, RoomService rooms)
    {
        _matchmaker = matchmaker;
        _rooms = rooms;
    }

    private Guid GetAuthenticatedUserId()
    {
        string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid userId = GetAuthenticatedUserId();

        _rooms.Leave(userId, out string? code, out Guid? oppUserId, out bool empty);
        if (code is not null)
        {
            if (oppUserId.HasValue && !empty)
                await Clients.OthersInGroup(code).SendAsync("OpponentLeft", code);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        }

        (string? oppId, string? _) = _matchmaker.GetOpponent(Context.ConnectionId);
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);
        if (oppId is not null)
            await Clients.Client(oppId).SendAsync("OpponentLeft", "");

        await base.OnDisconnectedAsync(exception);
    }
}
