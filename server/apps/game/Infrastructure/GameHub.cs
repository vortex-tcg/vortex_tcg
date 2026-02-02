using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using game.Application.Service;
using game.Domaine.Match.ValueObject;

namespace game.Infrastructure;
public class GameHubClean : Hub
{
    
    private UserId GetAuthenticatedUserId()
    {
        string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User not authenticated");
        return new UserId(Guid.Parse(userIdClaim));
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public Task JoinQueue(DeckId deckId)
        => QueueService.JoinQueueAsync(GetAuthenticatedUserId(), deckId, Context.ConnectionAborted);

    public Task LeaveQueue()
        => QueueService.LeaveQueueAsync(GetAuthenticatedUserId(), Context.ConnectionAborted);
    
}
