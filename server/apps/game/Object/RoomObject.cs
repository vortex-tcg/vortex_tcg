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
    /// Énumération des différentes phases de jeu dans un tour.
    /// Chaque joueur passe par ces phases dans l'ordre lors de son tour.
    /// </summary>
    /// <remarks>
    /// FLOW DU JEU:
    /// Draw → Placement → Attack → Defense → EndTurn (puis tour du joueur suivant)
    /// 
    /// RÈGLES SPÉCIALES:
    /// - Draw: Le serveur change automatiquement si aucune action possible
    /// - Placement: Le joueur DOIT appeler changePhase() manuellement
    /// - Attack/Defense: Le serveur change automatiquement si aucune action possible
    /// - EndTurn: Timeout de 1 minute, puis changement automatique
    /// </remarks>
    public enum GamePhase
    {
        /// <summary>Phase de pioche - Le joueur pioche une ou plusieurs cartes</summary>
        DrawCard = 0,

        /// <summary>Phase de placement - Le joueur peut placer des cartes sur le board</summary>
        PlayCard = 1,

        /// <summary>Phase d'attaque - Le joueur peut attaquer avec ses cartes</summary>
        AttackWithCard = 2,

        /// <summary>Phase de défense - Le joueur adverse peut défendre</summary>
        DefenseWithCard = 3,

        /// <summary>Phase de fin de tour - Transition vers le joueur suivant</summary>
        EndTurn = 4
    }

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
            await _deck_user_1.initDeck(deck);

            // Configurer le champion (HP, gold, capacités)
            await _champion_user_1.initChampion(deck);
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
            await _deck_user_2.initDeck(deck);

            // Configurer le champion (HP, gold, capacités)
            await _champion_user_2.initChampion(deck);
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
        #region Phase Management

        /// <summary>
        /// Indique quel joueur est actuellement actif.
        /// 1 = _user_1 (créateur du salon), 2 = _user_2 (a rejoint le salon)
        /// </summary>
        private int _currentPlayer = 1;

        /// <summary>
        /// Phase actuelle du jeu selon l'énumération GamePhase.
        /// Détermine quelles actions le joueur actuel peut effectuer.
        /// </summary>
        private GamePhase _currentPhase = GamePhase.DrawCard;

        /// <summary>
        /// Timestamp du début de la phase actuelle (UTC).
        /// Utilisé pour implémenter le timeout de 1 minute sur les phases.
        /// </summary>
        private DateTime _phaseStartTime = DateTime.UtcNow;

        /// <summary>
        /// Indique si le serveur attend un appel manuel à ChangePhase().
        /// true = Phase de placement (le joueur doit manuellement terminer)
        /// false = Phases automatiques (serveur peut changer si aucune action possible)
        /// </summary>
        private bool _waitingForManualPhaseChange = false;

        /// <summary>
        /// Change la phase actuelle du jeu selon les règles définies.
        /// Gère les transitions automatiques et manuelles entre les phases.
        /// </summary>
        /// <param name="userId">ID du joueur qui demande le changement (pour validation des droits)</param>
        /// <param name="isManual">true = changement demandé par le joueur, false = changement automatique du serveur</param>
        /// <returns>true si le changement a réussi, false si refusé (mauvais joueur, phase incorrecte, etc.)</returns>
        /// <remarks>
        /// RÈGLES DE TRANSITION:
        /// - Draw → Placement: Toujours automatique
        /// - Placement → Attack: SEULEMENT manuel (le joueur doit appeler)
        /// - Attack → Defense: Automatique ou manuel
        /// - Defense → EndTurn: Automatique ou manuel
        /// - EndTurn → Draw (joueur suivant): Automatique ou timeout
        /// 
        /// VALIDATION:
        /// - Vérifie que c'est le tour du bon joueur
        /// - Respecte les contraintes manuelles/automatiques
        /// - Réinitialise le timer à chaque changement
        /// </remarks>
        public bool ChangePhase(Guid userId, bool isManual = true)
        {
            // Étape 1: Vérifier que c'est le tour du bon joueur
            if (!IsCurrentPlayer(userId))
            {
                return false; // Ce n'est pas le tour de ce joueur
            }

            // Étape 2: Gérer le changement selon la phase actuelle
            switch (_currentPhase)
            {
                case GamePhase.DrawCard:
                    // Phase de pioche: Transition automatique vers Placement
                    _currentPhase = GamePhase.PlayCard;
                    _waitingForManualPhaseChange = true; // La phase placement nécessite un appel manuel
                    break;

                case GamePhase.PlayCard:
                    // Phase de placement: SEULEMENT changement manuel autorisé
                    if (!isManual)
                    {
                        return false; // Le serveur ne peut pas changer cette phase automatiquement
                    }
                    _currentPhase = GamePhase.AttackWithCard;
                    _waitingForManualPhaseChange = false; // Les phases suivantes peuvent être automatiques
                    break;

                case GamePhase.AttackWithCard:
                    // Phase d'attaque: Transition vers défense
                    _currentPhase = GamePhase.DefenseWithCard;
                    break;

                case GamePhase.DefenseWithCard:
                    // Phase de défense: Transition vers fin de tour
                    _currentPhase = GamePhase.EndTurn;
                    break;

                case GamePhase.EndTurn:
                    // Fin de tour: Passer au joueur suivant et recommencer le cycle
                    _currentPlayer = (_currentPlayer == 1) ? 2 : 1; // Alterner entre joueur 1 et 2
                    _currentPhase = GamePhase.DrawCard; // Recommencer le cycle des phases
                    _waitingForManualPhaseChange = false;
                    break;
            }

            // Étape 3: Réinitialiser le timer pour la nouvelle phase
            _phaseStartTime = DateTime.UtcNow;
            return true; // Changement réussi
        }

        /// <summary>
        /// Vérifie si l'utilisateur spécifié est le joueur actuellement actif.
        /// </summary>
        /// <param name="userId">ID de l'utilisateur à vérifier</param>
        /// <returns>true si c'est son tour, false sinon</returns>
        /// <remarks>
        /// LOGIQUE:
        /// - Si _currentPlayer == 1, alors seul _user_1 peut jouer
        /// - Si _currentPlayer == 2, alors seul _user_2 peut jouer
        /// - Utilisé pour valider les droits avant les actions
        /// </remarks>
        private bool IsCurrentPlayer(Guid userId)
        {
            return (_currentPlayer == 1 && userId == _user_1) ||
                   (_currentPlayer == 2 && userId == _user_2);
        }

        /// <summary>
        /// Vérifie si la phase actuelle a dépassé le timeout de 1 minute.
        /// </summary>
        /// <returns>true si la phase a expiré, false sinon</returns>
        /// <remarks>
        /// UTILISATION:
        /// - Appelé périodiquement par le serveur
        /// - Si true, le serveur peut forcer un changement de phase
        /// - Sauf pour la phase de placement qui nécessite toujours un appel manuel
        /// </remarks>
        public bool IsPhaseExpired()
        {
            TimeSpan elapsed = DateTime.UtcNow - _phaseStartTime;
            return elapsed.TotalMinutes >= 1.0; // Timeout de 1 minute
        }

        /// <summary>
        /// Détermine si le serveur peut changer automatiquement la phase actuelle.
        /// </summary>
        /// <returns>true si changement automatique autorisé, false si appel manuel requis</returns>
        /// <remarks>
        /// RÈGLES:
        /// - Placement: Toujours false (appel manuel obligatoire)
        /// - Autres phases: true si aucune action possible OU timeout atteint
        /// </remarks>
        public bool CanServerChangePhase()
        {
            // La phase de placement nécessite toujours un appel manuel
            if (_currentPhase == GamePhase.PlayCard)
            {
                return false;
            }

            // Pour les autres phases: autoriser si timeout atteint
            return IsPhaseExpired();
        }

        /// <summary>
        /// Récupère des informations sur l'état actuel des phases pour debugging/logging.
        /// </summary>
        /// <returns>Chaîne décrivant l'état actuel des phases</returns>
        public string GetPhaseInfo()
        {
            TimeSpan elapsed = DateTime.UtcNow - _phaseStartTime;
            return $"Joueur {_currentPlayer}, Phase: {_currentPhase}, Temps écoulé: {elapsed.TotalSeconds:F1}s, Manuel requis: {_waitingForManualPhaseChange}";
        }

        #endregion
    }

}