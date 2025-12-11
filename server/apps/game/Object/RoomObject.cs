// =============================================
// FICHIER: Object/RoomObject.cs
// =============================================
// RÔLE PRINCIPAL:
// Représente l'état complet d'une partie de jeu entre 2 joueurs.
// Toutes les données sont chargées en mémoire au départ (AUCUN appel DB pendant la partie).
//
// RESPONSABILITÉS:
// 1. Stocker l'état de jeu pour les 2 joueurs (deck, main, board, or, HP)
// 2. Initialiser les joueurs avec leurs decks chargés
// 3. Gérer les phases de jeu et les tours
//
// ARCHITECTURE:
// - Room: Conteneur principal avec Player1, Player2, tour actuel, phase
// - Player: État d'un joueur (deck, main, board, ressources)
// - CardInstance: Instance d'une carte (avec toutes ses données)
// - BoardUnit: Unité sur le plateau (carte factionnaire jouée)
// =============================================

namespace VortexTCG.Game.Object;

/// <summary>
/// État complet d'une partie en cours (tout est en mémoire).
/// Instance créée par RoomService quand les 2 joueurs ont sélectionné leurs decks.
/// </summary>
public class Room
{
    /// <summary>Joueur 1 (créateur du salon)</summary>
    public Player Player1 { get; set; } = null!;

    /// <summary>Joueur 2 (a rejoint le salon)</summary>
    public Player Player2 { get; set; } = null!;

    /// <summary>Joueur dont c'est le tour (1 ou 2)</summary>
    public int CurrentPlayer { get; set; } = 1;

    /// <summary>Numéro du tour actuel</summary>
    public int Turn { get; set; } = 1;

    /// <summary>
    /// Initialise le joueur 1 avec son deck (toutes les cartes sont chargées en mémoire).
    /// </summary>
    public void setUser1(Guid userId, Guid deckId, List<CardInstance> deckCards)
    {
        Player1 = new Player
        {
            UserId = userId,
            DeckId = deckId,
            Deck = new List<CardInstance>(deckCards),
            Hand = new List<CardInstance>(),
            Board = new BoardUnit?[7],
            Discard = new List<CardInstance>(),
            Gold = 1,
            MaxGold = 1,
            Health = 30
        };
        DrawCards(Player1, 3);
    }

    /// <summary>
    /// Initialise le joueur 2 avec son deck (toutes les cartes sont chargées en mémoire).
    /// </summary>
    public void setUser2(Guid userId, Guid deckId, List<CardInstance> deckCards)
    {
        Player2 = new Player
        {
            UserId = userId,
            DeckId = deckId,
            Deck = new List<CardInstance>(deckCards),
            Hand = new List<CardInstance>(),
            Board = new BoardUnit?[7],
            Discard = new List<CardInstance>(),
            Gold = 1,
            MaxGold = 1,
            Health = 30
        };
        DrawCards(Player2, 3);
    }

    private static void DrawCards(Player player, int count)
    {
        for (int i = 0; i < count && player.Deck.Count > 0; i++)
        {
            CardInstance card = player.Deck[0];
            player.Deck.RemoveAt(0);
            player.Hand.Add(card);
        }
    }
}

/// <summary>
/// État d'un joueur dans la partie.
/// Contient toutes ses cartes, ressources et informations de jeu.
/// </summary>
public class Player
{
    public Guid UserId { get; set; }
    public Guid DeckId { get; set; }
    public List<CardInstance> Deck { get; set; } = new();
    public List<CardInstance> Hand { get; set; } = new();
    public List<CardInstance> Discard { get; set; } = new();
    public BoardUnit?[] Board { get; set; } = new BoardUnit?[7];
    public int Gold { get; set; }
    public int MaxGold { get; set; }
    public int Health { get; set; }
}

/// <summary>
/// Instance d'une carte (avec TOUTES ses données chargées depuis la DB).
/// </summary>
public class CardInstance
{
    public Guid InstanceId { get; set; } = Guid.NewGuid();
    public Guid CardModelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CardType Type { get; set; }
    public int Cost { get; set; }
    public int? Attack { get; set; }
    public int? Defense { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> Effects { get; set; } = new();
}

public class BoardUnit
{
    public Guid InstanceId { get; set; }
    public Guid CardModelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CurrentAttack { get; set; }
    public int CurrentDefense { get; set; }
    public int Position { get; set; }
    public bool IsTapped { get; set; }
    public bool CanAttackThisTurn { get; set; }
    public List<string> ActiveEffects { get; set; } = new();
}

public enum CardType
{
    Faction = 1,
    Equipment = 2,
    Spell = 3
}
