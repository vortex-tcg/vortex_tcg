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
        /// Configure le joueur 2 avec son deck et son champion.
        /// Charge les cartes depuis la base de données et initialise le champion.
        /// </summary>
        /// <param name="user">ID utilisateur (base de données) du joueur 2</param>
        /// <param name="deck">ID du deck sélectionné par le joueur 2</param>
        /// <remarks>
        /// APPELÉ PAR: RoomService.SetPlayerDeck() quand les 2 joueurs sont prêts.
        /// EFFETS:
        /// - Enregistre l'ID utilisateur
        /// - Charge les cartes du deck depuis la BDD (via DeckFactory)
        /// - Configure le champion avec ses stats de base (30 HP, 1 gold, etc.)
        /// </remarks>
        public async Task setUser2(Guid user, Guid deck)
        {
            _user_2 = user;
            
            // Charger les cartes du deck depuis la base de données
            _deck_user_2.initDeck(deck);
            
            // Configurer le champion (HP, gold, capacités)
            _champion_user_2.initChampion(deck);
        }

        #endregion

        // =============================================
        // TODO: Ajouter les méthodes de gameplay ici
        // =============================================
        // Exemples de méthodes à implémenter:
        // - DrawCard(int playerId) : Piocher une carte
        // - PlayCard(int playerId, Card card) : Jouer une carte
        // - AttackWithCard(int playerId, Card attacker, Card target) : Attaquer
        // - EndTurn(int playerId) : Terminer le tour
        // - GetGameState() : Récupérer l'état complet pour l'UI
        // =============================================
    }    
}