// =============================================
// FICHIER: GameHub.cs
// Rôle: Hub SignalR qui gère la connexion des joueurs,
//       la mise en file d'attente (matchmaking), les salons privés (rooms),
//       et les messages/événements de jeu entre deux clients.
//       Les commentaires expliquent le cycle de vie et les patterns utilisés.
// =============================================
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using game.Services;
using VortexTCG.Game.DTO;

namespace game.Hubs;

public class GameHub : Hub
{
    // Le Hub dépend de trois services singleton:
    // - Matchmaker: gère une file d'attente (queue) pour appairer deux joueurs aléatoirement.
    // - RoomService: gère des salons identifiés par un code (type "K3H9Z8") pour jeu privé.
    // - PhaseTimerService: gère les timers de phase (1 minute max par phase).
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
        string? userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
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
        Guid userId = GetAuthenticatedUserId();
        _rooms.Leave(userId, out string? code, out Guid? oppUserId, out bool empty);
        if (code is not null)
        {
            // Notifier le groupe avant de quitter (autres connexions verront l'événement)
            if (oppUserId.HasValue && !empty)
                await Clients.OthersInGroup(code).SendAsync("OpponentLeft", code);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        }

        // 2) S'il était en matchmaking, on retire la paire et on avertit l'adversaire.
        (string? oppId, string? _) = _matchmaker.GetOpponent(Context.ConnectionId);
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);
        if (oppId is not null)
            await Clients.Client(oppId).SendAsync("OpponentLeft", "");

        await base.OnDisconnectedAsync(exception);
    }

    // Permet au client de définir son pseudo côté serveur (stocké dans Matchmaker/RoomService).
    public Task SetName(string name)
    {
        Guid userId = GetAuthenticatedUserId();
        _matchmaker.SetName(Context.ConnectionId, name);
        _rooms.SetName(userId, name);
        return Task.CompletedTask;
    }

    // --- FILE D'ATTENTE (matchmaking aléatoire) ---

    // Le client demande à entrer dans la file.
    // Si un autre joueur est aussi en attente, on créer une "room" logique (GUID) et on envoie "Matched" aux deux.
    public async Task JoinQueue(Guid deckId)
    {
        // S'il était dans un salon privé, on le quitte d'abord pour éviter les situations mixtes.
        await LeaveRoomByCode();

        Guid userId = GetAuthenticatedUserId();

        (bool matched, string? roomId, string? otherConnId, Guid? otherUserId, Guid? otherDeckId) result = _matchmaker.Enqueue(Context.ConnectionId, userId, deckId);
        if (result.matched && result.otherConnId is not null && result.otherUserId.HasValue && result.otherDeckId.HasValue)
        {
            Guid otherUserIdValue = result.otherUserId.Value;
            Guid otherDeckIdValue = result.otherDeckId.Value;

            string code = Guid.NewGuid().ToString("N")[..6].ToUpper();

            // 1. Création de la room pour le premier joueur (userId)
            if (!_rooms.TryCreateRoom(userId, out string createdCode, code))
            {
                await Clients.Caller.SendAsync("RoomCreateError", "CODE_TAKEN");
                return;
            }

            // 2. Ajout du second joueur (otherUserId) dans la room
            _rooms.TryJoinRoom(otherUserIdValue, createdCode, out _, out _);

            await _rooms.SetPlayerDeck(userId, deckId);
            await _rooms.SetPlayerDeck(otherUserIdValue, otherDeckIdValue);

            // 4. Ajout des deux connexions au groupe SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, createdCode);
            await Groups.AddToGroupAsync(result.otherConnId, createdCode);

            // 5. Notifier les deux joueurs avec leur position
            string meName = _rooms.GetName(userId);
            string otherName = _rooms.GetName(otherUserIdValue);

            // userId = créateur = position 1, otherUserId = rejoint = position 2
            await Clients.Client(Context.ConnectionId)
                .SendAsync("Matched", createdCode, new { you = meName, opponent = otherName, position = 1 });
            await Clients.Client(result.otherConnId)
                .SendAsync("Matched", createdCode, new { you = otherName, opponent = meName, position = 2 });
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
        Guid userId = GetAuthenticatedUserId();
        string? code = _rooms.GetRoomOf(userId);
        Guid? opponentId = null;
        bool roomEmpty = false;
        _rooms.Leave(userId, out code, out opponentId, out roomEmpty);
        if (opponentId.HasValue && code != null && !roomEmpty)
        {
            await Clients.Group(code).SendAsync("OpponentLeft", code);
        }
    }

    // Envoi d'un message texte au sein d'un "match" (roomId = GUID coté matchmaking).
    public async Task SendRoomMessage(string roomId, string message)
    {
        Guid userId = GetAuthenticatedUserId();
        string? code = _rooms.GetRoomOf(userId);
        if (code == null || code != roomId) return; // Sécurité: ignorer si pas dans la même room.

        string from = _rooms.GetName(userId);
        await Clients.OthersInGroup(code).SendAsync("ReceiveRoomMessage", code, from, message);
    }

    // --- SALONS PRIVÉS PAR CODE ---

    // Crée un salon privé avec un code (soit proposé par le client, soit aléatoire). Le créateur rejoint automatiquement le group SignalR.
    public async Task CreateRoom(Guid deckId, string? preferredCode = null)
    {
        // Si on était en matchmaking, on s'en retire, car on va jouer par code.
        Guid userId = GetAuthenticatedUserId();
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);

        if (!_rooms.TryCreateRoom(userId, out string code, preferredCode))
        {
            await Clients.Caller.SendAsync("RoomCreateError", "CODE_TAKEN");
            return;
        }

        await _rooms.SetPlayerDeck(userId, deckId);

        // On s'abonne au group SignalR portant le code du salon. Tout broadcast ciblé ira aux membres.
        await Groups.AddToGroupAsync(Context.ConnectionId, code);
        await Clients.Caller.SendAsync("RoomCreated", code);
        await Clients.Caller.SendAsync("Waiting"); // En attente d'un 2e joueur.
    }

    // Rejoindre un salon existant par code. Si l'adversaire est déjà présent, on notifie "Matched" aux deux.
    public async Task JoinRoom(Guid deckId, string code)
    {
        Guid userId = GetAuthenticatedUserId();
        _matchmaker.LeaveOrDisconnect(Context.ConnectionId);

        bool ok = _rooms.TryJoinRoom(userId, code, out Guid? oppUserId, out bool isFull);
        if (!ok)
        {
            await Clients.Caller.SendAsync("RoomJoinError", isFull ? "ROOM_FULL" : "NOT_FOUND");
            return;
        }

        await _rooms.SetPlayerDeck(userId, deckId);

        await Groups.AddToGroupAsync(Context.ConnectionId, code);

        if (oppUserId.HasValue)
        {
            string meName = _rooms.GetName(userId);
            string otherName = _rooms.GetName(oppUserId.Value);

            // Le créateur (oppUserId) est en position 1, celui qui rejoint (userId) est en position 2
            // Envoyer à chaque joueur sa propre position
            int myPosition = _rooms.GetPlayerPosition(userId) ?? 2;
            int oppPosition = _rooms.GetPlayerPosition(oppUserId.Value) ?? 1;

            await Clients.Caller
                .SendAsync("Matched", code, new { you = meName, opponent = otherName, position = myPosition });
            await Clients.OthersInGroup(code)
                .SendAsync("Matched", code, new { you = otherName, opponent = meName, position = oppPosition });
        }
        else
        {
            await Clients.Caller.SendAsync("Waiting"); // Premier dans la room -> attendre un 2e joueur.
        }
    }

    // Envoi d'un message dans un salon privé (adressage par code). Diffusé à "OthersInGroup".
    public async Task SendRoomMessageByCode(string code, string message)
    {
        Guid userId = GetAuthenticatedUserId();
        string? myCode = _rooms.GetRoomOf(userId);
        if (myCode != code) return; // Sécurité: on n'envoie que si l'émetteur appartient bien à ce salon.

        string from = _rooms.GetName(userId);
        await Clients.OthersInGroup(code).SendAsync("ReceiveRoomMessage", code, from, message);
    }

    // Quitter un salon privé (par code). Si la room devient vide, elle est supprimée côté serveur.
    public async Task LeaveRoomByCode()
    {
        Guid userId = GetAuthenticatedUserId();
        _rooms.Leave(userId, out string? code, out Guid? oppUserId, out bool roomEmpty);
        if (code is not null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        if (oppUserId.HasValue && !roomEmpty)
        {
            // Broadcast au groupe (tous les clients de l'adversaire)
            await Clients.Group(code).SendAsync("OpponentLeft", code ?? "");
        }
    }

    // Événement "jeu": jouer une carte. On supporte les deux modes:
    // - par code (rooms privées) -> broadcast au gro   up
    // - par roomId (matchmaking GUID) -> envoi direct à l'adversaire
    public async Task PlayCard(int cardId, int location)
    => await _rooms.PlayCard(GetAuthenticatedUserId(), cardId, location);


    // --- GESTION DES PHASES DE JEU ---

    /// <summary>
    /// Démarre la partie dans une room. Seul le joueur 1 (créateur) peut démarrer.
    /// Initialise le jeu et envoie l'état initial (la pioche est gérée côté serveur à l'entrée en PLACEMENT).
    /// </summary>
    public async Task StartGame()
    {
        Guid userId = GetAuthenticatedUserId();
        PhaseChangeResultDTO? result = await _rooms.StartGame(userId);

        if (result == null)
        {
            return;
        }

        string? code = _rooms.GetRoomOf(userId);
        if (code == null) return;

        // Envoyer l'état initial aux deux joueurs
        await Clients.Group(code).SendAsync("GameStarted", result);
    }

    /// <summary>
    /// Le joueur demande à changer de phase.
    /// Le serveur vérifie si le joueur peut changer de phase et gère les auto-skips.
    /// </summary>
    public async Task ChangePhase()
    {
        Guid userId = GetAuthenticatedUserId();
        PhaseChangeResultDTO result = await _rooms.ChangePhase(userId);

        if (result == null)
        {
            return;
        }

        string? code = _rooms.GetRoomOf(userId);
        if (code == null) return;


        // Envoyer le changement de phase aux deux joueurs

        await Clients.Group(code).SendAsync("PhaseChanged", result);
    }

    #region Handle Attack and defense calls

    public async Task HandleAttackPos(int cardId)
    => await _rooms.EngageAttackCard(GetAuthenticatedUserId(), cardId);

    public async Task HandleDefensePos(int cardId, int opponentCardId)
    => await _rooms.EngageDefenseCard(GetAuthenticatedUserId(), cardId, opponentCardId);

    #endregion
}
