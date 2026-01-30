using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using game.Application.Service;
namespace game.Infrastructure;
public class GameHubClean : Hub
{

    private readonly QueueService _queueService;

    public GameHubClean(QueueService queueService)
    {
        _queueService = queueService;
 
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
    
    public Task JoinQueue(Guid deckId)
        => _queueService.JoinQueueAsync(GetAuthenticatedUserId(), deckId);

    public Task LeaveQueue()
        => _queueService.LeaveQueueAsync(GetAuthenticatedUserId());
    
}
