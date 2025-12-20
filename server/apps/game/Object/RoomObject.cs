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
using System.Timers;

namespace VortexTCG.Game.Object
{
    public class Room
    {
        #region Identifiants des joueurs

            private Guid _user_1;
            private Guid _user_2;

        #endregion

        #region État du tour et des phases

            private Guid _activePlayerId;
            private GamePhase _currentPhase;
            private int _turnNumber;
            private bool _gameStarted;
            private bool _isActif = false;
            private System.Timers.Timer _timer;
            public event Action OnTimeUp;

        #endregion

        #region État de jeu - Joueur 1

            private Deck _deck_user_1;
            private Hand _hand_user_1;
            private Champion _champion_user_1;
            private Graveyard _graveyard_user_1;
            private Board _board_user_1;

        #endregion

        #region État de jeu - Joueur 2

            private Deck _deck_user_2;
            private Hand _hand_user_2;
            private Champion _champion_user_2;
            private Graveyard _graveyard_user_2;
            private Board _board_user_2;

        #endregion

        #region Constructeur

            public Room()
            {
                _deck_user_1 = new Deck();
                _deck_user_2 = new Deck();

                _hand_user_1 = new Hand();
                _hand_user_2 = new Hand();

                _champion_user_1 = new Champion();
                _champion_user_2 = new Champion();

                _graveyard_user_1 = new Graveyard();
                _graveyard_user_2 = new Graveyard();

                _board_user_1 = new Board();
                _board_user_2 = new Board();
            
                _timer = new System.Timers.Timer(60000); 
                _timer.Elapsed += HandleTimerElapsed;
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
                _deck_user_1.initDeck(deck);
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
                _deck_user_2.initDeck(deck);
                _champion_user_2.initChampion(deck);

                return Task.CompletedTask;
            }

        #endregion

        #region Actions de pioche

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

        #region Gestion des phases et tours

            /// <summary>
            /// Handle when user don't play during 1 minute
            /// </summary>
            public void Start(int seconds)
            {
                _timer.Interval = seconds * 1000;
                _timer.Start();
            }
            public void Stop(){
                _timer.Stop();
            }
            private void HandleTimerElapsed(object? sender, ElapsedEventArgs e)
            {
                OnTimeUp?.Invoke(); 
            }

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
                _currentPhase = GamePhase.PLACEMENT;
                _isActif = false;

                DrawCards(_activePlayerId, 1);

                Start(60);
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
            public PhaseChangeResultDTO GetState()
            {
                return new PhaseChangeResultDTO
                {
                    CurrentPhase = _currentPhase,
                    ActivePlayerId = _activePlayerId,
                    TurnNumber = _turnNumber,
                    AutoChanged = false,
                    AutoChangeReason = null,
                    CanAct = _gameStarted
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
                if (_currentPhase == GamePhase.DEFENSE) {
                    if (playerId != _activePlayerId) return null; 
                }
                else if (playerId != _activePlayerId) return null;

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

                if (ShouldAutoSkip(nextPhase, out autoChangeReason)) {
                    autoChanged = true;
                    nextPhase = GetNextPhase(nextPhase);
                }

                if (_currentPhase == GamePhase.ATTACK)
                {
                    SwitchActivePlayer();
                } 
                
                _currentPhase = nextPhase;
               if (_currentPhase == GamePhase.PLACEMENT) 
                {
                    if (_activePlayerId == _user_1) 
                    {
                        _turnNumber += 1;
                    }

                    DrawCards(_activePlayerId, 1);
                }
                HandleChangePhaseEvent();

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

       public void HandleChangePhaseEvent()
        {
            Stop(); 

            if (_currentPhase == GamePhase.END_TURN)
            {
                ResolveBattle();
            }
            Start(60);
        }
        /// <summary>
        /// Résout le combat entre les deux joueurs.
        /// </summary>
        private void ResolveBattle()
        {    
            Console.WriteLine("Résolution du combat en cours...");
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
            /// Vérifie si un joueur peut agir dans une phase donnée.
            /// </summary>
            private bool CanPlayerActInPhase(GamePhase phase, Guid activePlayer)
            {
                // En phase Defense, l'adversaire peut agir
                // Dans les autres phases, seul le joueur actif peut agir
                return phase != GamePhase.END_TURN;
            }

            /// <summary>
            /// Retourne la phase suivante dans le cycle.
            /// </summary>
            private GamePhase GetNextPhase(GamePhase current)
            {
                return current switch
                {
                    GamePhase.PLACEMENT => GamePhase.ATTACK,
                    GamePhase.ATTACK => GamePhase.DEFENSE,
                    GamePhase.DEFENSE => GamePhase.END_TURN,
                    GamePhase.END_TURN => GamePhase.PLACEMENT,
                    _ => GamePhase.PLACEMENT
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

                    case GamePhase.PLACEMENT:
                        // Le joueur DOIT appeler ChangePhase manuellement
                        // Jamais auto-skip, même s'il n'a pas de cartes jouables
                        return false;

                    case GamePhase.ATTACK:
                        // Auto-skip si aucune carte sur le board ne peut attaquer
                        if (!CanActivePlayerAttack())
                        {
                            reason = "Aucune carte ne peut attaquer";
                            return true;
                        }
                        return false;

                    case GamePhase.DEFENSE:
                        // Auto-skip si l'adversaire n'a pas de cartes pour défendre
                        // ou si le joueur actif n'a pas attaqué
                        if (!CanOpponentDefend())
                        {
                            reason = "Aucune carte ne peut défendre";
                            return true;
                        }
                        return false;

                    case GamePhase.END_TURN:
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
                if (_currentPhase == GamePhase.DEFENSE)
                {
                    return playerId == GetOpponentId(_activePlayerId);
                }

                // Dans les autres phases, seul le joueur actif peut agir
                return playerId == _activePlayerId;
            }

        #endregion
    }
}