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
        _rooms.leave(Context.ConnectionId, out var code, out var opp_code_id, out var empty);
        if (code is not null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        if (opp_code_id is not null && !empty)
            await Clients.Client(opp_code_id).SendAsync("OpponentLeft", code ?? "");

        // 2) S'il était en matchmaking, on retire la paire et on avertit l'adversaire.
        var (opp_id, _) = _matchmaker.get_opponent(Context.ConnectionId);
        _matchmaker.leave_or_disconnect(Context.ConnectionId);
        if (opp_id is not null)
            await Clients.Client(opp_id).SendAsync("OpponentLeft", "");

        await base.OnDisconnectedAsync(exception);
    }

    // Permet au client de définir son pseudo côté serveur (stocké dans Matchmaker/RoomService).
    public Task set_name(string name)
    {
        _matchmaker.set_name(Context.ConnectionId, name);
        _rooms.set_name(Context.ConnectionId, name);
        return Task.CompletedTask;
    }

    // --- FILE D'ATTENTE (matchmaking aléatoire) ---

    // Le client demande à entrer dans la file.
    // Si un autre joueur est aussi en attente, on créer une "room" logique (GUID) et on envoie "Matched" aux deux.
    public async Task join_queue()
    {
        // S'il était dans un salon privé, on le quitte d'abord pour éviter les situations mixtes.
        await leave_room_by_code();

        var result = _matchmaker.enqueue(Context.ConnectionId);
        if (result.matched && result.room_id is not null && result.other_id is not null)
        {
            var me_name = _matchmaker.get_name(Context.ConnectionId);
            var other_name = _matchmaker.get_name(result.other_id);

            await Clients.Clients(new[] { Context.ConnectionId, result.other_id })
                .SendAsync("Matched", result.room_id, new { you = me_name, opponent = other_name });
        }
        else
        {
            // Personne en face pour le moment -> le client peut afficher un écran d'attente.
            await Clients.Caller.SendAsync("Waiting");
        }
    }

    // Le client sort de la file d'attente. On prévient l'adversaire si un match était déjà formé.
    public async Task leave_queue()
    {
        var (opp_id, room_id) = _matchmaker.get_opponent(Context.ConnectionId);
        _matchmaker.leave_or_disconnect(Context.ConnectionId);
        if (opp_id is not null && room_id is not null)
            await Clients.Client(opp_id).SendAsync("OpponentLeft", room_id);
    }

    // Envoi d'un message texte au sein d'un "match" (roomId = GUID coté matchmaking).
    public async Task send_room_message(string room_id, string message)
    {
        var (opp_id, my_room_id) = _matchmaker.get_opponent(Context.ConnectionId);
        if (opp_id is null || my_room_id != room_id) return; // Sécurité: ignorer si pas dans la même room.

        var from = _matchmaker.get_name(Context.ConnectionId);
        await Clients.Client(opp_id).SendAsync("ReceiveRoomMessage", room_id, from, message);
    }

    // --- SALONS PRIVÉS PAR CODE ---

    // Crée un salon privé avec un code (soit proposé par le client, soit aléatoire). Le créateur rejoint automatiquement le group SignalR.
    public async Task create_room(string? preferredCode = null)
    {
    // Si on était en matchmaking, on s'en retire, car on va jouer par code.
    _matchmaker.leave_or_disconnect(Context.ConnectionId);

        if (!_rooms.try_create_room(Context.ConnectionId, out var code, preferredCode))
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
    public async Task join_room(string code)
    {
        _matchmaker.leave_or_disconnect(Context.ConnectionId);

        var ok = _rooms.try_join_room(Context.ConnectionId, code, out var opp_id, out var isFull);
        if (!ok)
        {
            await Clients.Caller.SendAsync("RoomJoinError", isFull ? "ROOM_FULL" : "NOT_FOUND");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, code);

        if (opp_id is not null)
        {
            var me_name = _rooms.get_name(Context.ConnectionId);
            var other_name = _rooms.get_name(opp_id);

            await Clients.Clients(new[] { Context.ConnectionId, opp_id })
                .SendAsync("Matched", code, new { you = me_name, opponent = other_name });
        }
        else
        {
            await Clients.Caller.SendAsync("Waiting"); // Premier dans la room -> attendre un 2e joueur.
        }
    }

    // Envoi d'un message dans un salon privé (adressage par code). Diffusé à "OthersInGroup".
    public async Task send_room_message_by_code(string code, string message)
    {
        var my_code = _rooms.get_room_of(Context.ConnectionId);
        if (my_code != code) return; // Sécurité: on n'envoie que si l'émetteur appartient bien à ce salon.

        var from = _rooms.get_name(Context.ConnectionId);
        await Clients.OthersInGroup(code).SendAsync("ReceiveRoomMessage", code, from, message);
    }

    // Quitter un salon privé (par code). Si la room devient vide, elle est supprimée côté serveur.
    public async Task leave_room_by_code()
    {
        _rooms.leave(Context.ConnectionId, out var code, out var opponent_id, out var roomEmpty);
        if (code is not null) await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
        if (opponent_id is not null && !roomEmpty)
            await Clients.Client(opponent_id).SendAsync("OpponentLeft", code ?? "");
    }

    // Événement "jeu": jouer une carte. On supporte les deux modes:
    // - par code (rooms privées) -> broadcast au group
    // - par roomId (matchmaking GUID) -> envoi direct à l'adversaire
    public async Task play_card(string keyOrCode, int cardId)
    {
        // Mode salon privé (code)
        var code = _rooms.get_room_of(Context.ConnectionId);
        if (code is not null && code == keyOrCode)
        {
            var from = _rooms.get_name(Context.ConnectionId);
            await Clients.OthersInGroup(code).SendAsync("OpponentPlayedCard", code, from, cardId);
            return;
        }

        // Mode matchmaking (GUID roomId)
        var (opp_id, room_id) = _matchmaker.get_opponent(Context.ConnectionId);
        if (opp_id is not null && room_id == keyOrCode)
        {
            var from = _matchmaker.get_name(Context.ConnectionId);
            await Clients.Client(opp_id).SendAsync("OpponentPlayedCard", room_id, from, cardId);
        }
    }
}
