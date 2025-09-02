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
    private readonly ConcurrentDictionary<string, string> _connToRoom = new(); // conn -> code
    private readonly ConcurrentDictionary<string, string> _names = new();      // pseudo par ConnectionId

    // Alphabet sans voyelles ambiguës (pas d'I/O/0/1) pour des codes lisibles.
    private static readonly char[] Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public void SetName(string connectionId, string? name)
        => _names[connectionId] = string.IsNullOrWhiteSpace(name)
            ? $"Player-{connectionId[..5]}"
            : name!;

    public string GetName(string connectionId)
        => _names.TryGetValue(connectionId, out var n) ? n : $"Player-{connectionId[..5]}";

    // Création d'un salon: soit avec un code préféré (si libre), soit avec un code aléatoire.
    public bool TryCreateRoom(string connectionId, out string code, string? preferred = null)
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
            room.Members.Add(connectionId);
            _connToRoom[connectionId] = code;
        }
        return true;
    }

    // Rejoindre un salon existant. On refuse au-delà de 2 membres et on remonte un flag isFull.
    public bool TryJoinRoom(string connectionId, string code, out string? opponentId, out bool isFull)
    {
        opponentId = null; isFull = false;
        code = code.Trim().ToUpperInvariant();

        if (!_rooms.TryGetValue(code, out var room)) return false;

        lock (room)
        {
            if (room.Members.Count >= 2) { isFull = true; return false; }
            room.Members.Add(connectionId);
            _connToRoom[connectionId] = code;
            opponentId = room.Members.FirstOrDefault(id => id != connectionId);
            return true;
        }
    }

    public string? GetRoomOf(string connectionId)
        => _connToRoom.TryGetValue(connectionId, out var c) ? c : null;

    public string? GetOpponentOf(string connectionId)
    {
        var code = GetRoomOf(connectionId);
        if (code is null) return null;
        if (!_rooms.TryGetValue(code, out var room)) return null;

        lock (room)
            return room.Members.FirstOrDefault(id => id != connectionId);
    }

    // Quitte le salon. Si personne ne reste, on supprime la room pour libérer la mémoire/codes.
    public void Leave(string connectionId, out string? code, out string? opponentId, out bool roomEmpty)
    {
        code = null; opponentId = null; roomEmpty = false;

        if (!_connToRoom.TryRemove(connectionId, out var c)) return;
        code = c;

        if (_rooms.TryGetValue(c, out var room))
        {
            lock (room)
            {
                room.Members.Remove(connectionId);
                opponentId = room.Members.FirstOrDefault();
                roomEmpty = room.Members.Count == 0;
                if (roomEmpty) _rooms.TryRemove(c, out _);
            }
        }

        _names.TryRemove(connectionId, out _);
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
