// =============================================
// FICHIER: GameHub.cs
// Rôle: Hub SignalR qui gère la connexion des joueurs,
//       la mise en file d'attente (matchmaking), les salons privés (rooms),
//       et les messages/événements de jeu entre deux clients.
//       Les commentaires expliquent le cycle de vie et les patterns utilisés.
// =============================================
using Microsoft.AspNetCore.SignalR;
using game.Services;

namespace game.Hubs;

public class GameHub : Hub
{
    // Le Hub dépend de deux services singleton:
    // - Matchmaker: gère une file d'attente (queue) pour appairer deux joueurs aléatoirement.
    // - RoomService: gère des salons identifiés par un code (type "K3H9Z8") pour jeu privé.
    private readonly Matchmaker _matchmaker;
    private readonly RoomService _rooms;

    public GameHub(Matchmaker matchmaker, RoomService rooms)
    {
        _matchmaker = matchmaker;
        _rooms = rooms;
    }

    // Extrait l'ID utilisateur (userId) depuis le token JWT d'authentification.
    // Cet ID est permanent (GUID) et survit aux reconnexions, contrairement au ConnectionId.
    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = Context.User?.FindFirst("sub")?.Value
            ?? throw new HubException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }

    // Méthode de cycle de vie appelée lorsque le client établit la websocket avec le Hub.
    // On répond au client avec son ConnectionId pour qu'il l'affiche/log s'il veut.
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    // Méthode de cycle de vie appelée quand un client se déconnecte (fermeture page, réseau, etc.).
    // On nettoie son état dans RoomService et Matchmaker, et on notifie l'adversaire si besoin.
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // 1) Si le joueur était dans un salon par code, on le retire.
        var userId = GetAuthenticatedUserId();
        _rooms.Leave(userId, out var code, out var oppUserId, out var empty);
        if (code is not null)
        {
            // Notifier le groupe avant de quitter (autres connexions verront l'événement)
            if (oppUserId.HasValue && !empty)
                await Clients.OthersInGroup(code).SendAsync("OpponentLeft", code);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        }

        // 2) S'il était en matchmaking, on retire la paire et on avertit l'adversaire.
        var (oppId, _) = _matchmaker.GetOpponent(Context.ConnectionId);
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);
        if (oppId is not null)
            await Clients.Client(oppId).SendAsync("OpponentLeft", "");

        await base.OnDisconnectedAsync(exception);
    }

    // Permet au client de définir son pseudo côté serveur (stocké dans Matchmaker/RoomService).
    public Task SetName(string name)
    {
        var userId = GetAuthenticatedUserId();
        _matchmaker.SetName(Context.ConnectionId, name);
        _rooms.SetName(userId, name);
        return Task.CompletedTask;
    }

    // --- FILE D'ATTENTE (matchmaking aléatoire) ---

    // Le client demande à entrer dans la file.
    // Si un autre joueur est aussi en attente, on créer une "room" logique (GUID) et on envoie "Matched" aux deux.
    public async Task JoinQueue()
    {
        // S'il était dans un salon privé, on le quitte d'abord pour éviter les situations mixtes.
        await LeaveRoomByCode();

        var result = _matchmaker.Enqueue(Context.ConnectionId);
        if (result.matched && result.roomId is not null && result.otherId is not null)
        {
            var meName = _matchmaker.GetName(Context.ConnectionId);
            var otherName = _matchmaker.GetName(result.otherId);

            await Clients.Clients(new[] { Context.ConnectionId, result.otherId })
                .SendAsync("Matched", result.roomId, new { you = meName, opponent = otherName });
        }
        else
        {
            // Personne en face pour le moment -> le client peut afficher un écran d'attente.
            await Clients.Caller.SendAsync("Waiting");
        }
    }

    // Le client sort de la file d'attente. On prévient l'adversaire si un match était déjà formé.
    public async Task LeaveQueue()
    {
        var (oppId, roomId) = _matchmaker.GetOpponent(Context.ConnectionId);
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);
        if (oppId is not null && roomId is not null)
            await Clients.Client(oppId).SendAsync("OpponentLeft", roomId);
    }

    // Envoi d'un message texte au sein d'un "match" (roomId = GUID coté matchmaking).
    public async Task SendRoomMessage(string roomId, string message)
    {
        var (oppId, myRoomId) = _matchmaker.GetOpponent(Context.ConnectionId);
        if (oppId is null || myRoomId != roomId) return; // Sécurité: ignorer si pas dans la même room.

        var from = _matchmaker.GetName(Context.ConnectionId);
        await Clients.Client(oppId).SendAsync("ReceiveRoomMessage", roomId, from, message);
    }

    // --- SALONS PRIVÉS PAR CODE ---

    // Crée un salon privé avec un code (soit proposé par le client, soit aléatoire). Le créateur rejoint automatiquement le group SignalR.
    public async Task CreateRoom(string? preferredCode = null)
    {
        // Si on était en matchmaking, on s'en retire, car on va jouer par code.
        var userId = GetAuthenticatedUserId();
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);

        if (!_rooms.TryCreateRoom(userId, out var code, preferredCode))
        {
            await Clients.Caller.SendAsync("RoomCreateError", "CODE_TAKEN");
            return;
        }

        // On s'abonne au group SignalR portant le code du salon. Tout broadcast ciblé ira aux membres.
        await Groups.AddToGroupAsync(Context.ConnectionId, code);
        await Clients.Caller.SendAsync("RoomCreated", code);
        await Clients.Caller.SendAsync("Waiting"); // En attente d'un 2e joueur.
    }

    // Rejoindre un salon existant par code. Si l'adversaire est déjà présent, on notifie "Matched" aux deux.
    public async Task JoinRoom(string code)
    {
        var userId = GetAuthenticatedUserId();
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);

        var ok = _rooms.TryJoinRoom(userId, code, out var oppUserId, out var isFull);
        if (!ok)
        {
            await Clients.Caller.SendAsync("RoomJoinError", isFull ? "ROOM_FULL" : "NOT_FOUND");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, code);

        if (oppUserId.HasValue)
        {
            var meName = _rooms.GetName(userId);
            var otherName = _rooms.GetName(oppUserId.Value);

            // Broadcast au groupe entier (supporte reconnexions multiples)
            await Clients.Group(code)
                .SendAsync("Matched", code, new { you = meName, opponent = otherName });
        }
        else
        {
            await Clients.Caller.SendAsync("Waiting"); // Premier dans la room -> attendre un 2e joueur.
        }
    }

    // Envoi d'un message dans un salon privé (adressage par code). Diffusé à "OthersInGroup".
    public async Task SendRoomMessageByCode(string code, string message)
    {
        var userId = GetAuthenticatedUserId();
        var myCode = _rooms.GetRoomOf(userId);
        if (myCode != code) return; // Sécurité: on n'envoie que si l'émetteur appartient bien à ce salon.

        var from = _rooms.GetName(userId);
        await Clients.OthersInGroup(code).SendAsync("ReceiveRoomMessage", code, from, message);
    }

    // Quitter un salon privé (par code). Si la room devient vide, elle est supprimée côté serveur.
    public async Task LeaveRoomByCode()
    {
        var userId = GetAuthenticatedUserId();
        _rooms.Leave(userId, out var code, out var oppUserId, out var roomEmpty);
        if (code is not null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        if (oppUserId.HasValue && !roomEmpty)
        {
            // Broadcast au groupe (tous les clients de l'adversaire)
            await Clients.Group(code).SendAsync("OpponentLeft", code ?? "");
        }
    }

    // Événement "jeu": jouer une carte. On supporte les deux modes:
    // - par code (rooms privées) -> broadcast au group
    // - par roomId (matchmaking GUID) -> envoi direct à l'adversaire
    public async Task PlayCard(string keyOrCode, int cardId)
    {
        // Mode salon privé (code)
        var userId = GetAuthenticatedUserId();
        var code = _rooms.GetRoomOf(userId);
        if (code is not null && code == keyOrCode)
        {
            var from = _rooms.GetName(userId);
            await Clients.OthersInGroup(code).SendAsync("OpponentPlayedCard", code, from, cardId);
            return;
        }

        // Mode matchmaking (GUID roomId)
        var (oppId, roomId) = _matchmaker.GetOpponent(Context.ConnectionId);
        if (oppId is not null && roomId == keyOrCode)
        {
            var from = _matchmaker.GetName(Context.ConnectionId);
            await Clients.Client(oppId).SendAsync("OpponentPlayedCard", roomId, from, cardId);
        }
    }
}
