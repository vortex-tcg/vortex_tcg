// =============================================
// FICHIER: Object/RoomObject.cs
// =============================================
// RÔLE PRINCIPAL:
// Représente l'état complet d'une partie de jeu entre 2 joueurs.
// Contient tous les éléments de jeu pour chaque joueur: deck, main, board, champion, cimetière.
//
// RESPONSABILITÉS:
// 1. Stocker l'état de jeu pour les 2 joueurs
// 2. Initialiser les decks et champions à partir des IDs de base de données
// 3. Fournir un accès centralisé à tous les composants de jeu
//
// ARCHITECTURE:
// - Chaque joueur (user_1 et user_2) possède:
//   * Un Deck (pioche de cartes)
//   * Une Hand (main de cartes)
//   * Un Board (terrain de jeu avec 5 emplacements)
//   * Un Champion (héros avec HP, gold, etc.)
//   * Un Graveyard (cimetière des cartes défaussées)
//
// INITIALISATION:
// Les méthodes setUser1() et setUser2() doivent être appelées pour charger
// les decks depuis la base de données et configurer les champions.
// =============================================

namespace VortexTCG.Game.Object
{
    /// <summary>
    /// Représente l'état complet d'une partie de jeu (match) entre 2 joueurs.
    /// Contient tous les éléments de gameplay pour chaque joueur.
    /// Instance créée par RoomService quand les 2 joueurs sont prêts.
    /// </summary>
    public class Room
    {
        #region Identifiants des joueurs

        /// <summary>ID utilisateur (base de données) du joueur 1 (créateur du salon)</summary>
        private Guid _user_1;
        
        /// <summary>ID utilisateur (base de données) du joueur 2 (a rejoint le salon)</summary>
        private Guid _user_2;

        #endregion

        #region État de jeu - Joueur 1

        /// <summary>Deck (pioche) du joueur 1 - Contient toutes les cartes non encore piochées</summary>
        private Deck _deck_user_1;

        /// <summary>Main du joueur 1 - Cartes actuellement disponibles pour être jouées</summary>
        private Hand _hand_user_1;

        /// <summary>Champion du joueur 1 - Héros avec HP, gold, capacités spéciales</summary>
        private Champion _champion_user_1;

        /// <summary>Cimetière du joueur 1 - Cartes défaussées ou détruites</summary>
        private Graveyard _graveyard_user_1;

        /// <summary>Plateau de jeu du joueur 1 - 5 emplacements pour poser des cartes</summary>
        private Board _board_user_1;

        #endregion

        #region État de jeu - Joueur 2

        /// <summary>Deck (pioche) du joueur 2 - Contient toutes les cartes non encore piochées</summary>
        private Deck _deck_user_2;

        /// <summary>Main du joueur 2 - Cartes actuellement disponibles pour être jouées</summary>
        private Hand _hand_user_2;

        /// <summary>Champion du joueur 2 - Héros avec HP, gold, capacités spéciales</summary>
        private Champion _champion_user_2;

        /// <summary>Cimetière du joueur 2 - Cartes défaussées ou détruites</summary>
        private Graveyard _graveyard_user_2;

        /// <summary>Plateau de jeu du joueur 2 - 5 emplacements pour poser des cartes</summary>
        private Board _board_user_2;

        #endregion

        #region Constructeur

        /// <summary>
        /// Constructeur: Initialise tous les composants de jeu vides pour les 2 joueurs.
        /// Les decks et champions seront chargés via setUser1() et setUser2().
        /// </summary>
        public Room()
        {
            // Initialiser les decks (vides pour l'instant, seront remplis par initDeck)
            _deck_user_1 = new Deck();
            _deck_user_2 = new Deck();

            // Initialiser les mains (vides au départ)
            _hand_user_1 = new Hand();
            _hand_user_2 = new Hand();

            // Initialiser les champions (stats par défaut, seront configurés par initChampion)
            _champion_user_1 = new Champion();
            _champion_user_2 = new Champion();

            // Initialiser les cimetières (vides)
            _graveyard_user_1 = new Graveyard();
            _graveyard_user_2 = new Graveyard();

            // Initialiser les boards (5 emplacements vides chacun)
            _board_user_1 = new Board();
            _board_user_2 = new Board();
        }

        #endregion

        #region Méthodes d'initialisation

        /// <summary>
        /// Configure le joueur 1 avec son deck et son champion.
        /// Charge les cartes depuis la base de données et initialise le champion.
        /// </summary>
        /// <param name="user">ID utilisateur (base de données) du joueur 1</param>
        /// <param name="deck">ID du deck sélectionné par le joueur 1</param>
        /// <remarks>
        /// APPELÉ PAR: RoomService.SetPlayerDeck() quand les 2 joueurs sont prêts.
        /// EFFETS:
        /// - Enregistre l'ID utilisateur
        /// - Charge les cartes du deck depuis la BDD (via DeckFactory)
        /// - Configure le champion avec ses stats de base (30 HP, 1 gold, etc.)
        /// </remarks>
        public async Task setUser1(Guid user, Guid deck)
        {
            _user_1 = user;
            
            // Charger les cartes du deck depuis la base de données
            _deck_user_1.initDeck(deck);
            
            // Configurer le champion (HP, gold, capacités)
            _champion_user_1.initChampion(deck);
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