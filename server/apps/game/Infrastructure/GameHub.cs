using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using game.Application.Service;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;

namespace game.Infrastructure;
public class GameHubClean : Hub
{
    
    private UserId GetAuthenticatedUserId()
    {
        string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("User not authenticated");
        return new UserId(Guid.Parse(userIdClaim));
    }
    private Match GetCurrentMatch()
        => RoomManager.Instance.GetMatchByUserId(GetAuthenticatedUserId())
           ?? throw new HubException("Match not found.");

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
    
    public Task JoinQueue(Guid deckId)
        => QueueService.JoinQueueAsync(
            GetAuthenticatedUserId(),
            new DeckId(deckId),
            Context.ConnectionAborted
        );

    public Task LeaveQueue()
        => QueueService.LeaveQueueAsync(GetAuthenticatedUserId(), Context.ConnectionAborted);
    public Task ToggleAttackCard(int position)
        => AttackService.ToggleAttackCardAsync(
            GetAuthenticatedUserId(),
            position,
            Context.ConnectionAborted
        );
    public Task PlayCard(int gameCardId, int boardPosition)
        => PlayCardAppService.PlayCardAsync(
            GetCurrentMatch(),
            GetAuthenticatedUserId(),
            gameCardId,
            boardPosition,
            Context.ConnectionAborted
        );
    public Task ToggleDefenseCard(int position, int positionOpponentCard)
        => DefenseService.ToggleDefenseCardAsync(
            GetAuthenticatedUserId(),
            position,
            positionOpponentCard,
            Context.ConnectionAborted
        );
    public Task ChangePhase()
        => PhaseService.ChangePhaseAsync(
            GetAuthenticatedUserId(),
            Context.ConnectionAborted
        );

}
