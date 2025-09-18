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

    // Rooms logiques (GUID), pas des groupes SignalR: juste pour identifier un match 1v1.
    private readonly ConcurrentDictionary<string, (string p1, string p2)> _rooms = new();
    private readonly ConcurrentDictionary<string, string> _conn_to_room = new(); // map conn -> roomId (GUID)

    private readonly object _lock = new(); // protège l'opération d'appairage

    public void set_name(string connection_id, string? name)
        => _names[connection_id] = name ?? $"Player-{connection_id[..5]}";

    public string get_name(string connection_id)
        => _names.TryGetValue(connection_id, out var n) ? n : $"Player-{connection_id[..5]}";

    // Ajoute le joueur dans la file, et tente immédiatement d'appairer 2 candidats valides.
    public (bool matched, string? room_id, string? other_id) enqueue(string connection_id)
    {
        lock (_lock)
        {
            _waiting[connection_id] = true;
            _queue.Enqueue(connection_id);

            string? p1 = dequeue_valid();
            if (p1 is null) return (false, null, null);

            string? p2 = dequeue_valid();
            if (p2 is null)
            {
                // Pas encore de deuxième joueur: on remet p1 en file.
                _queue.Enqueue(p1);
                _waiting[p1] = true;
                return (false, null, null);
            }

            // Deux joueurs -> crée un match (roomId = GUID) et mappe les connexions.
            var room_id = Guid.NewGuid().ToString("N");
            _rooms[room_id] = (p1, p2);
            _conn_to_room[p1] = room_id;
            _conn_to_room[p2] = room_id;

            return (true, room_id, p1 == connection_id ? p2 : p1);
        }
    }

    // Retire de la queue jusqu'à trouver un ConnectionId toujours marqué "en attente".
    private string? dequeue_valid()
    {
        while (_queue.TryDequeue(out var c_id))
        {
            if (_waiting.TryRemove(c_id, out _))
                return c_id;
        }
        return null;
    }

    public string? get_room_id(string connection_id)
        => _conn_to_room.TryGetValue(connection_id, out var r) ? r : null;

    // Récupère l'adversaire + roomId du match actif d'un joueur, si existe.
    public (string? opponent_id, string? room_id) get_opponent(string my_connection_id)
    {
        if (_conn_to_room.TryGetValue(my_connection_id, out var room_id)
            && _rooms.TryGetValue(room_id, out var pair))
        {
            var opp = pair.p1 == my_connection_id ? pair.p2 : pair.p1;
            return (opp, room_id);
        }
        return (null, null);
    }

    // Nettoie tous les états quand un joueur quitte ou se déconnecte.
    public void leave_or_disconnect(string connection_id)
    {
        _waiting.TryRemove(connection_id, out _);

        if (_conn_to_room.TryRemove(connection_id, out var room_id))
        {
            if (_rooms.TryGetValue(room_id, out var pair))
            {
                var opp = pair.p1 == connection_id ? pair.p2 : pair.p1;
                _conn_to_room.TryRemove(opp, out _); // l'adversaire est "libéré"
            }
            _rooms.TryRemove(room_id, out _);
        }

        _names.TryRemove(connection_id, out _);
    }
}
