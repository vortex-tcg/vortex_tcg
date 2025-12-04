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

    /// <summary>Phase de jeu actuelle</summary>
    public GamePhase Phase { get; set; } = GamePhase.Draw;

    /// <summary>
    /// Initialise le joueur 1 avec son deck (toutes les cartes sont chargées en mémoire).
    /// </summary>
    /// <param name="userId">ID utilisateur du joueur 1</param>
    /// <param name="deckId">ID du deck sélectionné</param>
    /// <param name="deckCards">Liste de TOUTES les cartes du deck (déjà chargées depuis la DB)</param>
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
        
        // Piocher la main initiale (3 cartes)
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
        
        // Piocher la main initiale (3 cartes)
        DrawCards(Player2, 3);
    }

    /// <summary>
    /// Pioche N cartes pour un joueur (retire du Deck et ajoute à Hand).
    /// </summary>
    private void DrawCards(Player player, int count)
    {
        for (int i = 0; i < count && player.Deck.Count > 0; i++)
        {
            var card = player.Deck[0];
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
    /// <summary>ID utilisateur (permanent, survit aux reconnexions)</summary>
    public Guid UserId { get; set; }

    /// <summary>ID du deck sélectionné</summary>
    public Guid DeckId { get; set; }

    /// <summary>Pioche (cartes non encore piochées) - TOUTES les données sont chargées</summary>
    public List<CardInstance> Deck { get; set; } = new();

    /// <summary>Main (cartes disponibles pour être jouées)</summary>
    public List<CardInstance> Hand { get; set; } = new();

    /// <summary>Défausse (cartes jouées ou défaussées)</summary>
    public List<CardInstance> Discard { get; set; } = new();

    /// <summary>Plateau de jeu (7 positions pour les unités)</summary>
    public BoardUnit?[] Board { get; set; } = new BoardUnit?[7];

    /// <summary>Or disponible ce tour</summary>
    public int Gold { get; set; }

    /// <summary>Or maximum (augmente chaque tour)</summary>
    public int MaxGold { get; set; }

    /// <summary>Points de vie du joueur</summary>
    public int Health { get; set; }
}

/// <summary>
/// Instance d'une carte (avec TOUTES ses données chargées depuis la DB).
/// Chaque carte dans le deck est une instance unique avec son propre ID.
/// </summary>
public class CardInstance
{
    /// <summary>ID unique de cette instance de carte (généré)</summary>
    public Guid InstanceId { get; set; } = Guid.NewGuid();

    /// <summary>ID du modèle de carte (référence au CardModel en DB)</summary>
    public Guid CardModelId { get; set; }

    /// <summary>Nom de la carte</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Type de carte (Faction, Equipment, Spell)</summary>
    public CardType Type { get; set; }

    /// <summary>Coût en or pour jouer la carte</summary>
    public int Cost { get; set; }

    /// <summary>Points d'attaque (pour les unités)</summary>
    public int? Attack { get; set; }

    /// <summary>Points de défense (pour les unités)</summary>
    public int? Defense { get; set; }

    /// <summary>Description textuelle de la carte</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Liste des effets de la carte (noms des effets)</summary>
    public List<string> Effects { get; set; } = new();
}

/// <summary>
/// Unité sur le plateau (carte factionnaire jouée).
/// Représente l'état actuel d'une unité en jeu.
/// </summary>
public class BoardUnit
{
    /// <summary>ID de l'instance de carte d'origine</summary>
    public Guid InstanceId { get; set; }

    /// <summary>ID du modèle de carte (référence)</summary>
    public Guid CardModelId { get; set; }

    /// <summary>Nom de l'unité</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Attaque actuelle (peut être modifiée par des effets)</summary>
    public int CurrentAttack { get; set; }

    /// <summary>Défense actuelle (peut être modifiée par des effets)</summary>
    public int CurrentDefense { get; set; }

    /// <summary>Position sur le plateau (0-6)</summary>
    public int Position { get; set; }

    /// <summary>A déjà attaqué ce tour ?</summary>
    public bool IsTapped { get; set; }

    /// <summary>Peut attaquer ce tour ? (false = summoning sickness)</summary>
    public bool CanAttackThisTurn { get; set; }

    /// <summary>Effets actifs sur cette unité</summary>
    public List<string> ActiveEffects { get; set; } = new();
}

/// <summary>
/// Types de cartes disponibles dans le jeu.
/// </summary>
public enum CardType
{
    Faction = 1,    // Unité combattante
    Equipment = 2,  // Équipement pour unité
    Spell = 3       // Sortilège à effet immédiat
}

/// <summary>
/// Phases d'un tour de jeu.
/// </summary>
public enum GamePhase
{
    Draw,   // Phase de pioche
    Main,   // Phase principale (jouer des cartes)
    Combat, // Phase de combat
    End     // Phase de fin de tour
}