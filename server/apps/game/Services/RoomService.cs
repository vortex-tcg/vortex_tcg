// =============================================
// FICHIER: Services/RoomService.cs
// Rôle: Gestion de salons privés identifiés par un code lisible.
//       Chaque room peut contenir jusqu'à 2 membres. On utilise des structures thread-safe
//       et des locks fins par salon pour éviter les conditions de course.
// =============================================
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace game.Services;

public class RoomService
{
    // Un salon est défini par son ensemble de membres (ConnectionId). HashSet pour éviter les doublons.
    private record Room(HashSet<string> Members);

    private readonly ConcurrentDictionary<string, Room> _rooms = new();       // code -> Room
    private readonly ConcurrentDictionary<string, string> _conn_to_room = new(); // conn -> code
    private readonly ConcurrentDictionary<string, string> _names = new();      // pseudo par ConnectionId

    // Alphabet sans voyelles ambiguës (pas d'I/O/0/1) pour des codes lisibles.
    private static readonly char[] Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public void set_name(string connection_id, string? name)
        => _names[connection_id] = string.IsNullOrWhiteSpace(name)
            ? $"Player-{connection_id[..5]}"
            : name!;

    public string get_name(string connection_id)
        => _names.TryGetValue(connection_id, out var n) ? n : $"Player-{connection_id[..5]}";

    // Création d'un salon: soit avec un code préféré (si libre), soit avec un code aléatoire.
    public bool try_create_room(string connection_id, out string code, string? preferred = null)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            code = preferred.Trim().ToUpperInvariant();
            if (!_rooms.TryAdd(code, new Room(new()))) return false; // code déjà pris
        }
        else
        {
            while (true)
            {
                code = GenerateCode(6); // ex: "F9K7ZQ"
                if (_rooms.TryAdd(code, new Room(new()))) break;
            }
        }

        var room = _rooms[code];
        lock (room)
        {
            room.Members.Add(connection_id);
            _conn_to_room[connection_id] = code;
        }
        return true;
    }

    // Rejoindre un salon existant. On refuse au-delà de 2 membres et on remonte un flag isFull.
    public bool try_join_room(string connection_id, string code, out string? opponent_id, out bool isFull)
    {
        opponent_id = null; isFull = false;
        code = code.Trim().ToUpperInvariant();

        if (!_rooms.TryGetValue(code, out var room)) return false;

        lock (room)
        {
            if (room.Members.Count >= 2) { isFull = true; return false; }
            room.Members.Add(connection_id);
            _conn_to_room[connection_id] = code;
            opponent_id = room.Members.FirstOrDefault(id => id != connection_id);
            return true;
        }
    }

    public string? get_room_of(string connection_id)
        => _conn_to_room.TryGetValue(connection_id, out var c) ? c : null;

    public string? get_opponent_of(string connection_id)
    {
        var code = get_room_of(connection_id);
        if (code is null) return null;
        if (!_rooms.TryGetValue(code, out var room)) return null;

        lock (room)
            return room.Members.FirstOrDefault(id => id != connection_id);
    }

    // Quitte le salon. Si personne ne reste, on supprime la room pour libérer la mémoire/codes.
    public void leave(string connection_id, out string? code, out string? opponent_id, out bool roomEmpty)
    {
        code = null; opponent_id = null; roomEmpty = false;

        if (!_conn_to_room.TryRemove(connection_id, out var c)) return;
        code = c;

        if (_rooms.TryGetValue(c, out var room))
        {
            lock (room)
            {
                room.Members.Remove(connection_id);
                opponent_id = room.Members.FirstOrDefault();
                roomEmpty = room.Members.Count == 0;
                if (roomEmpty) _rooms.TryRemove(c, out _);
            }
        }

        _names.TryRemove(connection_id, out _);
    }

    // Génère un code "humainement lisible" en utilisant un RNG cryptographique.
    private string GenerateCode(int len)
    {
        var bytes = new byte[len];
        _rng.GetBytes(bytes);
        var chars = new char[len];
        for (int i = 0; i < len; i++) chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        return new(chars);
    }
}
