using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using game.Application.Service;
using game.Domaine.Match.Agregate;
using game.Domaine.Match.Entity;
using game.Domaine.Match.Service;
using game.Domaine.Match.ValueObject;
using game.Infrastructure.Manager;
using SurrenderService = game.Application.Service.SurrenderService;
using DisconnectService = game.Application.Service.DisconnectService;

namespace game.Infrastructure;
public class GameHubClean : Hub
{
    private readonly ILogger<GameHubClean> _logger;

    public GameHubClean(ILogger<GameHubClean> logger)
    {
        _logger = logger;
    }

    private UserId GetAuthenticatedUserId()
    {
        string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            _logger.LogWarning("[HUB] GetAuthenticatedUserId échoué — connectionId={ConnectionId}", Context.ConnectionId);
            throw new HubException("User not authenticated");
        }
        return new UserId(Guid.Parse(userIdClaim));
    }

    private Match GetCurrentMatch()
    {
        UserId userId = GetAuthenticatedUserId();
        Match? match = RoomManager.Instance.GetMatchByUserId(userId);
        if (match == null)
        {
            _logger.LogWarning("[HUB] GetCurrentMatch échoué — aucun match actif pour userId={UserId}", userId);
            throw new HubException("Match not found.");
        }
        return match;
    }

    public override async Task OnConnectedAsync()
    {
        string userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "non authentifié";
        _logger.LogInformation("[HUB] Connexion — connectionId={ConnectionId} userId={UserId}", Context.ConnectionId, userId);
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string rawUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "inconnu";
        if (exception != null)
            _logger.LogWarning(exception, "[HUB] Déconnexion avec erreur — connectionId={ConnectionId} userId={UserId}", Context.ConnectionId, rawUserId);
        else
            _logger.LogInformation("[HUB] Déconnexion normale — connectionId={ConnectionId} userId={UserId}", Context.ConnectionId, rawUserId);

        if (Guid.TryParse(rawUserId, out Guid parsedId))
        {
            try
            {
                await DisconnectService.HandleDisconnectAsync(new UserId(parsedId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HUB] Erreur lors du HandleDisconnect — userId={UserId}", rawUserId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public Task Surrender()
    {
        UserId userId = GetAuthenticatedUserId();
        _logger.LogInformation("[HUB] Surrender — userId={UserId}", userId);
        return SurrenderService.SurrenderAsync(userId, Context.ConnectionAborted);
    }

    public Task JoinQueue(Guid deckId)
    {
        UserId userId = GetAuthenticatedUserId();
        _logger.LogInformation("[HUB] JoinQueue — userId={UserId} deckId={DeckId}", userId, deckId);
        return QueueService.JoinQueueAsync(userId, new DeckId(deckId), Context.ConnectionAborted);
    }

    public Task LeaveQueue()
    {
        UserId userId = GetAuthenticatedUserId();
        _logger.LogInformation("[HUB] LeaveQueue — userId={UserId}", userId);
        return QueueService.LeaveQueueAsync(userId, Context.ConnectionAborted);
    }

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
    public async Task ToggleDefenseCard(int position, int positionOpponentCard)
    {
        try
        {
            await DefenseService.ToggleDefenseCardAsync(
                GetAuthenticatedUserId(),
                position,
                positionOpponentCard,
                Context.ConnectionAborted
            );
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
    }
    public Task ChangePhase()
        => PhaseService.ChangePhaseAsync(
            GetAuthenticatedUserId(),
            Context.ConnectionAborted
        );

}
