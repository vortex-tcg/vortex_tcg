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
// 4. Gérer les phases et les tours de jeu
//
// ARCHITECTURE:
// - Chaque joueur (user_1 et user_2) possède:
//   * Un Deck (pioche de cartes)
//   * Une Hand (main de cartes)
//   * Un Board (terrain de jeu avec 5 emplacements)
//   * Un Champion (héros avec HP, gold, etc.)
//   * Un Graveyard (cimetière des cartes défaussées)
//
// PHASES DE JEU:
// - Draw: Pioche automatique d'une carte
// - Placement: Le joueur pose des cartes (attente manuelle)
// - Attack: Le joueur attaque (auto-skip si aucune action possible)
// - Defense: L'adversaire défend (auto-skip si aucune action possible)
// - EndTurn: Fin du tour, passage au joueur suivant
//
// INITIALISATION:
// Les méthodes setUser1() et setUser2() doivent être appelées pour charger
// les decks depuis la base de données et configurer les champions.
// =============================================
using VortexTCG.Game.DTO;

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

        #region État du tour et des phases

        /// <summary>ID du joueur dont c'est actuellement le tour</summary>
        private Guid _activePlayerId;

        /// <summary>Phase actuelle du tour</summary>
        private GamePhase _currentPhase;

        /// <summary>Numéro du tour actuel (commence à 1)</summary>
        private int _turnNumber;

        /// <summary>Indique si la partie a commencé</summary>
        private bool _gameStarted;

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
        public Task setUser1(Guid user, Guid deck)
        {
            _user_1 = user;

            // Charger les cartes du deck depuis la base de données
            _deck_user_1.initDeck(deck);

            // Configurer le champion (HP, gold, capacités)
            _champion_user_1.initChampion(deck);

            return Task.CompletedTask;
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
        public Task setUser2(Guid user, Guid deck)
        {
            _user_2 = user;

            // Charger les cartes du deck depuis la base de données
            _deck_user_2.initDeck(deck);

            // Configurer le champion (HP, gold, capacités)
            _champion_user_2.initChampion(deck);

            return Task.CompletedTask;
        }

        #endregion

        #region Actions de jeu

        /// <summary>
        /// Fait piocher des cartes à un joueur.
        /// </summary>
        /// <param name="playerId">ID du joueur qui pioche.</param>
        /// <param name="cardCount">Nombre de cartes à piocher.</param>
        /// <returns>Résultat pour le joueur et l'adversaire, ou null si joueur invalide.</returns>
        public DrawCardsResultDTO? DrawCards(Guid playerId, int cardCount)
        {
            if (playerId != _user_1 && playerId != _user_2) return null;
            if (cardCount <= 0) return null;

            bool isPlayer1 = playerId == _user_1;
            Deck deck = isPlayer1 ? _deck_user_1 : _deck_user_2;
            Hand hand = isPlayer1 ? _hand_user_1 : _hand_user_2;
            Champion champion = isPlayer1 ? _champion_user_1 : _champion_user_2;

            List<DrawnCardDTO> drawnCards = new List<DrawnCardDTO>();
            int fatigueCount = 0;
            int baseFatigue = champion.GetFatigue();

            for (int i = 0; i < cardCount; i++)
            {
                Card? card = deck.DrawCard();
                if (card != null)
                {
                    hand.AddCard(card);
                    drawnCards.Add(new DrawnCardDTO
                    {
                        GameCardId = card.GetGameCardId(),
                        Name = card.GetName(),
                        Hp = card.GetHp(),
                        Attack = card.GetAttack(),
                        Cost = card.GetCost(),
                        Description = card.GetDescription(),
                        CardType = card.GetCardType()
                    });
                }
                else
                {
                    fatigueCount++;
                    champion.ApplyFatigueDamage();
                }
            }

            return new DrawCardsResultDTO
            {
                PlayerResult = new DrawResultForPlayerDTO
                {
                    DrawnCards = drawnCards,
                    FatigueCount = fatigueCount,
                    BaseFatigue = baseFatigue
                },
                OpponentResult = new DrawResultForOpponentDTO
                {
                    PlayerId = playerId,
                    CardsDrawnCount = drawnCards.Count,
                    FatigueCount = fatigueCount,
                    BaseFatigue = baseFatigue
                }
            };
        }

        #endregion

        #region Propriétés publiques - État du tour

        /// <summary>ID du joueur dont c'est actuellement le tour</summary>
        public Guid ActivePlayerId => _activePlayerId;

        /// <summary>Phase actuelle du tour</summary>
        public GamePhase CurrentPhase => _currentPhase;

        /// <summary>Numéro du tour actuel</summary>
        public int TurnNumber => _turnNumber;

        /// <summary>Indique si la partie a commencé</summary>
        public bool GameStarted => _gameStarted;

        /// <summary>ID du joueur 1</summary>
        public Guid User1Id => _user_1;

        /// <summary>ID du joueur 2</summary>
        public Guid User2Id => _user_2;

        #endregion

        #region Gestion des phases et tours

        /// <summary>
        /// Démarre la partie. Le joueur 1 commence toujours.
        /// Initialise le tour 1 en phase Draw.
        /// </summary>
        /// <returns>DTO avec l'état initial de la partie</returns>
        public PhaseChangeResultDTO StartGame()
        {
            _gameStarted = true;
            _turnNumber = 1;
            _activePlayerId = _user_1;
            _currentPhase = GamePhase.Draw;

            return new PhaseChangeResultDTO
            {
                CurrentPhase = _currentPhase,
                ActivePlayerId = _activePlayerId,
                TurnNumber = _turnNumber,
                AutoChanged = false,
                AutoChangeReason = null,
                CanAct = true
            };
        }

        /// <summary>
        /// Change de phase pour le joueur actif.
        /// Gère les auto-skips selon les règles:
        /// - Draw: auto-skip après la pioche automatique
        /// - Placement: attente manuelle (le joueur doit appeler ChangePhase)
        /// - Attack: auto-skip si aucune carte sur le board ne peut attaquer
        /// - Defense: auto-skip si l'adversaire n'a pas de cartes pouvant défendre
        /// - EndTurn: passe au joueur suivant et retourne en Draw
        /// </summary>
        /// <param name="playerId">ID du joueur qui demande le changement</param>
        /// <returns>Résultat du changement de phase, ou null si non autorisé</returns>
        public PhaseChangeResultDTO? ChangePhase(Guid playerId)
        {
            // Seul le joueur actif peut changer de phase (sauf Defense où c'est l'adversaire)
            if (!_gameStarted) return null;

            // En phase Defense, c'est l'adversaire qui répond
            if (_currentPhase == GamePhase.Defense)
            {
                Guid defenderId = GetOpponentId(_activePlayerId);
                if (playerId != defenderId) return null;
            }
            else
            {
                if (playerId != _activePlayerId) return null;
            }

            return AdvancePhase();
        }

        /// <summary>
        /// Avance à la phase suivante avec gestion des auto-skips.
        /// </summary>
        /// <returns>Résultat du changement de phase</returns>
        private PhaseChangeResultDTO AdvancePhase()
        {
            GamePhase nextPhase = GetNextPhase(_currentPhase);
            bool autoChanged = false;
            string? autoChangeReason;

            // Gérer les auto-skips selon la phase
            while (ShouldAutoSkip(nextPhase, out autoChangeReason))
            {
                autoChanged = true;
                nextPhase = GetNextPhase(nextPhase);

                // Si on revient à Draw après EndTurn, on change de joueur
                if (nextPhase == GamePhase.Draw && _currentPhase != GamePhase.Draw)
                {
                    break;
                }
            }

            // Si on passe de EndTurn à Draw, c'est un nouveau tour
            if (_currentPhase == GamePhase.EndTurn ||
                (autoChanged && nextPhase == GamePhase.Draw))
            {
                SwitchActivePlayer();
                _turnNumber++;
            }

            _currentPhase = nextPhase;

            return new PhaseChangeResultDTO
            {
                CurrentPhase = _currentPhase,
                ActivePlayerId = _activePlayerId,
                TurnNumber = _turnNumber,
                AutoChanged = autoChanged,
                AutoChangeReason = autoChangeReason,
                CanAct = CanPlayerActInPhase(_currentPhase, _activePlayerId)
            };
        }

        /// <summary>
        /// Force le changement de phase (utilisé par le timer d'1 minute).
        /// </summary>
        /// <returns>Résultat du changement de phase forcé</returns>
        public PhaseChangeResultDTO ForceChangePhase()
        {
            if (!_gameStarted)
            {
                return new PhaseChangeResultDTO
                {
                    CurrentPhase = _currentPhase,
                    ActivePlayerId = _activePlayerId,
                    TurnNumber = _turnNumber,
                    AutoChanged = true,
                    AutoChangeReason = "Timeout - 1 minute écoulée",
                    CanAct = false
                };
            }

            PhaseChangeResultDTO result = AdvancePhase();
            result.AutoChanged = true;
            result.AutoChangeReason = "Timeout - 1 minute écoulée";
            return result;
        }

        /// <summary>
        /// Avance automatiquement de la phase Draw à Placement (après la pioche automatique).
        /// Appelé par le serveur après avoir effectué la pioche.
        /// </summary>
        /// <returns>Résultat du changement de phase</returns>
        public PhaseChangeResultDTO AdvanceFromDraw()
        {
            if (_currentPhase != GamePhase.Draw)
            {
                return new PhaseChangeResultDTO
                {
                    CurrentPhase = _currentPhase,
                    ActivePlayerId = _activePlayerId,
                    TurnNumber = _turnNumber,
                    AutoChanged = false,
                    AutoChangeReason = null,
                    CanAct = CanPlayerActInPhase(_currentPhase, _activePlayerId)
                };
            }

            // Passer directement à Placement
            _currentPhase = GamePhase.Placement;

            return new PhaseChangeResultDTO
            {
                CurrentPhase = _currentPhase,
                ActivePlayerId = _activePlayerId,
                TurnNumber = _turnNumber,
                AutoChanged = true,
                AutoChangeReason = "Pioche automatique effectuée",
                CanAct = true
            };
        }

        /// <summary>
        /// Vérifie si un joueur peut agir dans une phase donnée.
        /// </summary>
        private bool CanPlayerActInPhase(GamePhase phase, Guid activePlayer)
        {
            // En phase Defense, l'adversaire peut agir
            // Dans les autres phases, seul le joueur actif peut agir
            return phase != GamePhase.EndTurn;
        }

        /// <summary>
        /// Retourne la phase suivante dans le cycle.
        /// </summary>
        private GamePhase GetNextPhase(GamePhase current)
        {
            return current switch
            {
                GamePhase.Draw => GamePhase.Placement,
                GamePhase.Placement => GamePhase.Attack,
                GamePhase.Attack => GamePhase.Defense,
                GamePhase.Defense => GamePhase.EndTurn,
                GamePhase.EndTurn => GamePhase.Draw,
                _ => GamePhase.Draw
            };
        }

        /// <summary>
        /// Détermine si une phase doit être automatiquement skippée.
        /// </summary>
        /// <param name="phase">Phase à vérifier</param>
        /// <param name="reason">Raison du skip si applicable</param>
        /// <returns>True si la phase doit être skippée</returns>
        private bool ShouldAutoSkip(GamePhase phase, out string? reason)
        {
            reason = null;

            switch (phase)
            {
                case GamePhase.Draw:
                    // Le joueur doit piocher lui-même via DrawCards
                    // Auto-skip seulement si le joueur ne peut faire aucune action
                    // (pour l'instant, on considère qu'il peut toujours piocher ou subir la fatigue)
                    // Le joueur doit appeler ChangePhase après avoir pioché
                    return false;

                case GamePhase.Placement:
                    // Le joueur DOIT appeler ChangePhase manuellement
                    // Jamais auto-skip, même s'il n'a pas de cartes jouables
                    return false;

                case GamePhase.Attack:
                    // Auto-skip si aucune carte sur le board ne peut attaquer
                    if (!CanActivePlayerAttack())
                    {
                        reason = "Aucune carte ne peut attaquer";
                        return true;
                    }
                    return false;

                case GamePhase.Defense:
                    // Auto-skip si l'adversaire n'a pas de cartes pour défendre
                    // ou si le joueur actif n'a pas attaqué
                    if (!CanOpponentDefend())
                    {
                        reason = "Aucune carte ne peut défendre";
                        return true;
                    }
                    return false;

                case GamePhase.EndTurn:
                    // EndTurn est toujours exécuté, jamais skippé
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Vérifie si le joueur actif a des cartes pouvant attaquer.
        /// </summary>
        private bool CanActivePlayerAttack()
        {
            Board activeBoard = _activePlayerId == _user_1 ? _board_user_1 : _board_user_2;
            return activeBoard.HasAttackableCards();
        }

        /// <summary>
        /// Vérifie si l'adversaire peut défendre.
        /// </summary>
        private bool CanOpponentDefend()
        {
            Board opponentBoard = _activePlayerId == _user_1 ? _board_user_2 : _board_user_1;
            return opponentBoard.HasDefendableCards();
        }

        /// <summary>
        /// Change le joueur actif.
        /// </summary>
        private void SwitchActivePlayer()
        {
            _activePlayerId = _activePlayerId == _user_1 ? _user_2 : _user_1;
        }

        /// <summary>
        /// Retourne l'ID de l'adversaire d'un joueur.
        /// </summary>
        public Guid GetOpponentId(Guid playerId)
        {
            return playerId == _user_1 ? _user_2 : _user_1;
        }

        /// <summary>
        /// Vérifie si c'est le tour du joueur spécifié.
        /// </summary>
        public bool IsPlayerTurn(Guid playerId)
        {
            return _gameStarted && _activePlayerId == playerId;
        }

        /// <summary>
        /// Vérifie si un joueur peut agir dans la phase actuelle.
        /// </summary>
        public bool CanPlayerAct(Guid playerId)
        {
            if (!_gameStarted) return false;

            // En phase Defense, seul l'adversaire peut agir
            if (_currentPhase == GamePhase.Defense)
            {
                return playerId == GetOpponentId(_activePlayerId);
            }

            // Dans les autres phases, seul le joueur actif peut agir
            return playerId == _activePlayerId;
        }

        #endregion
    }
}