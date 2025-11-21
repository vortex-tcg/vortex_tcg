// =============================================
// FICHIER: Services/RoomService.cs
// =============================================
// RÔLE PRINCIPAL:
// Ce service gère le cycle de vie complet des salons de jeu (rooms), de la création
// à la suppression, en passant par l'initialisation de la partie de jeu.
//
// RESPONSABILITÉS:
// 1. Créer des salons avec des codes uniques et lisibles (ex: "F9K7ZQ")
// 2. Gérer les connexions/déconnexions des joueurs (max 2 par salon)
// 3. Lier les joueurs à leurs decks et initialiser l'état de la partie (RoomObject)
// 4. Fournir un accès thread-safe aux informations des salons
//
// ARCHITECTURE THREAD-SAFE:
// - Utilise ConcurrentDictionary pour les accès concurrents
// - Utilise des locks fins par salon pour les opérations atomiques
// - Sépare les opérations async (await) des sections lock
// =============================================

using System.Collections.Concurrent;
using System.Security.Cryptography;
using VortexTCG.Game.Object;

namespace game.Services;

/// <summary>
/// Service de gestion des salons de jeu multijoueur.
/// Thread-safe et conçu pour SignalR/WebSocket avec des connexions concurrentes.
/// </summary>
public class RoomService
{
    #region Structures internes
    
    /// <summary>
    /// Représente un salon de jeu avec ses métadonnées.
    /// Contient à la fois les informations de connexion (Members) et l'état de jeu (GameRoom).
    /// </summary>
    private class Room
    {
        /// <summary>Ensemble des ConnectionId SignalR des joueurs connectés (max 2)</summary>
        public HashSet<string> Members { get; } = new();
        
        /// <summary>Instance du RoomObject contenant l'état complet de la partie (decks, mains, boards, etc.)</summary>
        public VortexTCG.Game.Object.Room? GameRoom { get; set; }
        
        /// <summary>ID utilisateur du premier joueur (celui qui a créé le salon)</summary>
        public Guid? User1Id { get; set; }
        
        /// <summary>ID utilisateur du second joueur (celui qui a rejoint)</summary>
        public Guid? User2Id { get; set; }
        
        /// <summary>ID du deck sélectionné par le joueur 1</summary>
        public Guid? User1DeckId { get; set; }
        
        /// <summary>ID du deck sélectionné par le joueur 2</summary>
        public Guid? User2DeckId { get; set; }
        
        /// <summary>Indique si la partie a été initialisée (RoomObject créé et configuré)</summary>
        public bool IsGameInitialized { get; set; }
    }
    
    #endregion

    #region Champs privés
    
    /// <summary>Dictionnaire thread-safe: code du salon (ex: "F9K7ZQ") -> Room</summary>
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    
    /// <summary>Dictionnaire thread-safe: ConnectionId -> code du salon</summary>
    private readonly ConcurrentDictionary<string, string> _connToRoom = new();
    
    /// <summary>Dictionnaire thread-safe: ConnectionId -> pseudo du joueur</summary>
    private readonly ConcurrentDictionary<string, string> _names = new();

    /// <summary>
    /// Alphabet optimisé pour la lisibilité humaine.
    /// Exclut: I, O (confusion avec 1, 0), voyelles pour éviter les mots offensants.
    /// </summary>
    private static readonly char[] Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
    
    /// <summary>Générateur de nombres aléatoires cryptographiquement sécurisé</summary>
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    
    #endregion

    #region Gestion des pseudos

    /// <summary>
    /// Définit le pseudo d'un joueur pour un ConnectionId donné.
    /// Si le nom est vide, génère un pseudo par défaut (Player-{5 premiers caractères du ConnectionId}).
    /// </summary>
    /// <param name="connectionId">ID de connexion SignalR du joueur</param>
    /// <param name="name">Pseudo choisi par le joueur (peut être null)</param>
    public void SetName(string connectionId, string? name)
        => _names[connectionId] = string.IsNullOrWhiteSpace(name)
            ? $"Player-{connectionId[..5]}"
            : name!;

    /// <summary>
    /// Récupère le pseudo d'un joueur. Si non défini, retourne un pseudo par défaut.
    /// </summary>
    /// <param name="connectionId">ID de connexion SignalR du joueur</param>
    /// <returns>Pseudo du joueur ou pseudo par défaut</returns>
    public string GetName(string connectionId)
        => _names.TryGetValue(connectionId, out var n) ? n : $"Player-{connectionId[..5]}";

    #endregion

    #region Création et gestion des salons

    /// <summary>
    /// Crée un nouveau salon de jeu et y ajoute le créateur.
    /// </summary>
    /// <param name="connectionId">ID de connexion du joueur créant le salon</param>
    /// <param name="code">Code généré ou choisi pour le salon (ex: "F9K7ZQ")</param>
    /// <param name="preferred">Code personnalisé souhaité (optionnel). Si null, génère un code aléatoire.</param>
    /// <returns>
    /// true si le salon a été créé avec succès.
    /// false si le code préféré est déjà utilisé.
    /// </returns>
    /// <remarks>
    /// ATOMICITÉ: L'ajout du salon dans _rooms est atomique grâce à TryAdd.
    /// Le lock garantit que l'ajout du membre et du mapping est cohérent.
    /// </remarks>
    public bool TryCreateRoom(string connectionId, out string code, string? preferred = null)
    {
        // Cas 1: Code personnalisé demandé
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            code = preferred.Trim().ToUpperInvariant();
            // TryAdd est atomique: retourne false si le code existe déjà
            if (!_rooms.TryAdd(code, new Room())) return false;
        }
        // Cas 2: Génération automatique d'un code unique
        else
        {
            // Boucle jusqu'à trouver un code libre (très rare de boucler avec 36^6 possibilités)
            while (true)
            {
                code = GenerateCode(6); // ex: "F9K7ZQ"
                if (_rooms.TryAdd(code, new Room())) break;
            }
        }

        // Ajout du créateur au salon (lock pour éviter les races sur Members)
        var room = _rooms[code];
        lock (room)
        {
            room.Members.Add(connectionId);
            _connToRoom[connectionId] = code;
        }
        return true;
    }

    /// <summary>
    /// Permet à un joueur de rejoindre un salon existant.
    /// Maximum 2 joueurs par salon.
    /// </summary>
    /// <param name="connectionId">ID de connexion du joueur rejoignant</param>
    /// <param name="code">Code du salon à rejoindre</param>
    /// <param name="opponentId">ConnectionId de l'adversaire déjà présent (null si erreur)</param>
    /// <param name="isFull">Indique si le salon est déjà plein (2/2 joueurs)</param>
    /// <returns>
    /// true si le joueur a rejoint avec succès.
    /// false si le salon n'existe pas ou est plein.
    /// </returns>
    /// <remarks>
    /// VÉRIFICATIONS:
    /// 1. Existence du salon
    /// 2. Capacité (< 2 joueurs)
    /// 3. Ajout atomique du membre
    /// </remarks>
    public bool TryJoinRoom(string connectionId, string code, out string? opponentId, out bool isFull)
    {
        opponentId = null;
        isFull = false;
        code = code.Trim().ToUpperInvariant();

        // Vérifier l'existence du salon
        if (!_rooms.TryGetValue(code, out var room)) return false;

        lock (room)
        {
            // Vérifier la capacité
            if (room.Members.Count >= 2)
            {
                isFull = true;
                return false;
            }
            
            // Ajouter le joueur et enregistrer le mapping
            room.Members.Add(connectionId);
            _connToRoom[connectionId] = code;
            
            // Récupérer l'adversaire (le seul autre membre)
            opponentId = room.Members.FirstOrDefault(id => id != connectionId);
            return true;
        }
    }

    /// <summary>
    /// Récupère le code du salon dans lequel se trouve un joueur.
    /// </summary>
    /// <param name="connectionId">ID de connexion du joueur</param>
    /// <returns>Code du salon ou null si le joueur n'est dans aucun salon</returns>
    public string? GetRoomOf(string connectionId)
        => _connToRoom.TryGetValue(connectionId, out var c) ? c : null;

    /// <summary>
    /// Récupère le ConnectionId de l'adversaire d'un joueur.
    /// </summary>
    /// <param name="connectionId">ID de connexion du joueur</param>
    /// <returns>ConnectionId de l'adversaire ou null si pas d'adversaire/salon</returns>
    public string? GetOpponentOf(string connectionId)
    {
        var code = GetRoomOf(connectionId);
        if (code is null) return null;
        if (!_rooms.TryGetValue(code, out var room)) return null;

        lock (room)
            return room.Members.FirstOrDefault(id => id != connectionId);
    }

    /// <summary>
    /// Retire un joueur d'un salon. Si le salon devient vide, il est supprimé.
    /// </summary>
    /// <param name="connectionId">ID de connexion du joueur quittant</param>
    /// <param name="code">Code du salon quitté (null si le joueur n'était dans aucun salon)</param>
    /// <param name="opponentId">ConnectionId de l'adversaire restant (null si aucun)</param>
    /// <param name="roomEmpty">Indique si le salon a été supprimé car vide</param>
    /// <remarks>
    /// NETTOYAGE:
    /// - Supprime le mapping connectionId -> room
    /// - Retire le joueur de la liste des membres
    /// - Supprime le salon si vide (libération mémoire et code)
    /// - Supprime le pseudo du joueur
    /// </remarks>
    public void Leave(string connectionId, out string? code, out string? opponentId, out bool roomEmpty)
    {
        code = null;
        opponentId = null;
        roomEmpty = false;

        // Retirer le mapping connectionId -> room
        if (!_connToRoom.TryRemove(connectionId, out var c)) return;
        code = c;

        // Retirer le joueur du salon et vérifier si vide
        if (_rooms.TryGetValue(c, out var room))
        {
            lock (room)
            {
                room.Members.Remove(connectionId);
                opponentId = room.Members.FirstOrDefault();
                roomEmpty = room.Members.Count == 0;
                
                // Supprimer le salon s'il est vide (libération mémoire + rend le code disponible)
                if (roomEmpty) _rooms.TryRemove(c, out _);
            }
        }

        // Nettoyer le pseudo
        _names.TryRemove(connectionId, out _);
    }

    /// <summary>
    /// Génère un code aléatoire et lisible pour un salon.
    /// Utilise un générateur cryptographiquement sécurisé pour garantir l'unicité.
    /// </summary>
    /// <param name="len">Longueur du code (recommandé: 6 caractères = 36^6 = 2 milliards de combinaisons)</param>
    /// <returns>Code généré (ex: "F9K7ZQ")</returns>
    private string GenerateCode(int len)
    {
        var bytes = new byte[len];
        _rng.GetBytes(bytes); // RNG cryptographique
        var chars = new char[len];
        for (int i = 0; i < len; i++)
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        return new(chars);
    }

    #endregion

    #region Initialisation de la partie

    /// <summary>
    /// Définit l'utilisateur et son deck pour un joueur dans le salon.
    /// Déclenche automatiquement l'initialisation de la partie quand les 2 joueurs ont défini leur deck.
    /// </summary>
    /// <param name="connectionId">ID de connexion du joueur</param>
    /// <param name="userId">ID utilisateur (base de données)</param>
    /// <param name="deckId">ID du deck sélectionné</param>
    /// <returns>true si l'opération a réussi, false si le joueur n'est pas dans un salon</returns>
    /// <remarks>
    /// LOGIQUE D'INITIALISATION:
    /// 1. Identifie si le joueur est User1 (créateur) ou User2 (rejoint)
    /// 2. Enregistre userId et deckId
    /// 3. Si les 2 joueurs ont défini leur deck ET que la partie n'est pas déjà initialisée:
    ///    - Crée le RoomObject
    ///    - Initialise les decks, champions, mains, etc. (via setUser1/setUser2)
    ///    - Marque IsGameInitialized = true
    /// 
    /// PATTERN LOCK-ASYNC:
    /// Les appels await sont HORS du lock pour éviter les deadlocks.
    /// Le lock ne contient que les opérations synchrones sur les champs.
    /// </remarks>
    public async Task<bool> SetPlayerDeck(string connectionId, Guid userId, Guid deckId)
    {
        var code = GetRoomOf(connectionId);
        if (code is null || !_rooms.TryGetValue(code, out var room)) return false;

        bool needsInitialization = false;
        Guid? user1Id = null, user2Id = null, deck1Id = null, deck2Id = null;

        // PHASE 1: Enregistrer les informations du joueur (lock court)
        lock (room)
        {
            var members = room.Members.ToList();
            if (!members.Contains(connectionId)) return false;

            // Déterminer si c'est le joueur 1 (créateur) ou 2 (rejoint)
            // Le premier membre dans la liste est toujours le créateur
            if (members[0] == connectionId)
            {
                room.User1Id = userId;
                room.User1DeckId = deckId;
            }
            else
            {
                room.User2Id = userId;
                room.User2DeckId = deckId;
            }

            // Vérifier si on peut initialiser la partie
            if (room.User1Id.HasValue && room.User2Id.HasValue && 
                room.User1DeckId.HasValue && room.User2DeckId.HasValue && 
                !room.IsGameInitialized)
            {
                // Préparer l'initialisation: copier les valeurs nécessaires
                needsInitialization = true;
                user1Id = room.User1Id.Value;
                user2Id = room.User2Id.Value;
                deck1Id = room.User1DeckId.Value;
                deck2Id = room.User2DeckId.Value;
                
                // Créer l'instance RoomObject (vide pour l'instant)
                room.GameRoom = new VortexTCG.Game.Object.Room();
            }
        }

        // PHASE 2: Initialiser la partie (HORS du lock car opérations async)
        if (needsInitialization && room.GameRoom != null)
        {
            // Ces appels initialisent les decks, champions, mains, boards, etc.
            await room.GameRoom.setUser1(user1Id!.Value, deck1Id!.Value);
            await room.GameRoom.setUser2(user2Id!.Value, deck2Id!.Value);
            
            // Marquer comme initialisé (lock court)
            lock (room)
            {
                room.IsGameInitialized = true;
            }
        }

        return true;
    }

    #endregion

    #region Accès à l'état de la partie

    /// <summary>
    /// Récupère le RoomObject (état complet de la partie) pour un code de salon.
    /// Le RoomObject contient: decks, mains, boards, champions, graveyards.
    /// </summary>
    /// <param name="code">Code du salon</param>
    /// <returns>RoomObject ou null si le salon n'existe pas ou n'est pas initialisé</returns>
    public VortexTCG.Game.Object.Room? GetGameRoom(string code)
    {
        code = code.Trim().ToUpperInvariant();
        return _rooms.TryGetValue(code, out var room) ? room.GameRoom : null;
    }

    /// <summary>
    /// Récupère le RoomObject à partir d'un ConnectionId.
    /// Utile dans les hubs SignalR où on a le ConnectionId mais pas le code.
    /// </summary>
    /// <param name="connectionId">ID de connexion du joueur</param>
    /// <returns>RoomObject ou null</returns>
    public VortexTCG.Game.Object.Room? GetGameRoomByConnection(string connectionId)
    {
        var code = GetRoomOf(connectionId);
        return code is not null ? GetGameRoom(code) : null;
    }

    /// <summary>
    /// Vérifie si la partie est prête (les 2 joueurs ont défini leur deck et la partie est initialisée).
    /// </summary>
    /// <param name="code">Code du salon</param>
    /// <returns>true si la partie peut commencer, false sinon</returns>
    public bool IsGameReady(string code)
    {
        code = code.Trim().ToUpperInvariant();
        return _rooms.TryGetValue(code, out var room) && room.IsGameInitialized;
    }

    /// <summary>
    /// Récupère les IDs des joueurs et de leurs decks pour un salon.
    /// Utile pour l'affichage, les logs, ou la validation.
    /// </summary>
    /// <param name="code">Code du salon</param>
    /// <returns>
    /// Tuple contenant:
    /// - user1Id: ID utilisateur du joueur 1 (créateur)
    /// - user2Id: ID utilisateur du joueur 2 (rejoint)
    /// - deck1Id: ID du deck du joueur 1
    /// - deck2Id: ID du deck du joueur 2
    /// Tous null si le salon n'existe pas.
    /// </returns>
    public (Guid? user1Id, Guid? user2Id, Guid? deck1Id, Guid? deck2Id) GetRoomPlayers(string code)
    {
        code = code.Trim().ToUpperInvariant();
        if (!_rooms.TryGetValue(code, out var room)) 
            return (null, null, null, null);
        
        lock (room)
            return (room.User1Id, room.User2Id, room.User1DeckId, room.User2DeckId);
    }

    #endregion
}
