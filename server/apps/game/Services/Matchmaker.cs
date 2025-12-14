// =============================================
// FICHIER: Services/Matchmaker.cs
// Rôle: Gestion d'une file d'attente concurrente pour appairer deux joueurs.
//       Utilise des structures thread-safe (ConcurrentQueue/Dictionary) + lock.
// =============================================
using System.Collections.Concurrent;

namespace game.Services;

public class Matchmaker
{
    private readonly ConcurrentQueue<string> _queue = new(); // file d'attente des ConnectionId
    private readonly ConcurrentDictionary<string, bool> _waiting = new(); // marque qui est encore "valide" en attente
    private readonly ConcurrentDictionary<string, string> _names = new(); // pseudo par ConnectionId

    // Mapping ConnectionId -> UserId (nécessaire pour créer une room côté RoomService lors du matchmaking)
    private readonly ConcurrentDictionary<string, Guid> _connToUser = new();

    // Mapping ConnectionId -> DeckId (nécessaire pour initialiser le jeu)
    private readonly ConcurrentDictionary<string, Guid> _connToDeck = new();

    // Rooms logiques (GUID), pas des groupes SignalR: juste pour identifier un match 1v1.
    private readonly ConcurrentDictionary<string, (string p1, string p2)> _rooms = new();
    private readonly ConcurrentDictionary<string, string> _connToRoom = new(); // map conn -> roomId (GUID)

    private readonly object _lock = new(); // protège l'opération d'appairage

    public void SetName(string connectionId, string? name)
        => _names[connectionId] = name ?? $"Player-{connectionId[..5]}";

    public string GetName(string connectionId)
        => _names.TryGetValue(connectionId, out var n) ? n : $"Player-{connectionId[..5]}";

    // Ajoute le joueur dans la file, et tente immédiatement d'appairer 2 candidats valides.
    public (bool matched, string? roomId, string? otherConnId, Guid? otherUserId, Guid? otherDeckId) Enqueue(string connectionId, Guid userId, Guid deckId)
    {
        lock (_lock)
        {
            _waiting[connectionId] = true;
            _queue.Enqueue(connectionId);
            _connToUser[connectionId] = userId;
            _connToDeck[connectionId] = deckId;

            string? p1 = DequeueValid();
            if (p1 is null) return (false, null, null, null, null);

            string? p2 = DequeueValid();
            if (p2 is null)
            {
                // Pas encore de deuxième joueur: on remet p1 en file.
                _queue.Enqueue(p1);
                _waiting[p1] = true;
                return (false, null, null, null, null);
            }

            // Deux joueurs -> crée un match (roomId = GUID) et mappe les connexions.
            var roomId = Guid.NewGuid().ToString("N");
            _rooms[roomId] = (p1, p2);
            _connToRoom[p1] = roomId;
            _connToRoom[p2] = roomId;

            var otherConnId = p1 == connectionId ? p2 : p1;
            _connToUser.TryGetValue(otherConnId, out var otherUserId);
            _connToDeck.TryGetValue(otherConnId, out var otherDeckId);

            return (true, roomId, otherConnId, otherUserId, otherDeckId);
        }
    }

    // Retire de la queue jusqu'à trouver un ConnectionId toujours marqué "en attente".
    private string? DequeueValid()
    {
        while (_queue.TryDequeue(out var cId))
        {
            if (_waiting.TryRemove(cId, out _))
                return cId;
        }
        return null;
    }

    public string? GetRoomId(string connectionId)
        => _connToRoom.TryGetValue(connectionId, out var r) ? r : null;

    // Récupère l'adversaire + roomId du match actif d'un joueur, si existe.
    public (string? opponentId, string? roomId) GetOpponent(string myConnectionId)
    {
        if (_connToRoom.TryGetValue(myConnectionId, out var roomId)
            && _rooms.TryGetValue(roomId, out var pair))
        {
            var opp = pair.p1 == myConnectionId ? pair.p2 : pair.p1;
            return (opp, roomId);
        }
        return (null, null);
    }

    // Nettoie tous les états quand un joueur quitte ou se déconnecte.
    public void LeaveOrDisconnect(string connectionId)
    {
        _waiting.TryRemove(connectionId, out _);
        _connToUser.TryRemove(connectionId, out _);
        _connToDeck.TryRemove(connectionId, out _);

        if (_connToRoom.TryRemove(connectionId, out var roomId))
        {
            if (_rooms.TryGetValue(roomId, out var pair))
            {
                var opp = pair.p1 == connectionId ? pair.p2 : pair.p1;
                _connToRoom.TryRemove(opp, out _); // l'adversaire est "libéré"
            }
            _rooms.TryRemove(roomId, out _);
        }

        _names.TryRemove(connectionId, out _);
    }
}
